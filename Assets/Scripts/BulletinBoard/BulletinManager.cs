using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prototypes.Alex.Days
{
    public class BulletinManager : MonoBehaviour
    {
        [SerializeField]
        private ProtocolElement[] protocolElements;
        [SerializeField]
        private ManifestElement[] manifestElements;

        private void Start()
        {
            ResetBoardElements();


        }

        public void Setup(List<DockRequirementData> dockRequirements, List<RuleData> rules)
        {
            ResetBoardElements();

            for (int i = 0; i < dockRequirements.Count; i++)
            {
                manifestElements[i].gameObject.SetActive(true);
                manifestElements[i].Setup(dockRequirements[i]);
            }
            
            for (int i = 0; i < rules.Count; i++)
            {
                protocolElements[i].gameObject.SetActive(true);
                protocolElements[i].Setup(rules[i]);
            }

        }

        private void ResetBoardElements()
        {
            for (int i = 0; i < protocolElements.Length; i++)
            {
                protocolElements[i].gameObject.SetActive(false);
            }
            
            for (int i = 0; i < manifestElements.Length; i++)
            {
                manifestElements[i].gameObject.SetActive(false);
            }
        }
    }
}