using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureHazardRatioMedianChartCreator : CumulativeExposureHazardRatioChartCreatorBase {

        public CumulativeExposureHazardRatioMedianChartCreator(
            CumulativeExposureHazardRatioSection section,
            bool isUncertainty
        ) : base(section, isUncertainty) {
        }

        public override string ChartId {
            get {
                var pictureId = "f362464f-6c8d-449f-8f27-d914fb4d6416";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var riskRecords = _section.RiskRecords.SelectMany(c => c.Records).ToList();
            var cumulativeRiskRecord = riskRecords
                .FirstOrDefault(c => c.IsCumulativeRecord);
            var cumulativeRisk = cumulativeRiskRecord != null
                ? (_isUncertainty ? cumulativeRiskRecord.RiskP50UncP50 : cumulativeRiskRecord.RiskP50Nom)
                : double.NaN;
            var orderedHazardRecords = riskRecords
                .Where(c => !c.IsCumulativeRecord)
                .OrderByDescending(c => c.RiskP50Nom)
                .ToList();

            var orderedExposureHazardRatios = _isUncertainty
                ? orderedHazardRecords.CumulativeWeights(c => c.RiskP50UncP50).ToList()
                : orderedHazardRecords.CumulativeWeights(c => c.RiskP50Nom).ToList();
            var substances = orderedHazardRecords.Select(c => c.SubstanceName).ToList();
            return create(
                orderedExposureHazardRatios,
                substances,
                cumulativeRisk,
                50
            );
        }
    }
}
