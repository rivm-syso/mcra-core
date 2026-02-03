using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.General;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {
    public static class HbmMultipleTargetExtrapolationCalculator {

        public static List<HbmIndividualDayCollection> Calculate(
           List<HbmIndividualDayCollection> hbmIndividualDayCollections,
           ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
           List<SimulatedIndividualDay> simulatedIndividualDays,
           ICollection<Compound> substances,
           ExposureType exposureType,
           CompositeProgressState progressState
        ) {
            var targetsTo = kineticConversionFactorModels
                .Select(m => m.TargetTo)
                .Distinct()
                .ToList();

            var convertedHbmIndividualDayCollections = hbmIndividualDayCollections
                .Where(c => !targetsTo.Contains(c.Target))
                .ToList();

            var matrixConversionCalculator = new TargetMatrixKineticConversionCalculator(
                kineticConversionFactorModels
            );
            var monitoringOtherIndividualDayCalculator = new HbmIndividualDayMatrixExtrapolationCalculator(
                matrixConversionCalculator
            );

            foreach (var targetTo in targetsTo) {
                var collectionTo = hbmIndividualDayCollections.FirstOrDefault(c => c.Target == targetTo)
                    ?? HbmIndividualDayConcentrationsCalculator
                        .CreateDefaultHbmIndividualDayCollection(simulatedIndividualDays, targetTo);

                var conversionModelsFrom = kineticConversionFactorModels
                    .Where(m => m.TargetTo == collectionTo.Target)
                    .Select(m => m.TargetFrom)
                    .ToList();
                var collectionsFrom = hbmIndividualDayCollections
                    .Where(c => conversionModelsFrom.Contains(c.Target))
                    .ToList();

                collectionTo = monitoringOtherIndividualDayCalculator
                    .Calculate(
                        collectionTo,
                        collectionsFrom,
                        simulatedIndividualDays,
                        substances,
                        exposureType,
                        progressState.NewCompositeState(100D / targetsTo.Count)
                    );

                convertedHbmIndividualDayCollections.Add(collectionTo);
            }
            return convertedHbmIndividualDayCollections;
        }
    }
}
