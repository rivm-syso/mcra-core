using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class HbmContributionPercentilesRecord : BoxPlotChartRecord{

        [Display(AutoGenerateField = false)]
        public ExposureTarget TargetUnit { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Expression type.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Description("The exposure route of the external exposure estimates derived from HBM data.")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        public override string GetLabel() {
            throw new NotImplementedException();
        }
    }
}
