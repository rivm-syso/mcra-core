using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class IndividualConcentrationCorrelationCumulativeChartCreator : ConcentrationCorrelationsChartCreatorBase {

        protected IndividualConcentrationCorrelationsCumulativeSection _section;
        protected string _codeSubstance;
        protected string _modelledExposureUnit;
        protected string _monitoringConcentrationUnit;
        protected double _lowerPercentage;
        protected double _upperPercentage;
        private string _nameSubstance;

        public override string Title => $"{_nameSubstance}: monitoring versus modelled (p{_lowerPercentage}, p{50}, p{_upperPercentage}) exposures.";

        public override string ChartId {
            get {
                var chartId = "805d8179-0e5c-4a51-bb98-8375362935a0";
                return StringExtensions.CreateFingerprint(_section.SectionId + chartId + _codeSubstance);
            }
        }

        public IndividualConcentrationCorrelationCumulativeChartCreator(
            IndividualConcentrationCorrelationsCumulativeSection section,
            string codeSubstance,
            string modelledExposureUnit,
            string monitoringExposureUnit,
            double lowerPercentage,
            double upperPercentage,
            int width,
            int height
        ) : base(width, height) {
            _section = section;
            _codeSubstance = codeSubstance;
            _modelledExposureUnit = modelledExposureUnit;
            _monitoringConcentrationUnit = monitoringExposureUnit;
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            _nameSubstance = section.Records.First(r => r.SubstanceCode == codeSubstance).SubstanceName;
        }

        public override PlotModel Create() {
            return createPlotModel(_section, _codeSubstance, _modelledExposureUnit, _monitoringConcentrationUnit);
        }

        protected virtual PlotModel createPlotModel(
            IndividualConcentrationCorrelationsCumulativeSection section,
            string codeSubstance,
            string modelledExposureUnit,
            string monitoringConcentrationUnit
        ) {
            var record = section.Records.First(r => r.SubstanceCode == codeSubstance);
            var percentages = new double[] { _lowerPercentage, 50, _upperPercentage };
            var groupedExposures = record.MonitoringVersusModelExposureRecords
                .GroupBy(r => r.Individual)
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

            var plotModel = CreateChart(modelledExposureUnit, monitoringConcentrationUnit, groupedExposures);
            return plotModel;
        }
    }
}
