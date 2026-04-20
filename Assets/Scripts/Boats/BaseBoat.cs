using System;
using System.Collections.Generic;
using Audio;
using Audio.SoundFX;
using NaughtyAttributes;
using Prototypes.Alex.Utilities;
using UnityEngine;
using Utilities.Debugging;

namespace Prototypes.Alex.Boats
{
    [RequireComponent(typeof(BoatFlagHoist))]
    public class BaseBoat : MonoBehaviour
    {
        enum STATE
        {
            NONE,
            AWAITING_PLAYER_FLAGS,
            RESPONDING,
            MOVING_TO_PORT,
            MOVING_TO_DOCK,
            LEAVING_PORT
        }
        
        public static readonly List<BaseBoat> AllBoats = new List<BaseBoat>();
        //public static event Action<BaseBoat> OnDocked;
        public static event Action OnNoMoreBoats;
        
        public FLAG CargoType => cargoType;
        public FLAG ShipType => shipType;
        public bool IsDocked => isDocked;
        
        [SerializeField]
        private Renderer[] flagRenderers;
        
        [SerializeField, ReadOnly] 
        private FLAG shipType;
        [SerializeField, ReadOnly] 
        private FLAG cargoType;
        [SerializeField, ReadOnly] 
        private bool isDocked;

        private static DockManager s_dockManager;
        
        private Transform m_moveTarget;
        private FLAG dockTarget;

        //================================================================================================================//
        
        [SerializeField]
        private float boatMoveSpeed;
        
        [SerializeField]
        private STATE startingState;
        
        [SerializeField, ReadOnly]
        private STATE state;

        private Vector3 m_startPosition;
        private bool m_approvedDocking;
        private bool m_didHailPort;
        
        [SerializeField]
        private BoatFlagHoist boatFlagHoist;
        
        [SerializeField]
        private float minDistanceThreshold;

        [SerializeField, ReadOnly]
        private bool inRangeOfTower;

        //================================================================================================================//
        
        public void Init(FLAG shipType, FLAG cargoType)
        {
            if(s_dockManager == null)
                s_dockManager = FindAnyObjectByType<DockManager>();
            
            this.shipType = shipType;
            this.cargoType = cargoType;
            isDocked = false;

            var mat = flagRenderers[0].materials[1];
            mat.mainTexture = shipType.GetTexture();
            for (var i = 1; i < flagRenderers.Length; i++)
            {
                var flagRenderer = flagRenderers[i];
                flagRenderer.sharedMaterials[1] = mat;
                flagRenderer.sharedMaterials[1].mainTexture = shipType.GetTexture();
            }

            m_moveTarget = s_dockManager.portEntranceTransform;
            
            boatFlagHoist.ProcessCommunicationForMe = ProcessCommunicationForMe;
            boatFlagHoist.boatType = shipType;
            
            SFX.BELL_RING.PlaySound();
        }

        private void OnDestroy()
        {
            AllBoats.Remove(this);
            
            if(AllBoats.Count == 0)
                OnNoMoreBoats?.Invoke();
        }
        //================================================================================================================//

        private void Start()
        {
            m_startPosition = transform.position;
            AllBoats.Add(this);

            SetState(startingState);
        }
        
