using MCRA.General;
using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleRiskRatioHeatMapCreator : RiskRatioHeatMapCreatorBase {

        private readonly MultipleRiskRatioSection _section;
        private readonly List<SubstanceRiskDistributionRecord> _records;
        private readonly TargetUnit _targetUnit;
        private readonly bool _isUncertainty;

        public override string ChartId {
            get {
                var pictureId = "6f72093c-a9ef-410b-9ba4-561cd1c88321";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _targetUnit?.BiologicalMatrix.ToString());
            }
        }

        public MultipleRiskRatioHeatMapCreator(
            MultipleRiskRatioSection section,
            TargetUnit targetUnit,
            bool isUncertainty
        ) {
            _section = section;
            _targetUnit = targetUnit;
            _isUncertainty = isUncertainty;
            var records = _section.MultipleRiskRecords.SingleOrDefault(c => c.Target == _targetUnit?.Target).Records;
            records = [.. records.Where(c => string.IsNullOrEmpty(c.StratificationLevelCode))];
            _records = filterAndSortRecords(records, _section.RiskMetric, _section.LeftMargin, _section.RightMargin);
            Height = 150 + _records.Count * 20;
        }

        public override PlotModel Create() {
            var threshold = _section.Threshold;
            var xlow = _section.LeftMargin;
            var xhigh = _section.RightMargin;
            return create(
                xlow,
                xhigh,
                threshold,
                _records,
                _isUncertainty,
                _section.RiskMetric
            );
        }

        private static List<SubstanceRiskDistributionRecord> filterAndSortRecords(
            List<SubstanceRiskDistributionRecord> records,
            RiskMetricType riskMetric,
            double xLow,
            double xHigh
        ) {
            records = [.. records.Where(c => c.PercentagePositives > 0)];

            var rpfWeightedRecord = records.FirstOrDefault(c => c.IsCumulativeRecord);
            if (rpfWeightedRecord != null) {
                records.Remove(rpfWeightedRecord);
            }
            if (riskMetric == RiskMetricType.ExposureHazardRatio) {
                records = records.Any(c => c.RiskP50UncP50 > 0)
                    ? [.. records.OrderBy(c => c.PUpperRiskUncUpper).Where(c => c.PUpperRiskUncUpper > xLow)]
                    : [.. records.OrderBy(c => c.PUpperRiskNom).Where(c => c.PUpperRiskNom > xLow)];
            } else {
                records = records.Any(c => c.RiskP50UncP50 > 0)
                    ? [.. records.OrderByDescending(c => c.PLowerRiskUncLower).Where(c => c.PLowerRiskUncLower < xHigh)]
                    : [.. records.OrderByDescending(c => c.PLowerRiskNom).Where(c => c.PLowerRiskNom < xHigh)];
            }
            if (rpfWeightedRecord != null) {
                records.Add(rpfWeightedRecord);
            }

            return records;
        }
    }
}
