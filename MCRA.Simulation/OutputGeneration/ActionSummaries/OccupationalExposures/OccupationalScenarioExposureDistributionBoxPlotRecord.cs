using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalScenarioExposureDistributionBoxPlotRecord : BoxPlotChartRecord {

        [Display(AutoGenerateField = false)]
        public double LowerUncertaintyBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UpperUncertaintyBound { get; set; }

        [DisplayName("Scenario name")]
        public string ScenarioName { get; set; }

        [DisplayName("Scenario code")]
        public string ScenarioCode { get; set; }

        [Description("The exposure route of the exposure estimates derived from dust data.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        public override string GetLabel() {
            return $"{ScenarioName.LimitTo(40)} - {SubstanceName.LimitTo(30)}";
        }
    }
}