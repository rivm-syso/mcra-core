using MCRA.Utils.DataTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryExposureSourceSummaryRecord {

        [DisplayName("Food name")]
        public string Name { get; set; }

        [DisplayName("Food code")]
        public string Code { get; set; }

        [DisplayName("Parent code")]
        public string CodeParent { get; set; }

    }
}
