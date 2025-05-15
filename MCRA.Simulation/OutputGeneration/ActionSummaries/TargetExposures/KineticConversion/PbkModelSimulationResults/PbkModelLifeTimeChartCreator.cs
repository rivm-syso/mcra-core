using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public class PbkModelLifeTimeChartCreator : TimeCourseChartCreatorBase {

        private readonly string _id;
        private readonly PbkModelTimeCourseSection _section;
        private readonly PbkModelTimeCourseDrilldownRecord _internalExposures;
        private readonly string _bodyWeightUnit;

        public PbkModelLifeTimeChartCreator(
            PbkModelTimeCourseDrilldownRecord internalExposures,
            PbkModelTimeCourseSection section,
            string bodyWeightUnit
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _id = internalExposures.IndividualCode;
            _internalExposures = internalExposures;
            _bodyWeightUnit = bodyWeightUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "09b11006-2b82-4cc4-a37c-00c8d00cfe0b";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _id + _internalExposures.BiologicalMatrix);
            }
        }

        public override PlotModel Create() {
            var xtitle = $"Time (days)";
            var ytitle = $"Body weight ({_bodyWeightUnit})";
            var xValues = _internalExposures.TargetExposures.Select(c => c.Time).ToList();
            var yValues = _internalExposures.TargetExposures.Select(c => (double)c.BodyWeight).ToList();
            return createPlotModel(
                xValues,
                yValues,
                _section.TimeScale,
                xtitle,
                ytitle,
                _section.Maximum,
                _section.NumberOfDaysSkipped,
                false
            );
        }
    }
}
