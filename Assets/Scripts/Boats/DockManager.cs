using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Prototypes.Alex.Days;
using UnityEngine;

namespace Prototypes.Alex.Boats
{
    public class DockManager : MonoBehaviour
    {
        [Serializable]
        private class DockData
        {
            public bool IsFull => boats.Count >= maxCapacity;
            
            public FLAG dock;
            public Transform dockTransform;
            [Space(10f)]
            [ReadOnly]
            public FLAG cargoType;
            [ReadOnly]
            public int maxCapacity;
            [ReadOnly]
            public List<BaseBoat> boats = new List<BaseBoat>();
        }
        
        public static event Action DocksFull;
        
        [SerializeField]
        private List<DockData> docks = new();

        //Ship Behaviours
        //================================================================================================================//

        public bool AreDocksFull() =>  docks.All(d => d.IsFull);
        
        public bool RequestToDock(FLAG dock, BaseBoat boat)
        {
            var dockData = docks.First(d => d.dock == dock);

            if (dockData.IsFull)
                return false;
            
            dockData.boats.Add(boat);
            
            if(AreDocksFull())
                DocksFull?.Invoke();
            
            return true;
        }

        public Transform GetDockTransform(FLAG dock)
        {
            return docks.First(d => d.dock == dock).dockTransform;
        }

        //Day Behaviours
        //================================================================================================================//

        public void SetupDocks(IReadOnlyList<DockRequirementData> dockRequirements)
        {
            foreach (var dockRequirement in dockRequirements)
            {
                var dock = docks.First(d => d.dock == dockRequirement.targetDock);
                
                dock.maxCapacity = dockRequirement.cargoCount;
                dock.boats.Clear();
                dock.cargoType = dockRequirement.cargoType;
                
            }
        }

        public bool DoDocksMatchRequest(IReadOnlyList<DockRequirementData> dockRequirements)
        {
            foreach (var dockRequirement in dockRequirements)
            {
                var dock = docks.FirstOrDefault(d => d.dock == dockRequirement.targetDock);

                if (dock.boats.Count != dockRequirement.cargoCount)
                    return false;
                
                var cargoCount = dock.boats.Count(c => c.CargoType == dockRequirement.cargoType);
                if (cargoCount != dockRequirement.cargoCount)
                    return false;
            }

            return true;
        }
        
        public bool DoDocksMatchRules(IReadOnlyList<RuleData> rules)
        {
            foreach (var ruleData in rules)
            {
                var shipType = ruleData.shipType;
                var cargoType = ruleData.cargoTypes;
                
                var any = docks.Any(d => d.boats.Any(b => b.ShipType == shipType && b.CargoType == cargoType));

                if (any)
                    return false;
            }

            return true;
        }
    }
}