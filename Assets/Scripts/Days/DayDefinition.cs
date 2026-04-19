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
        
        public List<ShipSpawnData> shipSpawnData;
        public List<RuleData> rules;
        public List<DockRequirementData> dockRequirements;

        public void SpawnRandomShip()
        {
            shipSpawnCount++;
            var shipData = GetRandomShipSpawnData();
            
            BoatFactory.SpawnBoat(shipData);
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