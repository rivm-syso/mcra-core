using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationsHistogramChartCreator : HazardCharacterisationsHistogramChartCreatorBase {

        private readonly HazardCharacterisationsSummarySection _section;

        public HazardCharacterisationsHistogramChartCreator(
            HazardCharacterisationsSummarySection section,
            string targetDoseUnit,
            int width,
            int height
        ) : base(
            section.Records.Cast<HazardCharacterisationSummaryRecord>().ToList(),
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
                var pictureId = "2468313F-3A6C-4575-AB3F-A49BDD9204E5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
    }
}
