using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Prototypes.Alex.Utilities;
using UnityEngine;
using Utilities.Debugging;

namespace Prototypes.Alex
{
    public class BoatFlagHoist : FlagHoister
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
        private class FlagCommsResults
        {
            public bool ShouldIgnore;
            public bool IsValid;
            public FLAG[] Targets;
            public FLAG Action;
        }
        
        private static PlayerFlagHoist s_playerFlagHoist;
        
        [SerializeField]
        private FLAG boatType;
        [SerializeField]
        private FLAG carryingFlag;

        [SerializeField]
        private float minDistanceThreshold;

        [SerializeField, ReadOnly]
        private bool inRangeOfTower;
        
        [SerializeField]
        private List<FLAG> presentFlagsOnStart;

        [SerializeField]
        private Transform moveToTargetTransform;
        
        [SerializeField]
        private Transform moveToDockTargetTransform;

        [SerializeField]
        private float boatMoveSpeed;

        [SerializeField]
        private STATE startingState;
        
        [SerializeField, ReadOnly]
        private STATE state;

        private Vector3 m_startPosition;
        private bool m_approvedDocking;
        private bool m_didHailPort;

        //Unity Functions
        //================================================================================================================//

        protected override void Start()
        {
            base.Start();
            
            m_startPosition = transform.position;

            SetState(startingState);
            
            if(s_playerFlagHoist == null)
                s_playerFlagHoist = FindAnyObjectByType<PlayerFlagHoist>();
                            
            if(presentFlagsOnStart != null && presentFlagsOnStart.Count > 0)
                HoistFlags(presentFlagsOnStart);
        }
        
        private void Update()
        {
            switch (state)
            {
                case STATE.NONE:
                    break;
                case STATE.MOVING_TO_PORT:
                    MoveTowards(moveToTargetTransform.position);

                    var planar = Vector3.ProjectOnPlane(transform.position - moveToTargetTransform.position, Vector3.up);
                    var distance = planar.magnitude;
                    if (distance < minDistanceThreshold)
                    {
                        inRangeOfTower = true;
                        
                        //If arriving in range, and player flags are visible, react to it 
                        if (PlayerHasFlagsForMe(out var results) && ProcessCommunicationForMe(results))
                            break;

                        if (distance < 1f)
                            //Otherwise just move towards the docks
                            SetState(STATE.MOVING_TO_DOCK);
                    }
                    break;
                case STATE.MOVING_TO_DOCK:
                    MoveTowards(moveToDockTargetTransform.position);
                    break;
                case STATE.AWAITING_PLAYER_FLAGS:
                    break;
                case STATE.RESPONDING:
                    break;
                case STATE.LEAVING_PORT:
                    MoveTowards(m_startPosition);
                    break;
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
                    s_playerFlagHoist.OnFlagsChanged += OnPlayerFlagsChanged;
                    break;
                case STATE.AWAITING_PLAYER_FLAGS:
                    s_playerFlagHoist.OnFlagsChanged += OnPlayerFlagsChanged;
                    //If there hasn't already been comms with the player, do so here
                    if (!m_didHailPort)
                    {
                        m_didHailPort = true;
                        HoistFlags(new List<FLAG>()
                        {
                            FLAG.HELLO,
                            carryingFlag
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
                    s_playerFlagHoist.OnFlagsChanged -= OnPlayerFlagsChanged;
                    break;
            }
        }
        
        //Flag Language Processing
        //================================================================================================================//
        private bool ProcessCommunicationForMe(FlagCommsResults flagCommsResults)
        {
            //If the message is for me but its not valid, notify the player
            if (!flagCommsResults.ShouldIgnore && !flagCommsResults.IsValid)
            {
                RemoveFlags();
                HoistFlags(new List<FLAG> { FLAG.UNCLEAR });
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
                    SetState(STATE.MOVING_TO_DOCK);
                    HoistFlags(new List<FLAG>() {carryingFlag, flagCommsResults.Action });
                    return true;
            }
            
            return false;
        }
        private bool PlayerHasFlagsForMe(out FlagCommsResults flagCommsResults)
        {
            flagCommsResults = ProcessPlayerFlags(boatType);

            return flagCommsResults != null && !flagCommsResults.ShouldIgnore;
        }
        private static FlagCommsResults ProcessPlayerFlags(FLAG myFlag)
        {
            if(s_playerFlagHoist.CurrentFlags == null || s_playerFlagHoist.CurrentFlags.Count <= 0)
                return null;
            
            //TODO I should be checking if the flags changed, before doing all the stuff below
            var currentFlags = s_playerFlagHoist.CurrentFlags;
            var currentFlagTypes = s_playerFlagHoist.CurrentFlags.Select(f => f.GetFlagDefinition().type);
            
            var targets = currentFlags.Where(f => f.GetFlagDefinition().type == FLAG_TYPE.TARGET);
            var actions = currentFlags.Where(f => f.GetFlagDefinition().type == FLAG_TYPE.ACTION);

            bool shouldIgnore = targets.Count() == 0 || targets.All(f => f != myFlag);
            bool isValid = currentFlags.Count > 0 && 
                           targets.Any() &&
                           actions.Count() == 1 && 
                           IsValid(currentFlagTypes, new List<FLAG_TYPE> { FLAG_TYPE.TARGET, FLAG_TYPE.ACTION} );
            
            return new FlagCommsResults
            {
                ShouldIgnore = shouldIgnore,
                IsValid = isValid,
                Targets = targets.ToArray(),
                Action = actions.FirstOrDefault()
            };
            
            bool IsValid(IEnumerable<FLAG_TYPE> list, List<FLAG_TYPE> expectedOrder)
            {
                //Prepares a defined order
                Dictionary<FLAG_TYPE, int> order = new Dictionary<FLAG_TYPE, int>();
                for (var i = 0; i < expectedOrder.Count; i++)
                {
                    var flagType = expectedOrder[i];
                    order.Add(flagType, i);
                }

                int last = int.MinValue;

                foreach (var flag in list)
                {
                    int current = order[flag];

                    if (current < last)
                        return false;

                    last = current;
                }

                return true;
            }
        }

        //Callbacks
        //================================================================================================================//

        private void OnPlayerFlagsChanged(IReadOnlyList<FLAG> newFlags)
        {
            if (newFlags.Count <= 0)
                return;
            
            //If arriving in range, and player flags are visible, react to it 
            if (PlayerHasFlagsForMe(out var results))
                ProcessCommunicationForMe(results);
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