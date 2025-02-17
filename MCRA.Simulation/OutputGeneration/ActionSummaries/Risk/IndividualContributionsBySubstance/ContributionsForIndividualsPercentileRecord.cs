using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk.IndividualContributionsBySubstance {
    public class ContributionsForIndividualsPercentileRecord : BoxPlotChartRecord {

        [Display(AutoGenerateField = false)]
        public ExposureTarget TargetUnit { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Code of the biological matrix.")]
        [DisplayName("Biological matrix code")]
        public string BiologicalMatrix { get; set; }

        public override string GetLabel() {
            return SubstanceName;
        }
    }
}
