using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.ObjectModel;

namespace MCRA.Simulation.OutputGeneration {
    public class PBPKChartCreator : LineChartCreatorBase {

        private KineticModelTimeCourseSection _section;
        private string _intakeUnit;
        private string _id;
        private InternalExposuresPerIndividual _internalExposures;

        public PBPKChartCreator(InternalExposuresPerIndividual InternalExposures, KineticModelTimeCourseSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
            _id = InternalExposures.Code;
            _internalExposures = InternalExposures;
        }

        public override string Title => $"Model {_section.ModelCode}";


        public override string ChartId {
            get {
                var pictureId = "7ffc4ee2-eeea-4800-9b5d-965409b78411";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _id);
            }
        }

        public override PlotModel Create() {
            return create(_intakeUnit);
        }

        private PlotModel create(string intakeUnit) {
            var xtitle = $"Time (days)";
            var ytitle = $"Exposure ({intakeUnit})";
            var xValues = _internalExposures.TargetExposures.Select(c => c.Time).ToList();
            var yValues = _internalExposures.TargetExposures.Select(c => c.Exposure / _internalExposures.CompartmentWeight).ToList();
            return createPlotModel(xValues, yValues, xtitle, ytitle, _section.StepLength, false);
        }

        /// <summary>
        /// To get nice x-axis (multiples of 24 hours) the variable interval must be even
        /// </summary>
        /// <param name="xValues"></param>
        /// <param name="yValues"></param>
        /// <param name="xtitle"></param>
        /// <param name="ytitle"></param>
        /// <param name="stepLength">
        /// is related to resolution, so stepLength 2 means every 2 minutes if resolution is in hours
        /// stepLength 60 means every 60 minutes (1 hour) if resolution is in hours
        /// </param>
        /// <param name="useGlobalYMax"></param>
        /// <returns></returns>
        private PlotModel createPlotModel(
            List<int> xValues,
            List<double> yValues,
            string xtitle,
            string ytitle,
            int stepLength,
            bool useGlobalYMax
        ) {
            var plotModel = base.createDefaultPlotModel(string.Empty);
            var maximumTickNumber = 11;

            var resolution = 60d;
            var nDays = xValues.Count * stepLength / resolution / 24;

            double interval;
            if (stepLength < resolution) {
                interval = Math.Ceiling(xValues.Count * stepLength / resolution / 60 / maximumTickNumber);
            } else {
                interval = Math.Ceiling(nDays / maximumTickNumber);
            }
            if (interval % 2 != 0 && nDays > maximumTickNumber) {
                interval++;
            }

            var timeSpanAxis = new TimeSpanAxis() {
                Position = AxisPosition.Bottom,
                StringFormat = "%d",
                Title = xtitle,
                MajorStep = 3600 * 24 * interval,
                MinorStep = 3600 * 12 * interval,
                MajorGridlineStyle = LineStyle.Dash,
            };
            plotModel.Axes.Add(timeSpanAxis);

            var verticalAxis = createLinearAxis(ytitle);
            verticalAxis.Maximum = useGlobalYMax ? _section.Maximum : yValues.DefaultIfEmpty(1).Max() * 1.05;
            verticalAxis.Minimum = 0;
            plotModel.Axes.Add(verticalAxis);

            var data = new Collection<object>();
            if (stepLength < resolution) {
                for (int i = 0; i < xValues.Count; i++) {
                    data.Add(new {
                        Time = new TimeSpan(0, 0, xValues[i] * stepLength, 0),
                        Value = yValues[i]
                    });
                }
            } else {
                for (int i = 0; i < xValues.Count; i++) {
                    data.Add(new {
                        Time = new TimeSpan(0, xValues[i], 0, 0),
                        Value = yValues[i]
                    });
                }
            }

            var series = new LineSeries() {
                Color = OxyColors.Red,
                MarkerType = MarkerType.None,
                //MarkerSize = .25,
                DataFieldX = "Time",
                DataFieldY = "Value",
                ItemsSource = data,
                StrokeThickness = 1.5,
            };

            plotModel.Series.Add(series);

            var target = new Collection<object>();
            var peakTarget = new Collection<object>();
            if (stepLength < resolution) {
                target.Add(new {
                    Time = new TimeSpan(0, 0, xValues[0] * stepLength, 0),
                    Value = _internalExposures.InternalLongTermTargetConcentration
                });
                target.Add(new {
                    Time = new TimeSpan(0, 0, xValues.Last() * stepLength, 0),
                    Value = _internalExposures.InternalLongTermTargetConcentration
                });
                peakTarget.Add(new {
                    Time = new TimeSpan(0, 0, xValues[0] * stepLength, 0),
                    Value = _internalExposures.InternalPeakTargetConcentration
                });
                peakTarget.Add(new {
                    Time = new TimeSpan(0, 0, xValues.Last() * stepLength, 0),
                    Value = _internalExposures.InternalPeakTargetConcentration
                });
            } else {
                if (_internalExposures.TargetExposure != _internalExposures.PeakTargetExposure) {
                    // chronic blue
                    target.Add(new {
                        Time = new TimeSpan(0, xValues[0], 0, 0),
                        Value = _internalExposures.InternalLongTermTargetConcentration
                    });
                    target.Add(new {
                        Time = new TimeSpan(0, xValues.Last(), 0, 0),
                        Value = _internalExposures.InternalLongTermTargetConcentration
                    });
                } else {
                    // acute green
                    peakTarget.Add(new {
                        Time = new TimeSpan(0, xValues[0], 0, 0),
                        Value = _internalExposures.InternalPeakTargetConcentration
                    });
                    peakTarget.Add(new {
                        Time = new TimeSpan(0, xValues.Last(), 0, 0),
                        Value = _internalExposures.InternalPeakTargetConcentration
                    });
                }
            }

            var lineSeries1 = new LineSeries() {
                Color = OxyColors.RoyalBlue,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1.5,
                DataFieldX = "Time",
                DataFieldY = "Value",
                ItemsSource = target,
            };
            plotModel.Series.Add(lineSeries1);

            var lineSeries2 = new LineSeries() {
                Color = OxyColors.Green,
                LineStyle = LineStyle.Solid,
                StrokeThickness = 1.5,
                DataFieldX = "Time",
                DataFieldY = "Value",
                ItemsSource = peakTarget,
            };
            plotModel.Series.Add(lineSeries2);


            if (_section.NumberOfDaysSkipped > 0) {
                var skipPeriod = new Collection<object> {
                    new {
                        Time = new TimeSpan(0, _section.NumberOfDaysSkipped * 24, 0, 0),
                        Value = 0
                    },
                    new {
                        Time = new TimeSpan(0, _section.NumberOfDaysSkipped * 24, 0, 0),
                        Value = _section.Maximum
                    }
                };
                var lineSeries3 = new LineSeries() {
                    Color = OxyColors.Black,
                    LineStyle = LineStyle.Solid,
                    StrokeThickness = 1,
                    DataFieldX = "Time",
                    DataFieldY = "Value",
                    ItemsSource = skipPeriod,
                };
                plotModel.Series.Add(lineSeries3);
            }
            return plotModel;
        }
    }
}
