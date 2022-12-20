using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.ComponentCalculation.DriverSubstanceCalculation {
    public static  class DriverSubstanceCalculator {

        public static List<DriverSubstance> CalculateExposureDrivers(ExposureMatrix exposureMatrix) {
            if (exposureMatrix == null) {
                return null;
            }

            var exposureTranspose = exposureMatrix.Exposures.Transpose();
            var driverSubstances = exposureTranspose.Array
                .AsParallel()
                .Select(c => {
                    var items = c.ToList();
                    var maximum = items.Max();
                    var cumulativeExposure = items.Sum();
                    return new DriverSubstance() {
                        CumulativeExposure = cumulativeExposure,
                        MaximumCumulativeRatio = cumulativeExposure / maximum,
                        Compound = exposureMatrix.Substances[items.IndexOf(maximum)],
                    };
                })
            .ToList();
            return driverSubstances;
        }
    }
}
