using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.ReverseDoseCalculation {
    public class ReverseDoseCalculator {

        private double _conversionFactorApproximation;

        public double Precision { get; set; } = 0.001;
        public int MaxIter { get; set; } = 1000;

        public ReverseDoseCalculator() {
            _conversionFactorApproximation = 1;
        }

        /// <summary>
        /// Reverse dose calculation using bisection search to find the external
        /// dose corresponding to the specified internal dose. The kinetic model
        /// is applied using nominal values (i.e., without variability).
        /// </summary>
        public double Reverse(
            IPbkModelCalculator pbkModelCalculator,
            SimulatedIndividual individual,
            double dose,
            TargetUnit internalDoseUnit,
            ExposureRoute externalExposureRoute,
            ExposureUnitTriple externalExposureUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            double f(double x) {
                var externalExposure = ExternalIndividualDayExposure
                    .FromSingleDose(
                        externalExposureRoute,
                        pbkModelCalculator.Substance,
                        x,
                        externalExposureUnit,
                        individual
                    );
                var simulationOutput = pbkModelCalculator.Calculate(
                    new List<(SimulatedIndividual, List<IExternalIndividualDayExposure>)> {
                        (externalExposure.SimulatedIndividual, 
                        [ externalExposure ]
                    )},
                    externalExposureUnit,
                    [externalExposureRoute],
                    [internalDoseUnit],
                    exposureType,
                    generator
                );
                var targetTimeSeries = simulationOutput[0].SubstanceTargetLevelTimeSeries.First();
                var targetDose = exposureType == ExposureType.Acute
                    ? targetTimeSeries.ComputePeakTargetExposure(
                        pbkModelCalculator.SimulationSettings.NonStationaryPeriod
                    )
                    : targetTimeSeries.ComputePeakTargetExposure(
                        pbkModelCalculator.SimulationSettings.NonStationaryPeriod
                    );
                var fObj = targetDose - dose;
                return fObj;
            }
            var (xopt, fopt) = optimise(
                f,
                0D,
                dose / _conversionFactorApproximation,
                Precision,
                MaxIter
            );
            if (xopt > 0) {
                var lr = 0.9;
                var kf = (fopt + dose) / xopt;
                _conversionFactorApproximation = lr * _conversionFactorApproximation + (1-lr) * kf;
            }
             return xopt;
        }

        private static (double xopt, double fopt) optimise(
            Func<double, double> f,
            double a,
            double b,
            double tol = 1e-6,
            int maxIter = 1000
        ) {
            var evalCount = 0;

            var fa = double.NaN;
            var fb = f(b);

            // First, find rough upper (and lower) bound
            while (fb < 0 && evalCount < maxIter) {
                a = b;
                fa = fb;
                b *= 2;
                fb = f(b);
                evalCount++;
            }

            if (Math.Abs(fb) <= tol) {
                // If fb is already a good solution, then stop and return
                return (b, fb);
            }

            if (fb < 0) {
                // MaxIter reached, fb still not positive, throw exception
                throw new ArgumentException("Cannot find any dose high enough to produce the target dose.");
            }

            // If not already computed during upper bound finding
            if (double.IsNaN(fa)) {
                fa = f(a);
                evalCount++;
            }

            // From here, do bisection search
            var mid = 0D;
            var fm = double.PositiveInfinity;
            while (Math.Abs(fm) > tol && evalCount < maxIter) {
                mid = (a + b) / 2.0;
                fm = f(mid);
                evalCount++;
                if (fa * fm < 0) {
                    b = mid;
                    fb = fm;
                } else {
                    a = mid;
                    fa = fm;
                }
            }

            // Return best guess after max iterations
            return (mid, fm);
        }
    }
}
