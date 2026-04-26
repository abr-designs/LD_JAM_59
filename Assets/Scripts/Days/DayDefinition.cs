using System.Collections.Generic;
using Prototypes.Alex.Boats;
using UnityEngine;

namespace Prototypes.Alex.Days
{
    [CreateAssetMenu(fileName = "New Day Definition", menuName = "ScriptableObjects/Day Definition", order = 1)]
    public class DayDefinition : ScriptableObject
    {
        public int shipSpawnCount;
        public float shipSpawnIntervalMin;
        public float shipSpawnIntervalMax;
        [Min(0f)]
        public float startDelay;
        
        public List<ShipSpawnData> shipSpawnData;
        public List<RuleData> rules;
        public List<DockRequirementData> dockRequirements;

        public bool SpawnRandomShip()
        {
            const int MAX_ATTEMPTS = 100;
            
            var shipData = GetRandomShipSpawnData();
            int attempts = 0;
            while (BaseBoat.HasActiveBoat(shipData.ShipType, shipData.CargoType))
            {
                if (++attempts >= MAX_ATTEMPTS)
                {
                    Debug.LogError("SpawnRandomShip: Failed to find a unique ship after 100 attempts...");
                    return false;
                }

                shipData = GetRandomShipSpawnData();
            }

            BoatFactory.SpawnBoat(shipData);
            return true;
        }

        private ShipSpawnData GetRandomShipSpawnData()
        {
            float totalWeight = 0f;
            foreach (var data in shipSpawnData)
                totalWeight += data.SpawnWeight;

            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var data in shipSpawnData)
            {
                cumulative += data.SpawnWeight;
                if (random < cumulative)
                    return data;
            }

            return shipSpawnData[^1];
        }
    }
}