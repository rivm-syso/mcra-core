using System.Collections.ObjectModel;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace MCRA.Simulation.OutputGeneration {
    public class KineticModelTimeCourseChartCreator : ReportLineChartCreatorBase {

        private readonly string _id;
        private readonly string _intakeUnit;
        private readonly KineticModelTimeCourseSection _section;
        private readonly PBKDrilldownRecord _internalExposures;

        public KineticModelTimeCourseChartCreator(
            PBKDrilldownRecord internalExposures,
            KineticModelTimeCourseSection section,
            string intakeUnit
        ) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
            _id = internalExposures.IndividualCode;
            _internalExposures = internalExposures;
        }

        public override string Title => $"Compartment: {_internalExposures.BiologicalMatrix}";

        public override string ChartId {
            get {
                var pictureId = "7ffc4ee2-eeea-4800-9b5d-965409b78411";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _id + _internalExposures.BiologicalMatrix);
            }
        }

        public override PlotModel Create() {
            return create(_intakeUnit);
        }

        private PlotModel create(string intakeUnit) {
            var xtitle = $"Time (days)";
            var ytitle = $"Exposure ({intakeUnit})";
            var xValues = _internalExposures.TargetExposures.Select(c => c.Time).ToList();
            var yValues = _internalExposures.TargetExposures.Select(c => c.Exposure).ToList();
            return createPlotModel(
                xValues,
                yValues,
                xtitle, ytitle,
                _section.EvaluationFrequency,
                _section.TimeScale,
                false
            );
        }

        /// <summary>
        /// To get nice x-axis (multiples of 24 hours) the variable interval must be even.
        /// Steplength is related to resolution, so stepLength 2 means every 2 minutes if 
        /// resolution is in hours stepLength 60 means every 60 minutes (1 hour) if resolution 
        /// is in hours.
        /// </summary>
        private PlotModel createPlotModel(
            List<double> xValues,
            List<double> yValues,
            string xtitle,
            string ytitle,
            int evaluationFrequency,
            TimeUnit timeScale,
            bool useGlobalYMax
        ) {
            var plotModel = createDefaultPlotModel();

            double timeMultiplier;
            if (timeScale == TimeUnit.Days) {
                timeMultiplier = 1;
            } else if (timeScale == TimeUnit.Hours) {
                timeMultiplier = 24;
            } else if (timeScale == TimeUnit.Minutes) {
                timeMultiplier = 24 * 60;
            } else {
                throw new NotImplementedException();
            }

            var timeSpanAxis = new LinearAxis() {
                Position = AxisPosition.Bottom,
                Title = xtitle,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(timeSpanAxis);

            var verticalAxis = createLinearAxis(ytitle);
            verticalAxis.Maximum = useGlobalYMax 
                ? _section.Maximum
                : yValues.DefaultIfEmpty(1).Max() * 1.05;
            verticalAxis.Minimum = 0;
            plotModel.Axes.Add(verticalAxis);

            // Internal (target) exposure time series
            var series = new LineSeries() {
                Color = OxyColors.Red,
                MarkerType = MarkerType.None,
                StrokeThickness = 1.5,
            };
            for (var i = 0; i < xValues.Count; i++) {
                series.Points.Add(new DataPoint(xValues[i] / timeMultiplier, yValues[i]));
            }
            plotModel.Series.Add(series);

            // Internal (target) exposure reference line
            var referenceLineAnnotation = new LineAnnotation() {
                Type = LineAnnotationType.Horizontal,
                Y = _internalExposures.TargetExposure,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.RoyalBlue,
                StrokeThickness = 1.5
            };
            plotModel.Annotations.Add(referenceLineAnnotation);

            // Vertical burn-in period reference line
            var burnInPeriodLineAnnotation = new LineAnnotation() {
                Type = LineAnnotationType.Vertical,
                X = _section.NumberOfDaysSkipped,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Black,
                StrokeThickness = 1
            };
            plotModel.Annotations.Add(burnInPeriodLineAnnotation);

            return plotModel;
        }
    }
}
