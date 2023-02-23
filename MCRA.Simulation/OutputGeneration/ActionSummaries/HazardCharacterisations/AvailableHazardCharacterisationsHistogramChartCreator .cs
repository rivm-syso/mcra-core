using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AvailableHazardCharacterisationsHistogramChartCreator : AvailableHazardCharacterisationsHistogramChartCreatorBase {

        private readonly AvailableHazardCharacterisationsSummarySection _section;

        public AvailableHazardCharacterisationsHistogramChartCreator(
            AvailableHazardCharacterisationsSummarySection section,
            string targetDoseUnit,
            int width,
            int height
        ) : base(
            section.Records.Cast<AvailableHazardCharacterisationsSummaryRecord>().ToList(),
            targetDoseUnit,
            width,
            height
        ) {
            _section = section;
            Width = width;
            Height = height;
        }

        public override string ChartId {
            get {
                var pictureId = "2cbaea535-b0b6-433e-a798-c4951a22a1f9";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
    }
}
