using System;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HierarchyRecord : ICloneable {

        public HierarchyRecord() {
            __IsSummaryRecord = false;
        }

        public bool __IsSummaryRecord { get; set; }

        public string __Id { get; set; }

        public string __IdParent { get; set; }

        #region IClonable

        public object Clone() {
            return this.MemberwiseClone();
        }

        #endregion

    }
}
