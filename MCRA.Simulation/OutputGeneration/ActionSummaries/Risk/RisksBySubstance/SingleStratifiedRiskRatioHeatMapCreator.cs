using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleStratifiedRiskRatioHeatMapCreator : StratifiedRiskRatioHeatMapCreatorBase {

        private readonly SingleRiskRatioSection _section;
        private readonly bool _isUncertainty;
        private readonly List<SubstanceRiskDistributionRecord> _records;

        public override string ChartId {
            get {
                var pictureId = "30387922-c986-4b96-a27b-56fa930c7446";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public SingleStratifiedRiskRatioHeatMapCreator(
            SingleRiskRatioSection section,
            TargetUnit targetUnit,
            bool isUncertainty
        ) {
            _section = section;
            _isUncertainty = isUncertainty;
            _records = _section.StratifiedSingleRiskRecords
                .Where(c => _section.RiskMetric == RiskMetricType.HazardExposureRatio
                    ? c.PLowerRiskNom < _section.RightMargin
                    : c.PUpperRiskNom > _section.LeftMargin)
                .ToList();
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
                _section.TargetUnit,
                _section.ReferenceDose,
                true,
                _section.StratificationVariables
            );
        }
    }
}
