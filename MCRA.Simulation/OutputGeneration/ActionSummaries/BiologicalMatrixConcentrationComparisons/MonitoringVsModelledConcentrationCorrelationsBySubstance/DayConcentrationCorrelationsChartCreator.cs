using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class DayConcentrationCorrelationsChartCreator : ConcentrationCorrelationsChartCreatorBase {

        protected DayConcentrationCorrelationsBySubstanceSection _section;
        protected string _codeSubstance;
        protected TargetUnit _targetUnit;
        protected string _monitoringConcentrationUnit;
        protected double _lowerPercentage;
        protected double _upperPercentage;
        private string _nameSubstance;
        public override string Title => $"{_nameSubstance}: monitoring versus modelled (p{_lowerPercentage}, p{50}, p{_upperPercentage}) exposures.";

        public DayConcentrationCorrelationsChartCreator(
            DayConcentrationCorrelationsBySubstanceSection section,
            string codeSubstance,
            TargetUnit targetUnit,
            string monitoringExposureUnit,
            double lowerPercentage,
            double upperPercentage,
            int width,
            int height
        ) : base(width, height) {
            _section = section;
            _codeSubstance = codeSubstance;
            _targetUnit = targetUnit;
            _monitoringConcentrationUnit = monitoringExposureUnit;
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _nameSubstance = section.Records.First(r => r.SubstanceCode == codeSubstance).SubstanceName;
        }

        public override string ChartId {
            get {
                var chartId = "DE1A428D-03F9-4C4B-B749-51ED6C1FC974";
                return StringExtensions.CreateFingerprint(_section.SectionId + chartId + _codeSubstance);
            }
        }

        public override PlotModel Create() {
            return createPlotModel(_section, _codeSubstance, _targetUnit, _monitoringConcentrationUnit);
        }

        protected virtual PlotModel createPlotModel(DayConcentrationCorrelationsBySubstanceSection section, string codeSubstance, TargetUnit targetUnit, string monitoringConcentrationUnit) {
            var record = section.Records.First(r => r.SubstanceCode == codeSubstance);

            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var groupedExposures = record.MonitoringVersusModelExposureRecords
                .GroupBy(r => (r.Individual, r.Day))
                .Select(g => {
                    var modelled = g.Select(r => r.ModelledExposure).ToList();
                    var monitoring = g.Select(r => r.MonitoringConcentration).Average();
                    var modelledPercentiles = modelled.Percentiles(percentages);
                    var modelledMedian = modelledPercentiles[1];
                    return (
                        NumRecords: g.Count(),
                        BothPositive: modelledMedian > 0 && monitoring > 0,
                        BothZero: modelledMedian <= 0 && monitoring <= 0,
                        ZeroMonitoring: modelledMedian > 0 && monitoring <= 0,
                        ZeroModelled: modelledMedian <= 0 && monitoring > 0,
                        Monitoring: monitoring,
                        ModelledMedian: modelledMedian,
                        ModelledPercentiles: g.Count() > 1
                            ? modelledPercentiles
                            : [double.NaN, modelled.First(), double.NaN]
                    );
                });
            var plotModel = CreateChart(targetUnit, monitoringConcentrationUnit, groupedExposures);
            return plotModel;
        }
    }
}
