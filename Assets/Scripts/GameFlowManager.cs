using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prototypes.Alex
{
    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField]
        private Transform boat;
        
        [SerializeField]
        private List<FlagDefinition> flagDefinitions;
        private Dictionary<FLAG, FlagDefinition> m_flags;

        private void Start()
        {
            m_flags = new Dictionary<FLAG, FlagDefinition>();
            foreach (var flagDefinition in flagDefinitions)
            {
                m_flags.Add(flagDefinition.flag, flagDefinition);
            }
        }

        private IEnumerator GameLoopCoroutine()
        {
            //TODO Boat moves towards the port
            //TODO Player signals the boat
            //TODO Process outgoing message when within range
            //TODO Boat responds to the signal
            //TODO Player decodes signals
            //TODO Player Responds
            //TODO Process outgoing message
            //TODO Boat responds to the signal
            
            yield return null;
        }
        
        //Utility Functions
        //================================================================================================================//

        public FlagDefinition GetFlagDefinition(FLAG flag)
        {
            return m_flags[flag];
        }
    }
}