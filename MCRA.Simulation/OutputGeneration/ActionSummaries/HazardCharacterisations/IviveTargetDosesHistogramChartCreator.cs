using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IviveTargetDosesHistogramChartCreator : IviveHazardCharacterisationsHistogramChartCreatorBase {

        private readonly IviveHazardCharacterisationsSummarySection _section;

        public IviveTargetDosesHistogramChartCreator(
            IviveHazardCharacterisationsSummarySection section,
            string targetDoseUnit,
            int width,
            int height
        ) : base(
            section.Records.Cast<IviveHazardCharacterisationsSummaryRecord>().ToList(),
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
                var pictureId = "22025964-91ff-483b-83b3-5f7c3d2c1e17";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
    }
}
