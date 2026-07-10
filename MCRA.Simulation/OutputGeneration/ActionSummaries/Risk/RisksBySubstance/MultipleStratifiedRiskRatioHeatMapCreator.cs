using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MultipleStratifiedRiskRatioHeatMapCreator : StratifiedRiskRatioHeatMapCreatorBase {

        private readonly MultipleRiskRatioSection _section;
        private readonly TargetUnit _targetUnit;
        private readonly bool _isUncertainty;
        private readonly List<SubstanceRiskDistributionRecord> _records;

        public override string ChartId {
            get {
                var pictureId = "30387922-c986-4b96-a27b-56fa930c7446";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _targetUnit.BiologicalMatrix.ToString());
            }
        }

        public MultipleStratifiedRiskRatioHeatMapCreator(
            MultipleRiskRatioSection section,
            TargetUnit targetUnit,
            bool isUncertainty
        ) {
            _section = section;
            _targetUnit = targetUnit;
            _isUncertainty = isUncertainty;
            _records = [.. _section.StratifiedMultipleRiskRecords
                .SingleOrDefault(c => c.Target == _targetUnit?.Target)
                .Records
                .Where(c => _section.RiskMetric == RiskMetricType.HazardExposureRatio 
                    ? c.PLowerRiskNom < _section.RightMargin && !string.IsNullOrEmpty(c.Stratification)
                    : c.PUpperRiskNom > _section.LeftMargin && !string.IsNullOrEmpty(c.Stratification))];
            Height = 150 + _records.Count * 20;
            Width = 1000;
        }

        public override PlotModel Create() {
            return create(
                _records,
                _section.RiskMetric,
                _section.LeftMargin,
                _section.RightMargin,
                _section.Threshold,
                _isUncertainty,
                _targetUnit,
                _section.ReferenceDose,
                false,
                _section.StratificationVariables
            );
        }
    }
}
