using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {
    public static class HbmMultipleTargetExtrapolationCalculator {

        public static List<HbmIndividualDayCollection> Calculate(
           List<HbmIndividualDayCollection> hbmIndividualDayCollections,
           ICollection<KineticConversionFactorModel> kineticConversionFactorModels,
           List<SimulatedIndividualDay> simulatedIndividualDays,
           ICollection<Compound> substances
        ) {
            var targetsTo = kineticConversionFactorModels
                .Select(m => m.ConversionRule.TargetTo)
                .Distinct()
                .ToList();
            var convertedHbmIndividualDayCollections = new List<HbmIndividualDayCollection>();
            foreach (var collection in hbmIndividualDayCollections) {
                if (targetsTo.Contains(collection.Target)) {
                    var collectionTo = collection;
                    var matrixConversionCalculator = new TargetMatrixKineticConversionCalculator(
                        kineticConversionFactorModels,
                        collectionTo.TargetUnit);
                    var monitoringOtherIndividualDayCalculator = new HbmIndividualDayMatrixExtrapolationCalculator(
                        matrixConversionCalculator
                    );

                    var conversionModelsFrom = kineticConversionFactorModels
                        .Where(m => m.ConversionRule.TargetTo == collectionTo.Target)
                        .Select(m => m.ConversionRule.TargetFrom)
                        .ToList();
                    var collectionsFrom = hbmIndividualDayCollections
                        .Where(c => conversionModelsFrom.Contains(c.Target))
                        .ToList();

                    collectionTo = monitoringOtherIndividualDayCalculator
                        .Calculate(
                            collectionTo,
                            collectionsFrom,
                            simulatedIndividualDays,
                            substances
                        );
                    convertedHbmIndividualDayCollections.Add(collectionTo);
                } else {
                    convertedHbmIndividualDayCollections.Add(collection);
                }
            }
            return convertedHbmIndividualDayCollections;
        }
    }
}
