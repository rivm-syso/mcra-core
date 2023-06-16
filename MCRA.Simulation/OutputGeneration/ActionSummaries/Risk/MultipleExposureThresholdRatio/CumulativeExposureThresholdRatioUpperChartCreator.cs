using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureThresholdRatioUpperChartCreator : CumulativeExposureThresholdRatioChartCreatorBase {

        private MultipleExposureThresholdRatioSection _section;
        private bool _isUncertainty;

        public CumulativeExposureThresholdRatioUpperChartCreator(MultipleExposureThresholdRatioSection section, bool isUncertainty) {
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
            var RPFweightedRecord = _section.ExposureThresholdRatioRecords.FirstOrDefault(c => c.IsCumulativeRecord);

            var rpfWeightedRisk = RPFweightedRecord != null ? RPFweightedRecord.PUpperRiskNom : double.NaN;
            rpfWeightedRisk = !_section.UseIntraSpeciesFactor ? rpfWeightedRisk : double.NaN;

            var orderedHazardRecords = _section.ExposureThresholdRatioRecords
                .Where(c => !c.IsCumulativeRecord)
                .OrderByDescending(c => c.PUpperRiskNom)
                .ToList();

            var orderedExposureThresholdRatios = orderedHazardRecords.CumulativeWeights(c => c.PUpperRiskNom).ToList();
            var substances = orderedHazardRecords.Select(c => c.CompoundName).ToList();
            var percentage = 100 - (100 - _section.ConfidenceInterval) / 2;
            return create(orderedExposureThresholdRatios, substances, rpfWeightedRisk, percentage, _isUncertainty);
        }
    }
}
