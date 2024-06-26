﻿using MCRA.General;
using MCRA.Utils.Statistics;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.SingleValueRisks {

    public class AFDensityChartCreatorBase : ReportLineChartCreatorBase {

        public AdjustmentFactorDistributionMethod _adjustmentFactorDistributionMethod;
        public SingleValueRisksAdjustmentFactorsSection _section;
        public double _a;
        public double _b;
        public double _c;
        public double _d;
        public bool _isExposure;
        public string _title;
        public RiskMetricType _riskMetric;

        public AFDensityChartCreatorBase(SingleValueRisksAdjustmentFactorsSection section, bool isExposure) {
            Width = 500;
            Height = 300;
            _section = section;
            _adjustmentFactorDistributionMethod = isExposure ? section.ExposureAdjustmentFactorDistributionMethod : section.HazardAdjustmentFactorDistributionMethod;
            _a = isExposure ? section.ExposureParameterA : section.HazardParameterA;
            _b = isExposure ? section.ExposureParameterB : section.HazardParameterB;
            _c = isExposure ? section.ExposureParameterC : section.HazardParameterC;
            _d = isExposure ? section.ExposureParameterD : section.HazardParameterD;
            _isExposure = isExposure;
            _title = createTitle();
        }

        public override string ChartId => throw new NotImplementedException();

        public override PlotModel Create() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// BetaScaled
        /// </summary>
        /// <param name="beta"></param>
        /// <param name="location"></param>
        /// <param name="scale"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public (List<double>, List<double>) GetPDF(BetaDistribution beta, double location, double scale, int steps) {
            var pdf = new List<double>();
            var x_as = new List<double>();
            var delta = 1d / steps;
            var ini = 0d;
            var range = scale - location;
            for (int i = 0; i < steps + 5; i++) {
                var point = BetaDistribution.Density(ini, beta.ShapeA, beta.ShapeB) / range;
                if (!double.IsInfinity(point)) {
                    x_as.Add(ini * range + location);
                    pdf.Add(point);
                }
                ini += delta;
            }
            return (x_as, pdf);
        }

        /// <summary>
        /// Gamma scaled
        /// </summary>
        /// <param name="gamma"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public (List<double>, List<double>) GetPDF(GammaDistribution gamma, double location, int steps) {
            var pdf = new List<double>();
            var x_as = new List<double>();
            var delta = (5 * gamma.Shape / gamma.Rate) / steps;
            var ini = 0D;
            for (int i = 0; i < steps + 5; i++) {
                var point = GammaDistribution.Density(ini, gamma.Shape, gamma.Rate);
                if (!double.IsInfinity(point)) {
                    x_as.Add(ini + gamma.Scale + location);
                    pdf.Add(point);
                }
                ini += delta;
            }
            return (x_as, pdf);
        }

        /// <summary>
        /// LogNormal scaled
        /// </summary>
        /// <param name="lognormal"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public (List<double>, List<double>) GetPDF(LogNormalDistribution lognormal, int steps) {
            var pdf = new List<double>();
            var x_as = new List<double>();
            var delta = Math.Exp(lognormal.Mu + 4 * lognormal.Sigma * lognormal.Sigma) / steps;
            var ini = 0D;
            for (int i = 0; i < steps + 5; i++) {
                var point = LogNormalDistribution.Density(ini, lognormal.Mu, lognormal.Sigma);
                if (!double.IsInfinity(point)) {
                    x_as.Add(ini + lognormal.Offset);
                    pdf.Add(point);
                }
                ini += delta;
            }
            return (x_as, pdf);
        }

        /// <summary>
        /// LogStudent t scaled
        /// </summary>
        /// <param name="student"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public (List<double>, List<double>) GetPDF(LogStudentTScaledDistribution student, int steps) {
            var pdf = new List<double>();
            var x_as = new List<double>();
            var delta = Math.Exp(student.Location + 8 * student.Scale * student.Scale) / steps;
            var ini = 0D;
            for (int i = 0; i < steps + 5; i++) {
                var point = getLogStudentDensity(student.Location, student.Scale, student.Freedom, ini);
                if (!double.IsInfinity(point)) {
                    x_as.Add(ini + student.Offset);
                    pdf.Add(point);
                }
                //pdf.Add(Math.Exp(student.Density(ini)) - 1);
                ini += delta;
            }
            return (x_as, pdf);
        }

        /// <summary>
        /// https://stats.stackexchange.com/questions/325154/log-student-t-distribution-calculated-mean
        /// file:///C:/LocalD/Data/Articles/On%20the%20Derivation%20of%20the%20SIA-log-Student-t%20Distribution.pdf
        /// </summary>
        /// <param name="location"></param>
        /// <param name="scale"></param>
        /// <param name="freedom"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        private double getLogStudentDensity(double location, double scale, double freedom, double x) {
            var arg1 = MathNet.Numerics.SpecialFunctions.Gamma((freedom + 1) / 2) / (x * MathNet.Numerics.SpecialFunctions.Gamma(freedom / 2) * scale * Math.Sqrt(freedom * Math.PI));
            var arg2 = Math.Pow((Math.Log(x) - location) / scale, 2);
            var result = arg1 * Math.Pow((1 + 1 / freedom * arg2), -(freedom + 1) / 2);
            return result;
        }

        protected PlotModel CreateDensity() {
            var steps = 100;
            var x_as = new List<double>();
            var pdf = new List<double>();

            if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta) {
                var distribution = new BetaDistribution(_a, _b);
                (x_as, pdf) = GetPDF(distribution, _c, _d, steps);
            } else if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Gamma) {
                var distribution = new GammaDistribution(_a, _b);
                (x_as, pdf) = GetPDF(distribution, _c, steps);
            } else if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogNormal) {
                var distribution = new LogNormalDistribution(_a, _b);
                (x_as, pdf) = GetPDF(distribution,  steps);
            } else if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                var distribution = new LogStudentTScaledDistribution(_a, _b, _c, _d);
                (x_as, pdf) = GetPDF(distribution,  steps);
            }

            var plotModel = createDefaultPlotModel();
            var lineSeries = createDefaultLineSeries();
            lineSeries.StrokeThickness = 2;
            for (int i = 0; i < x_as.Count; i++) {
                if (!double.IsInfinity(pdf[i])) {
                    lineSeries.Points.Add(new DataPoint(x_as[i], pdf[i]));
                }
            }
            plotModel.Series.Add(lineSeries);

            var neutralReferenceLineAnnotation = new LineAnnotation() {
                Type = LineAnnotationType.Vertical,
                X = 1,
                LineStyle = LineStyle.Solid,
                Color = OxyColors.Red,
                StrokeThickness = 2,
            };
            plotModel.Annotations.Add(neutralReferenceLineAnnotation);

            var linearAxis = createDefaultBottomLinearAxis();
            var xtitle = _isExposure ? "exposure" : "hazard";
            linearAxis.Title = $"Adjustment factor related to {xtitle} uncertainties";
            linearAxis.Minimum = 0;
            linearAxis.Maximum = x_as.Max() * 1.05;
            plotModel.Axes.Add(linearAxis);

            var verticalAxis = new LinearAxis {
                Title = "Probability density",
                Minimum = 0,
                Maximum = pdf.Max() * 1.05
            };
            plotModel.Axes.Add(verticalAxis);

            return plotModel;
        }

        private string createTitle() {
            var distribution = string.Empty;
            if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Beta) {
                distribution = $"adjustment factor distribution scaled Beta (a={_a}, b={_b}, lower bound={_c}, upper bound={_d})";
            } else if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.Gamma) {
                distribution = $"adjustment factor distribution scaled Gamma (a={_a}, b={_b}, offset={_c})";
            } else if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogNormal) {
                distribution = $"adjustment factor distribution scaled LogNormal (mu={_a}, stdev={_b}, offset={_c})";
            } else if (_adjustmentFactorDistributionMethod == AdjustmentFactorDistributionMethod.LogStudents_t) {
                distribution = $"adjustment factor distribution scaled LogStudents t (mu={_a}, stdev={_b}, v={_c}, offset={_d})";
            }
            var exposureOrRisk = _isExposure ? "exposure" : "hazard";
            var title = _isExposure ? $"Exposure {distribution}." : $"Hazard {distribution}.";
            title += $" The red line marks a factor of 1 indicating the point where resolving the uncertainties will not change the calculated {exposureOrRisk} estimates.";
            return title;
        }
    }
}
