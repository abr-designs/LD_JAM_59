using System;
using System.Collections;
using System.Collections.Generic;
using Prototypes.Alex.Boats;
using Prototypes.Alex.Days;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Prototypes.Alex
{
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField, Header("Managers")]
        private DockManager dockManager;
        
        [SerializeField]
        private BulletinManager bulletinManager;
        
        [SerializeField, Min(0f), Header("Times")]
        private float boardReviewTime = 10f;

        [SerializeField, Min(0f)]
        private float fadeTime = 0.5f;
        
        [SerializeField, Header("Days")]
        private List<DayDefinition> dayDefinitions;
        
        [SerializeField, Header("Flags")]
        private List<FlagDefinition> flagDefinitions;
        private Dictionary<FLAG, FlagDefinition> m_flags;

        [SerializeField]
        private Camera waitCamera;
        [SerializeField]
        private Camera playerCamera;

#if UNITY_EDITOR
        [SerializeField, Header("Debugging")]
        private bool debug;

        [SerializeField]
        private float boatSpawnTimeOverride;
        [SerializeField]
        private float startDelayOverride;
#endif

       

        private void Start()
        {
            m_flags = new Dictionary<FLAG, FlagDefinition>();
            foreach (var flagDefinition in flagDefinitions)
            {
                m_flags.Add(flagDefinition.flag, flagDefinition);
            }
            
            StartCoroutine(GameLoopCoroutine());
        }

        private IEnumerator GameLoopCoroutine()
        {
            foreach (var dayDefinition in dayDefinitions)
            {
                ScreenFader.ForceSetColorBlack();

                waitCamera.gameObject.SetActive(true);
                playerCamera.gameObject.SetActive(false);
                
                dockManager.SetupDocks(dayDefinition.dockRequirements);
                bulletinManager.Setup(dayDefinition.dockRequirements, dayDefinition.rules);
                
                yield return ScreenFader.FadeIn(fadeTime, null);

                yield return new WaitForSeconds(boardReviewTime);

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
                dayDefinition.SpawnRandomShip();

#if UNITY_EDITOR
                var wait = Random.Range(dayDefinition.shipSpawnIntervalMin, dayDefinition.shipSpawnIntervalMax);
                yield return new WaitForSeconds(debug ? boatSpawnTimeOverride : wait);
#else
                var wait = Random.Range(dayDefinition.shipSpawnIntervalMin, dayDefinition.shipSpawnIntervalMax);
                
                yield return new WaitForSeconds(wait);
#endif
            }
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