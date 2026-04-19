using System;
using UnityEngine;

namespace Prototypes.Alex.Days
{
    [Serializable]
    public class DockRequirementData
    {
        public FLAG targetDock;
        public FLAG cargoType;
        public int cargoCount;
    }
}