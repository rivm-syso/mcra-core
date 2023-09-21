using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsHistogramChartCreator : HazardCharacterisationsHistogramChartCreatorBase {

        private readonly string _sectionId;

        public HazardCharacterisationsHistogramChartCreator(
            string sectionId,
            List<HazardCharacterisationsSummaryRecord> records,
            string targetDoseUnit,
            int width,
            int height
        ) : base(
            records.Cast<HazardCharacterisationSummaryRecord>().ToList(),
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
                var pictureId = "2468313F-3A6C-4575-AB3F-A49BDD9204E5";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId);
            }
        }
    }
}
