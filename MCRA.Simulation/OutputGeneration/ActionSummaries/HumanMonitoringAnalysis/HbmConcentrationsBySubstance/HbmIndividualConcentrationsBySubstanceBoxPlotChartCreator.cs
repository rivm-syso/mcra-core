using MCRA.General;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase {

        private List<HbmConcentrationsPercentilesRecord> _records;
        private readonly ExposureTarget _target;
        private string _sectionId;
        private readonly string _unit;
        private bool _showOutLiers;


        public HbmIndividualConcentrationsBySubstanceBoxPlotChartCreator(
            List<HbmConcentrationsPercentilesRecord> records,
            ExposureTarget target,
            string sectionId,
            string unit, 
            bool showOutLiers
        ) {
            _records = records;
            _target = target;
            _sectionId = sectionId;
            _unit = unit;
            _showOutLiers = showOutLiers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "f40513c6-7e3a-46cd-903f-d2930c2dc8da";
                return StringExtensions.CreateFingerprint(_sectionId + pictureId + _target.Code);
            }
        }

        public override PlotModel Create() {
            return create(_records, $"Concentration ({_unit})", _showOutLiers);
        }
    }
}
