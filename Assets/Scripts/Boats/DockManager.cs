using System;
using System.Collections.Generic;
using System.Linq;
using Flags;
using NaughtyAttributes;
using Prototypes.Alex.Days;
using Prototypes.Alex.Utilities;
using UnityEngine;
using UnityUtils;
using Utilities;
using Utilities.Animations;

namespace Prototypes.Alex.Boats
{
    public class DockManager : MonoBehaviour
    {
        [Serializable]
        private class DockData
        {
            public bool IsFull => maxCapacity > 0 && boats.Count >= maxCapacity;
            
            public FLAG dock;
            public SimplePath dockPath;
            public DockFlagHoist dockFlagHoist;
            
            [Space(10f)]
            [ReadOnly]
            public FLAG cargoType;
            [ReadOnly]
            public int maxCapacity;
            [ReadOnly]
            public List<BaseBoat> boats = new List<BaseBoat>();
        }
        
        public static event Action DocksFull;
        public static event Action OnWrongDock;
        
        private IReadOnlyCollection<RuleData> m_daysRules;
        
        [SerializeField]
        private List<DockData> docks = new();
        
        public Transform portEntranceTransform;

        //Ship Behaviours
        //================================================================================================================//

        public bool AreDocksFull()
        {
            var boatCount = docks.Sum(d => d.boats?.Count);
            var expectedBoatCount = docks.Sum(d => d.maxCapacity);
            
            return boatCount >= expectedBoatCount;
        }
        
        public bool RequestToDock(FLAG dockFlag, BaseBoat boat)
        {
            if(!dockFlag.IsDock())
                throw new ArgumentException("Cannot dock with non-dock flag: " + dockFlag);
            
            var dockData = docks.FirstOrDefault(d => d.dock == dockFlag);

            if (dockData == null)
                throw new ArgumentNullException(nameof(dockFlag), "Cannot find dock with flag: " + dockFlag);

            if (dockData.IsFull)
            {
                Debug.Log("Dock is full: " + dockFlag + "");
                //If this boat auto-attempted to dock here OR they were sent to a full dock, that's a problem
                OnWrongDock?.Invoke();
                return false;
            }
            
            dockData.boats.Add(boat);
            
            //Determine if boat is supposed to be here based on the days rules & if it located at the right place
            if(!IsBoatAllowedAtDock(boat, dockData))
            {
                Debug.Log($"Docking boat not allowed\n- Ship: {boat.name}\n- Dock: {dockFlag}\n");
                OnWrongDock?.Invoke();
            }
            
            if(AreDocksFull())
            {
                Debug.Log("All docks are full");
                DocksFull?.Invoke();
            }
            
            return true;
        }

        public FLAG GetRandomDockAvailableDock()
        {
            var freeSpots =  docks.Where(d => !d.IsFull);
            return freeSpots.Random().dock;
        }
        public SimplePath GetDockPath(FLAG dock)
        {
            return docks.First(d => d.dock == dock).dockPath;
        }

        private bool IsBoatAllowedAtDock(BaseBoat boat, DockData dockData)
        {
            if (dockData.cargoType != boat.CargoType)
            {
                Debug.Log($"{dockData.cargoType} != {boat.CargoType}");
                return false;
            }

            foreach (var rule in m_daysRules)
            {
                if (rule.shipType == boat.ShipType && rule.cargoTypes == boat.CargoType)
                {
                    Debug.Log($"{rule.shipType} == {boat.ShipType} && {rule.cargoTypes} == {boat.CargoType}");
                    return false;
                }
            }

            return true;
        }

        //Day Behaviours
        //================================================================================================================//

        public void SetupDocks(IReadOnlyList<DockRequirementData> dockRequirements, List<RuleData> daysRules)
        {
            m_daysRules = daysRules;
            
            foreach (var dock in docks)
            {
                dock.dockFlagHoist.RemoveFlags();
            }
            
            foreach (var dockRequirement in dockRequirements)
            {
                var dock = docks.First(d => d.dock == dockRequirement.targetDock);
                
                dock.maxCapacity = dockRequirement.cargoCount;
                dock.boats.Clear();
                dock.cargoType = dockRequirement.cargoType;
                dock.dockFlagHoist.SetupDock(dock.dock, dockRequirement);
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