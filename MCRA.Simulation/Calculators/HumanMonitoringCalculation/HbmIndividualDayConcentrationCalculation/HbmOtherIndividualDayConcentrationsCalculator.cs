using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmOtherIndividualDayConcentrationsCalculator : HbmIndividualDayConcentrationBaseCalculator {

        public HbmOtherIndividualDayConcentrationsCalculator(ITargetMatrixConversionCalculator targetMatrixConversionCalculator) {
            BiologicalMatrixConversionCalculator = targetMatrixConversionCalculator;
        }

        /// <summary>
        /// Computes the HBM individual day concentrations for the specified
        /// target biological matrix based on the HBM data in the sample 
        /// substance collection.
        /// </summary>
        public ICollection<HbmIndividualDayCollection> Calculate(
            ICollection<HbmIndividualDayCollection> individualDayConcentrationCollections,
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances
        ) {
            // Depending on the setting, impute from other matrices
            // Sample substance collections of other (non-target) matrices
            var results = new List<HbmIndividualDayCollection>();

            // Aggregate imputation records
            foreach (var item in individualDayConcentrationCollections) {
                var otherMatrixImputationRecords = CreateIndividualDayConcentrationRecords(
                    hbmSampleSubstanceCollections,
                    individualDays,
                    substances,
                    item.TargetUnit
                );
                var result = aggregateIndividualDayConcentrations(
                    item.HbmIndividualDayConcentrations,
                    otherMatrixImputationRecords,
                    individualDays,
                    substances,
                    item.TargetUnit.BiologicalMatrix
                );
                results.Add(new HbmIndividualDayCollection() {
                    TargetUnit = item.TargetUnit,
                    HbmIndividualDayConcentrations = result
                });
            }
            return results;
        }

        private Dictionary<(Individual Individual, string IdDay), List<HbmIndividualDayConcentration>> CreateIndividualDayConcentrationRecords(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            //Note that this collection is not equal to all collections except the main (it is all except all the collections equal to the target biological matrix)
            var otherSampleSubstanceCollections = hbmSampleSubstanceCollections
                .Where(r => r.SamplingMethod.BiologicalMatrix != targetUnit.BiologicalMatrix)
                .ToList();

            // Collect imputation records for all other sample substance collections for each individual day
            var otherMatrixImputationRecords = individualDays
                .ToDictionary(r => (r.Individual, r.IdDay), r => new List<HbmIndividualDayConcentration>());
            foreach (var sampleSubstanceCollection in otherSampleSubstanceCollections) {
                // Compute HBM individual day concentrations for sample substance collection
                var individualConcentrations = Compute(
                    sampleSubstanceCollection,
                    individualDays,
                    substances,
                    targetUnit
                )
                .ToDictionary(c => (c.Individual, c.Day));

                // Store the calculated HBM individual day concentrations
                foreach (var individualDay in individualDays) {
                    var key = (individualDay.Individual, individualDay.IdDay);
                    if (individualConcentrations.TryGetValue(key, out var hbmConcentration) && hbmConcentration != null) {
                        otherMatrixImputationRecords[key].Add(hbmConcentration);
                    }
                }
            }
            return otherMatrixImputationRecords;
        }

        private List<HbmIndividualDayConcentration> aggregateIndividualDayConcentrations(
            ICollection<HbmIndividualDayConcentration> individualDayConcentrationCollections,
            Dictionary<(Individual Individual, string IdDay), List<HbmIndividualDayConcentration>> otherMatrixImputationRecords,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            BiologicalMatrix targetBiologicalMatrix
        ) {
            var individualDayConcentrations = individualDayConcentrationCollections.ToDictionary(c => (c.Individual, c.Day));

            foreach (var individualDay in individualDays) {
                var key = (individualDay.Individual, individualDay.IdDay);
                individualDayConcentrations.TryGetValue(key, out var mainRecord);
                otherMatrixImputationRecords.TryGetValue(key, out var imputationRecords);

                if (mainRecord == null && imputationRecords != null) {
                    // If a main record does not yet exist for this individual day, then
                    // create it and add it to the individual day concentrations collection
                    mainRecord = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.Individual.Id,
                        Individual = individualDay.Individual,
                        Day = individualDay.IdDay
                    };
                    individualDayConcentrations[key] = mainRecord;
                }

                // Loop over substances and try impute value for each substance for which
                // a main record is missing
                foreach (var substance in substances) {
                    if (!mainRecord.ConcentrationsBySubstance.ContainsKey(substance)) {
                        // If there is no concentration value for the substance in the main
                        // record then impute from the other records
                        var imputationValues = imputationRecords
                            .Select(r => r.ConcentrationsBySubstance.TryGetValue(substance, out var conc) ? conc : null)
                            .Where(r => r != null)
                            .ToList();
                        if (imputationValues.Any()) {
                            var imputationRecord = new HbmSubstanceTargetExposure() {
                                Substance = substance,
                                BiologicalMatrix = targetBiologicalMatrix,
                                Concentration = getImputedConcentration(imputationValues),
                                SourceSamplingMethods = imputationValues
                                    .SelectMany(r => r.SourceSamplingMethods)
                                    .Distinct()
                                    .ToList(),
                                IsAggregateOfMultipleSamplingMethods = true,
                            };
                            mainRecord.ConcentrationsBySubstance[substance] = imputationRecord;
                        }
                    }
                }
            }
            return individualDayConcentrations.Values.ToList();
        }

        private double getImputedConcentration(
            List<IHbmSubstanceTargetExposure> imputationValues
        ) {
            var conversionConcentration = new List<double>();
            foreach (var record in imputationValues) {
                conversionConcentration.Add(record.Concentration);
            }
            return conversionConcentration.Average();
        }
    }
}
