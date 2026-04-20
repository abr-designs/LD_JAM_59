using System;
using Prototypes.Alex.Days;
using UnityEngine;
using Utilities;
using Utilities.Debugging;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Prototypes.Alex.Boats
{
    public class BoatFactory : HiddenSingleton<BoatFactory>
    {
        [Serializable]
        private struct SpawnPoint
        {
            public Vector3 worldPosition;
            public float radius;
        }
        
        [SerializeField]
        private BaseBoat boatPrefab;
        
        [SerializeField]
        private SpawnPoint[] spawnPoints;

        public static BaseBoat SpawnBoat(ShipSpawnData shipSpawnData) => SpawnBoat(shipSpawnData.ShipType, shipSpawnData.CargoType);
        public static BaseBoat SpawnBoat(FLAG shipType, FLAG cargoType)
        {
            return Instance.Instantiate(shipType, cargoType);
        }

        private BaseBoat Instantiate(FLAG shipType, FLAG cargoType)
        {
            var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var randomPosition = Random.insideUnitCircle * spawnPoint.radius;
            var position = spawnPoint.worldPosition + new Vector3(randomPosition.x, 0, randomPosition.y);
            
            var baseBoat = Object.Instantiate(boatPrefab, position, Quaternion.identity);
            
            baseBoat.gameObject.name = $"{shipType}_{cargoType}_{nameof(BaseBoat)}_instance";
            
            baseBoat.Init(shipType, cargoType);
            
            return baseBoat;
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Draw.Circle(spawnPoints[i].worldPosition, Vector3.up, Color.yellow, spawnPoints[i].radius, 10);
            }
        }
    }
}