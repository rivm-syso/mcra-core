using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {
    public static class HbmSingleTargetExtrapolationCalculator {

        public static List<HbmIndividualDayCollection> Calculate(
           List<SimulatedIndividualDay> simulatedIndividualDays,
           List<HbmIndividualDayCollection> hbmIndividualDayCollections,
           ICollection<Compound> substances,
           ExposureType exposureType,
           TargetLevelType targetLevelType,
           BiologicalMatrix targetMatrix,
           Func<TargetMatrixKineticConversionCalculator> matrixConversionCalculatorFactory,
           IRandom generator,
           CompositeProgressState progress
        ) {
            // Here we assume that we have selected one matrix to which we want to convert all
            // concentrations. However, notice that we could still end up with multiple target units
            // because of the inclusion of different expression types (e.g., blood concentrations
            // as ug/L and ug/g lipids).

            // Get target surface level(s)
            var targets = new List<ExposureTarget>();
            if (targetLevelType == TargetLevelType.External) {
                targets.Add(new ExposureTarget(ExposureRoute.Oral));
            } else {
                targets = [.. hbmIndividualDayCollections
                    .Where(r => r.Target.BiologicalMatrix == targetMatrix)
                    .Select(r => r.Target)];
                if (!targets.Any()) {
                    targets.Add(new ExposureTarget(targetMatrix));
                }
            }

            // If no HBM individual day collection(s) was/were constructed from the HBM data,
            // then we need to construct it/them here.
            var targetHbmIndividualDayCollections = hbmIndividualDayCollections
                .Where(r => targets.Contains(r.Target))
                .ToList();
            foreach (var target in targets) {
                if (!targetHbmIndividualDayCollections.Any(r => r.Target == target)) {
                    var defaultCollection = HbmIndividualDayConcentrationsCalculator
                        .CreateDefaultHbmIndividualDayCollection(simulatedIndividualDays, target);
                    targetHbmIndividualDayCollections.Add(defaultCollection);
                }
            }

            // Loop over the target collections and do the imputation via kinetic conversion
            var convertedHbmIndividualDayCollections = new List<HbmIndividualDayCollection>();
            var collection = new HbmIndividualDayCollection();

            // Initialize kinetic model calculators
            var matrixConversionCalculator = matrixConversionCalculatorFactory();

            foreach (var targetHbmIndividualDayCollection in targetHbmIndividualDayCollections) {
                var monitoringOtherIndividualDayCalculator = new HbmIndividualDayMatrixExtrapolationCalculator(
                    matrixConversionCalculator
                );
                collection = monitoringOtherIndividualDayCalculator
                    .Calculate(
                        targetHbmIndividualDayCollection,
                        hbmIndividualDayCollections,
                        simulatedIndividualDays,
                        substances,
                        exposureType,
                        progress.NewCompositeState(100D / targetHbmIndividualDayCollections.Count),
                        generator
                    );
                convertedHbmIndividualDayCollections.Add(collection);
            }
            return convertedHbmIndividualDayCollections;
        }
    }
}
