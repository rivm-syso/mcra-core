using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeHazardIndexMedianChartCreator : CumulativeHazardIndexChartCreatorBase {

        private MultipleHazardIndexSection _section;
        private bool _isUncertainty;

        public CumulativeHazardIndexMedianChartCreator(MultipleHazardIndexSection section, bool isUncertainty) {
            _section = section;
            _isUncertainty = isUncertainty;
        }

        public override string ChartId {
            get {
                var pictureId = "f362464f-6c8d-449f-8f27-d914fb4d6416";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var RPFweightedRecord = _section.HazardIndexRecords.FirstOrDefault(c => c.IsCumulativeRecord);
            var rpfWeightedHI = RPFweightedRecord != null ? RPFweightedRecord.HIP50Nom : double.NaN;
            rpfWeightedHI = !_section.UseIntraSpeciesFactor ? rpfWeightedHI : double.NaN;
            var orderedHazardRecords = _section.HazardIndexRecords
                .Where(c => !c.IsCumulativeRecord)
                .OrderByDescending(c => c.HIP50Nom)
                .ToList();

            var orderedHazardIndices = orderedHazardRecords.CumulativeWeights(c => c.HIP50Nom).ToList();
            var substances = orderedHazardRecords.Select(c => c.CompoundName).ToList();
            return create(orderedHazardIndices, substances, rpfWeightedHI, 50, _isUncertainty);
        }
    }
}
