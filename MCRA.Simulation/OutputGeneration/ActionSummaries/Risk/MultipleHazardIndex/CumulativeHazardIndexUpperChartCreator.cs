using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeHazardIndexUpperChartCreator : CumulativeHazardIndexChartCreatorBase {

        private MultipleHazardIndexSection _section;
        private bool _isUncertainty;

        public CumulativeHazardIndexUpperChartCreator(MultipleHazardIndexSection section, bool isUncertainty) {
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "8edc3312-6386-4054-9e95-c571b8beef25";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var RPFweightedRecord = _section.HazardIndexRecords
                .Where(c => c.IsCumulativeRecord)
                .FirstOrDefault();

            var rpfWeightedHI = RPFweightedRecord != null ? RPFweightedRecord.PUpperHINom : double.NaN;
            rpfWeightedHI = !_section.UseIntraSpeciesFactor ? rpfWeightedHI : double.NaN;

            var orderedHazardRecords = _section.HazardIndexRecords
                .Where(c => !c.IsCumulativeRecord)
                .OrderByDescending(c => c.PUpperHINom)
                .ToList();

            var orderedHazardIndices = orderedHazardRecords.CumulativeWeights(c => c.PUpperHINom).ToList();
            var substances = orderedHazardRecords.Select(c => c.CompoundName).ToList();
            var percentage = 100 - (100 - _section.ConfidenceInterval) / 2;
            return create(orderedHazardIndices, substances, rpfWeightedHI, percentage, _isUncertainty);
        }
    }
}
