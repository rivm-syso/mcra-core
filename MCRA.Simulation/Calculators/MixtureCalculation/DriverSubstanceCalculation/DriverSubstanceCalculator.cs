using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;

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
                    var ix = items.IndexOf(maximum);
                    return new DriverSubstance() {
                        CumulativeExposure = cumulativeExposure,
                        MaximumCumulativeRatio = cumulativeExposure / maximum,
                        Compound = exposureMatrix.RowRecords[ix].Substance,
                        Target = exposureMatrix.RowRecords[ix].TargetUnit.Target
                    };
                })
            .ToList();
            return driverSubstances;
        }
    }
}
