using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureThresholdRatioMedianChartCreator : CumulativeExposureThresholdRatioChartCreatorBase {

        private MultipleExposureThresholdRatioSection _section;
        private bool _isUncertainty;

        public CumulativeExposureThresholdRatioMedianChartCreator(MultipleExposureThresholdRatioSection section, bool isUncertainty) {
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
            var RPFweightedRecord = _section.RiskRecords.FirstOrDefault(c => c.IsCumulativeRecord);
            var rpfWeightedRisk = RPFweightedRecord != null ? RPFweightedRecord.RiskP50Nom : double.NaN;
            rpfWeightedRisk = !_section.UseIntraSpeciesFactor ? rpfWeightedRisk : double.NaN;
            var orderedHazardRecords = _section.RiskRecords
                .Where(c => !c.IsCumulativeRecord)
                .OrderByDescending(c => c.RiskP50Nom)
                .ToList();

            var orderedExposureThresholdRatios = orderedHazardRecords.CumulativeWeights(c => c.RiskP50Nom).ToList();
            var substances = orderedHazardRecords.Select(c => c.SubstanceName).ToList();
            return create(orderedExposureThresholdRatios, substances, rpfWeightedRisk, 50, _isUncertainty);
        }
    }
}
