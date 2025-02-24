using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public class FullCoExposureRecord : CoExposureRecord {
        [Description("Substances occurring together")]
        [Display(Name="Substances", Order = 5)]
        public string Substances { get; set; }
    }
}
