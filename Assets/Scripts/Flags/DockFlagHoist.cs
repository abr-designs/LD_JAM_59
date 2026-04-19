using System.Collections.Generic;
using Prototypes.Alex;
using Prototypes.Alex.Days;

namespace Flags
{
    public class DockFlagHoist : FlagHoister
    {
        private List<FLAG> m_flags;
    
        public void SetupDock(FLAG dockFlag, DockRequirementData dockRequirementData)
        {
            if (m_flags == null)
                m_flags = new List<FLAG>();

            if (m_flags.Count > 0)
            {
                RemoveFlags();
                m_flags.Clear();
            }
        
            var flagCount = dockRequirementData.cargoCount;
            var flagType = dockRequirementData.cargoType;
        
            m_flags.Add(dockFlag);
            for (int i = 0; i < flagCount; i++)
            {
                m_flags.Add(flagType);
            }
        
            HoistFlags(m_flags);
        }
    }
}
