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
        
        [SerializeField, Header("Days")]
        private List<DayDefinition> dayDefinitions;
        
        [SerializeField, Header("Flags")]
        private List<FlagDefinition> flagDefinitions;
        private Dictionary<FLAG, FlagDefinition> m_flags;

        [SerializeField]
        private Camera waitCamera;
        [SerializeField]
        private Camera playerCamera;

        

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
                
                yield return ScreenFader.FadeIn(0.5f, null);

                yield return new WaitForSeconds(Random.Range(10f, 15f));

                ScreenFader.FadeInOut(0.5f, () =>
                {
                    waitCamera.gameObject.SetActive(false);
                    playerCamera.gameObject.SetActive(true);
                }, null);

                var spawnShipsCoroutine = StartCoroutine(SpawnShips(dayDefinition));

                var isDone = false;
                DockManager.DocksFull += OnDocksFull;
                BaseBoat.OnNoMoreBoats += OnDocksFull;

                yield return new WaitUntil(() => isDone);

                StopCoroutine(spawnShipsCoroutine);
                
                yield return ScreenFader.FadeOut(0.5f, null);
                
                continue;

                void OnDocksFull()
                {
                    DockManager.DocksFull -= OnDocksFull;
                    isDone = true;
                }
            }
            
            //TODO Move to the win screen
        }

        private static IEnumerator SpawnShips(DayDefinition dayDefinition)
        {
            for (int i = 0; i < dayDefinition.shipSpawnCount; i++)
            {
                dayDefinition.SpawnRandomShip();
                
                var wait = Random.Range(dayDefinition.shipSpawnIntervalMin, dayDefinition.shipSpawnIntervalMax);
                yield return new WaitForSeconds(wait);
            }
        }
        
        //Utility Functions
        //================================================================================================================//

        public FlagDefinition GetFlagDefinition(FLAG flag)
        {
            return m_flags[flag];
        }
    }
}