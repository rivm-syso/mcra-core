using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {
    public static class HbmSingleTargetExtrapolationCalculator {

        public static List<HbmIndividualDayCollection> Calculate(
           List<HbmIndividualDayCollection> hbmIndividualDayCollections,
           ICollection<KineticConversionFactorModel> kineticConversionFactorModels,
           List<SimulatedIndividualDay> simulatedIndividualDays,
           ICollection<Compound> substances,
           TargetLevelType targetLevelType,
           BiologicalMatrix targetMatrix
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
                targets = hbmIndividualDayCollections
                    .Where(r => r.Target.BiologicalMatrix == targetMatrix)
                    .Select(r => r.Target)
                    .ToList();
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
            foreach (var targetHbmIndividualDayCollection in targetHbmIndividualDayCollections) {
                var matrixConversionCalculator = new TargetMatrixKineticConversionCalculator(
                    kineticConversionFactorModels,
                    targetHbmIndividualDayCollection.TargetUnit);
                var monitoringOtherIndividualDayCalculator = new HbmIndividualDayMatrixExtrapolationCalculator(
                    matrixConversionCalculator
                );
                var collection = monitoringOtherIndividualDayCalculator
                    .Calculate(
                        targetHbmIndividualDayCollection,
                        hbmIndividualDayCollections,
                        simulatedIndividualDays,
                        substances
                    );
                convertedHbmIndividualDayCollections.Add(collection);
            }
            return convertedHbmIndividualDayCollections;
        }
    }
}
