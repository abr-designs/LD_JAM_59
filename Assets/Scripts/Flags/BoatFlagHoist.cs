using System;
using System.Collections.Generic;
using System.Linq;
using Prototypes.Alex.Utilities;
using UnityEngine;

namespace Prototypes.Alex
{
    public class BoatFlagHoist : FlagHoister
    {
        internal class FlagCommsResults
        {
            public bool ShouldIgnore;
            public bool IsValid;
            public FLAG[] Targets;
            public FLAG Action;
        }
        
        private static PlayerFlagHoist s_playerFlagHoist;
        
        internal Func<FlagCommsResults, bool> ProcessCommunicationForMe;
        internal FLAG boatType;
        
        [SerializeField]
        private List<FLAG> presentFlagsOnStart;

        //Unity Functions
        //================================================================================================================//

        protected override void Start()
        {
            base.Start();
            
            if(s_playerFlagHoist == null)
                s_playerFlagHoist = FindAnyObjectByType<PlayerFlagHoist>();
                            
            if(presentFlagsOnStart != null && presentFlagsOnStart.Count > 0)
                HoistFlags(presentFlagsOnStart);
        }


        //================================================================================================================//

        private bool m_isListeningForPlayerFlags;
        internal void StartListeningForPlayerFlags()
        {
            if (m_isListeningForPlayerFlags)
                return;
            
            s_playerFlagHoist.OnFlagsChanged += OnPlayerFlagsChanged;
            m_isListeningForPlayerFlags = true;
        }

        internal void StopListeningForPlayerFlags()
        {
            if (!m_isListeningForPlayerFlags)
                return;
            
            s_playerFlagHoist.OnFlagsChanged -= OnPlayerFlagsChanged;
            m_isListeningForPlayerFlags = false;
        }

        
        //Flag Language Processing
        //================================================================================================================//

        internal bool PlayerHasFlagsForMe(out FlagCommsResults flagCommsResults)
        {
            flagCommsResults = ProcessPlayerFlags(boatType);

            return flagCommsResults != null && !flagCommsResults.ShouldIgnore;
        }
        internal static FlagCommsResults ProcessPlayerFlags(FLAG myFlag)
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


    }
}