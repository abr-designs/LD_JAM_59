using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Prototypes.Alex.Boats;
using Prototypes.Alex.Days;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Prototypes.Alex
{
    public class GameFlowManager : MonoBehaviour
    {
        public static event Action<int> OnStrikesChanged;
        public static event Action OnGameOver;
        
        [SerializeField, Header("Managers")]
        private DockManager dockManager;
        
        [SerializeField]
        private BulletinManager bulletinManager;
        
        [SerializeField, Min(0f), Header("Times")]
        private float boardReviewTime = 10f;

        [SerializeField, Min(0f)]
        private float fadeTime = 0.5f;

        public int MaxStrikes => maxStrikes;
        [SerializeField, Min(0)]
        private int maxStrikes = 3;
        [SerializeField, ReadOnly]
        private int currentStrikes;
        
        [SerializeField, Header("Days")]
        private List<DayDefinition> dayDefinitions;
        
        [SerializeField, Header("Flags")]
        private List<FlagDefinition> flagDefinitions;
        private Dictionary<FLAG, FlagDefinition> m_flags;

        [SerializeField]
        private Camera waitCamera;
        [SerializeField]
        private Camera playerCamera;
        
        private bool m_gameOver;
        private Coroutine m_gameLoopCoroutine;

#if UNITY_EDITOR
        [SerializeField, Header("Debugging")]
        private bool debug;

        [SerializeField]
        private float boardReviewTimeOverride;
        [SerializeField]
        private float boatSpawnTimeOverride;
        [SerializeField]
        private float startDelayOverride;
#endif

        //Unity Functions
        //================================================================================================================//

        private void OnEnable()
        {
            DockManager.OnWrongDock += ApplyStrike;
        }

        private void Start()
        {
            
            m_flags = new Dictionary<FLAG, FlagDefinition>();
            foreach (var flagDefinition in flagDefinitions)
            {
                m_flags.Add(flagDefinition.flag, flagDefinition);
            }
            
            m_gameLoopCoroutine = StartCoroutine(GameLoopCoroutine());
        }

        private void OnDisable()
        {
            DockManager.OnWrongDock -= ApplyStrike;
        }
        
        //Game Loop
        //================================================================================================================//


        private IEnumerator GameLoopCoroutine()
        {
            foreach (var dayDefinition in dayDefinitions)
            {
                ScreenFader.ForceSetColorBlack();

                waitCamera.gameObject.SetActive(true);
                playerCamera.gameObject.SetActive(false);
                
                dockManager.SetupDocks(dayDefinition.dockRequirements, dayDefinition.rules);
                bulletinManager.Setup(dayDefinition.dockRequirements, dayDefinition.rules);
                
                yield return ScreenFader.FadeIn(fadeTime, null);

#if UNITY_EDITOR
                yield return new WaitForSeconds(debug ? boardReviewTimeOverride : boardReviewTime);
#else
                yield return new WaitForSeconds(boardReviewTime);
#endif

                ScreenFader.FadeInOut(fadeTime, () =>
                {
                    waitCamera.gameObject.SetActive(false);
                    playerCamera.gameObject.SetActive(true);
                }, null);

#if UNITY_EDITOR
                var wait = dayDefinition.startDelay;
                yield return new WaitForSeconds(debug ? startDelayOverride : wait);
#else
                yield return new WaitForSeconds(dayDefinition.startDelay);
                
#endif

                var spawnShipsCoroutine = StartCoroutine(SpawnShips(dayDefinition));

                var isDone = false;
                DockManager.DocksFull += OnDocksFull;
                BaseBoat.OnNoMoreBoats += OnDocksFull;

                yield return new WaitUntil(() => isDone);

                StopCoroutine(spawnShipsCoroutine);
                BaseBoat.CleanBoats();
                yield return ScreenFader.FadeOut(fadeTime, null);
                
                continue;

                void OnDocksFull()
                {
                    DockManager.DocksFull -= OnDocksFull;
                    isDone = true;
                }
            }
            
            //TODO Move to the win screen
        }

        private IEnumerator SpawnShips(DayDefinition dayDefinition)
        {
            for (int i = 0; i < dayDefinition.shipSpawnCount; i++)
            {
                //If something failed (Too many attempts), wait & try again
                if (!dayDefinition.SpawnRandomShip())
                    i -= 1;

#if UNITY_EDITOR
                var wait = Random.Range(dayDefinition.shipSpawnIntervalMin, dayDefinition.shipSpawnIntervalMax);
                yield return new WaitForSeconds(debug ? boatSpawnTimeOverride : wait);
#else
                var wait = Random.Range(dayDefinition.shipSpawnIntervalMin, dayDefinition.shipSpawnIntervalMax);
                
                yield return new WaitForSeconds(wait);
#endif
            }
        }

        //Callbacks
        //================================================================================================================//

        [Button]
        private void ApplyStrike()
        {
            if (m_gameOver)
                return;
            
            //If it's set to zero or less, we don't care
            if (maxStrikes <= 0)
                return;
            
            currentStrikes++;
            OnStrikesChanged?.Invoke(currentStrikes);

            if (currentStrikes < maxStrikes)
                return;
            
            OnGameOver?.Invoke();

            StopCoroutine(m_gameLoopCoroutine);
        }
        
        //Utility Functions
        //================================================================================================================//

        public FlagDefinition GetFlagDefinition(FLAG flag)
        {
            if(m_flags.TryGetValue(flag, out var flagDefinition))
                return flagDefinition;

            throw new MissingMemberException($"No flag definition found for {flag}");
        }
    }
}