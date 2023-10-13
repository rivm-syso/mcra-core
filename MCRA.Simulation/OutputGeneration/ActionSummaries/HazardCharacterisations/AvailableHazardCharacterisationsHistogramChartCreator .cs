using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AvailableHazardCharacterisationsHistogramChartCreator : AvailableHazardCharacterisationsHistogramChartCreatorBase {

        private readonly string _sectionId;

        public AvailableHazardCharacterisationsHistogramChartCreator(
            string sectionId,
            List<AvailableHazardCharacterisationsSummaryRecord> records,
            string targetDoseUnit,
            int width,
            int height
        ) : base(
            records,
            targetDoseUnit,
            width,
            height
        ) {
            _sectionId = sectionId;
            Width = width;
            Height = height;
        }

        public override string ChartId {
            get {
                var pictureId = "2cbaea535-b0b6-433e-a798-c4951a22a1f9";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }
    }
}
