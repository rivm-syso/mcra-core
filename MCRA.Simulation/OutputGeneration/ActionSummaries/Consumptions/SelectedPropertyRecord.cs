using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Consumptions {
    public sealed class SelectedPropertyRecord {
        [DisplayName("Property name")]
        public string PropertyName { get; set; }

        [DisplayName("Selected levels")]
        public string Levels { get; set; }
    }
}
