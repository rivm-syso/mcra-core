using MCRA.Utils.ExtensionMethods;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardCharacterisationImputationCandidatesSectionChartCreator
        : HazardCharacterisationsHistogramChartCreatorBase {

        private readonly HazardCharacterisationImputationCandidatesSection _section;

        public HazardCharacterisationImputationCandidatesSectionChartCreator(
            HazardCharacterisationImputationCandidatesSection section,
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
                var pictureId = "D50A69FC-4795-461F-99F9-D7FABB60A84B";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
    }
}