        private void Update()
        {
            switch (state)
            {
                case STATE.NONE:
                    break;
                case STATE.MOVING_TO_PORT:
                {
                    MoveTowards(m_moveTarget.position);

                    var planar = Vector3.ProjectOnPlane(transform.position - m_moveTarget.position, Vector3.up);
                    var distance = planar.magnitude;
                    if (distance < minDistanceThreshold)
                    {
                        inRangeOfTower = true;

                        //If arriving in range, and player flags are visible, react to it 
                        if (boatFlagHoist.PlayerHasFlagsForMe(out var results) &&
                            ProcessCommunicationForMe(results))
                            break;

                        if (distance < 1f)
                        {
                            //Otherwise just move towards the docks
                            SetState(STATE.MOVING_TO_DOCK);
                        }
                    }

                    break;
                }
                case STATE.MOVING_TO_DOCK:
                {
                    MoveTowards(m_moveTarget.position);
                    var planar = Vector3.ProjectOnPlane(transform.position - m_moveTarget.position, Vector3.up);
                    var distance = planar.magnitude;
                    if (distance < 10f)
                    {
                        if (s_dockManager.RequestToDock(dockTarget, this))
                            isDocked = true;
                        SetState(STATE.NONE);
                    }

                    break;
                }
                case STATE.AWAITING_PLAYER_FLAGS:
                    break;
                case STATE.RESPONDING:
                    break;
                case STATE.LEAVING_PORT:
                {
                    MoveTowards(m_startPosition);
                    var planar = Vector3.ProjectOnPlane(transform.position - m_startPosition, Vector3.up);
                    var distance = planar.magnitude;
                    if (distance < 10f)
                    {
                        SetState(STATE.NONE);
                        Destroy(gameObject);
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return;
            void MoveTowards(Vector3 target)
            {
                var pos = Vector3.MoveTowards(transform.position, target, Time.deltaTime * boatMoveSpeed);
                var dir = (pos - transform.position).normalized;

                transform.position = pos;
                if (dir != Vector3.zero)
                    transform.forward = dir;
            }
        }

        //================================================================================================================//

        private void SetState(STATE newState)
        {
            LeavingState(state);
            state = newState;
            switch (state)
            {
                case STATE.MOVING_TO_DOCK when !m_approvedDocking:
                    dockTarget = s_dockManager.GetRandomDockAvailableDock();
                    m_moveTarget = s_dockManager.GetDockTransform(dockTarget);
                    
                    boatFlagHoist.StartListeningForPlayerFlags();
                    break;
                case STATE.AWAITING_PLAYER_FLAGS:
                    boatFlagHoist.StartListeningForPlayerFlags();
                    //If there hasn't already been comms with the player, do so here
                    if (!m_didHailPort)
                    {
                        m_didHailPort = true;
                        boatFlagHoist.HoistFlags(new List<FLAG>()
                        {
                            FLAG.HELLO,
                            cargoType
                        });
                    }
                    break;
            }
        }
        private void LeavingState(STATE oldState)
        {
            switch (oldState)
            {
                case STATE.MOVING_TO_DOCK:
                case STATE.AWAITING_PLAYER_FLAGS:
                    boatFlagHoist.StopListeningForPlayerFlags();
                    break;
            }
        }
        
        internal bool ProcessCommunicationForMe(BoatFlagHoist.FlagCommsResults flagCommsResults)
        {
            //If the message is for me but its not valid, notify the player
            if (!flagCommsResults.ShouldIgnore && !flagCommsResults.IsValid)
            {
                boatFlagHoist.RemoveFlags();
                boatFlagHoist.HoistFlags(new List<FLAG> { FLAG.UNCLEAR });
                return false;
            }
            
            //TODO Add the loading response setup
            //TODO Respond with the correct action

            switch (flagCommsResults.Action)
            {
                case FLAG.DENIED_ENTRY:
                    SetState(STATE.LEAVING_PORT);
                    return true;
                case FLAG.STOP when inRangeOfTower:
                    SetState(STATE.AWAITING_PLAYER_FLAGS);
                    return true;
                case FLAG.MOVE_TO_A when inRangeOfTower && state != STATE.MOVING_TO_DOCK:
                case FLAG.MOVE_TO_B when inRangeOfTower && state != STATE.MOVING_TO_DOCK:
                case FLAG.MOVE_TO_C when inRangeOfTower && state != STATE.MOVING_TO_DOCK:
                    m_approvedDocking = true;
                    dockTarget = flagCommsResults.Action;
                    m_moveTarget = s_dockManager.GetDockTransform(dockTarget);
                    SetState(STATE.MOVING_TO_DOCK);
                    boatFlagHoist.HoistFlags(new List<FLAG>() {cargoType, dockTarget });
                    return true;
            }
            
            return false;
        }

        public static bool HasActiveBoat(FLAG shipType, FLAG cargoType)
        {
            for (int i = 0; i < AllBoats.Count; i++)
            {
                var boat = AllBoats[i];
                if (boat.shipType == shipType && !boat.isDocked)
                    return true;
            }

            return false;
        }

        public static void CleanBoats()
        {
            for (int i = AllBoats.Count - 1; i >= 0 ; i--)
            {
                Destroy(AllBoats[i].gameObject);
            }
        }
        
        //Unity Editor Functions
        //================================================================================================================//
        private void OnDrawGizmosSelected()
        {
            Draw.Circle(transform.position, Vector3.up, inRangeOfTower ? Color.green : Color.red, minDistanceThreshold);
        }
        //================================================================================================================//
    }
}