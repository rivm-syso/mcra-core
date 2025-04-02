using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryExposuresPercentilesRecord : BoxPlotChartRecord {

        [DisplayName("Non-dietary survey")]
        public string NonDietarySurvey { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Route")]
        public string ExposureRoute { get; set; }

        [Description("Number of measurements.")]
        [DisplayName("Number of measurements")]
        public int NumberOfMeasurements { get; set; }

        public override string GetLabel() {
            return $"{NonDietarySurvey} {ExposureRoute} {SubstanceName}";
        }
    }
}
