﻿using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.TargetMatrixConcentrationConversion {
    public sealed class HbmIndividualDayMatrixExtrapolationCalculator {

        public ITargetMatrixConversionCalculator BiologicalMatrixConversionCalculator { get; set; }

        public HbmIndividualDayMatrixExtrapolationCalculator(ITargetMatrixConversionCalculator targetMatrixConversionCalculator) {
            BiologicalMatrixConversionCalculator = targetMatrixConversionCalculator;
        }

        /// <summary>
        /// Computes the HBM individual day concentrations for the specified
        /// target biological matrix based on the HBM data in the sample 
        /// substance collection.
        /// </summary>
        public HbmIndividualDayCollection Calculate(
            HbmIndividualDayCollection targetIndividualDayCollection,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances
        ) {
            var otherMatrixImputationRecords = collectConvertedOtherMatrixIndividualDayConcentrationCollections(
                hbmIndividualDayCollections,
                individualDays,
                targetIndividualDayCollection.TargetUnit
            );

            var hbmIndividualDayConcentrations = aggregateIndividualDayConcentrations(
                targetIndividualDayCollection.HbmIndividualDayConcentrations,
                otherMatrixImputationRecords,
                individualDays,
                substances
            );

            var result = new HbmIndividualDayCollection() {
                TargetUnit = targetIndividualDayCollection.TargetUnit,
                HbmIndividualDayConcentrations = hbmIndividualDayConcentrations
            };

            return result;
        }

        private Dictionary<SimulatedIndividualDay, List<HbmIndividualDayConcentration>> collectConvertedOtherMatrixIndividualDayConcentrationCollections(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<SimulatedIndividualDay> individualDays,
            TargetUnit targetUnit
        ) {
            // Note that this collection is not equal to all collections except the main (it is
            // all except all the collections equal to the target biological matrix)
            var otherHbmIndividualDayCollections = hbmIndividualDayCollections
                .Where(r => r.Target != targetUnit.Target)
                .ToList();

            // Collect imputation records for all other sample substance collections for each individual day
            var otherMatrixImputationRecords = individualDays
                .ToDictionary(
                    r => r,
                    r => new List<HbmIndividualDayConcentration>()
                );

            foreach (var collection in otherHbmIndividualDayCollections) {
                // Compute HBM individual day concentrations for all collections
                var individualConcentrations = collectOtherMatrixHbmIndividualDayConcentrations(
                    collection,
                    individualDays
                );

                // Store the calculated HBM individual day concentrations
                foreach (var individualDay in individualDays) {
                    if (individualConcentrations.TryGetValue(individualDay, out var hbmConcentration) && hbmConcentration != null) {
                        otherMatrixImputationRecords[individualDay].Add(hbmConcentration);
                    }
                }
            }

            return otherMatrixImputationRecords;
        }

        private IDictionary<SimulatedIndividualDay, HbmIndividualDayConcentration> collectOtherMatrixHbmIndividualDayConcentrations(
            HbmIndividualDayCollection collection,
            ICollection<SimulatedIndividualDay> individualDays
        ) {
            var result = new Dictionary<SimulatedIndividualDay, HbmIndividualDayConcentration>();

            // Group by simulated individual day
            var hbmIndividualDayConcentrationsLookup = collection.HbmIndividualDayConcentrations
                .ToDictionary(r => r.SimulatedIndividualDayId);

            foreach (var individualDay in individualDays) {
                if (hbmIndividualDayConcentrationsLookup.TryGetValue(individualDay.SimulatedIndividualDayId, out var hbmIndividualDayConcentration)) {

                    var concentrationsBySubstance = hbmIndividualDayConcentration.ConcentrationsBySubstance
                        .SelectMany(r => BiologicalMatrixConversionCalculator
                            .GetTargetSubstanceExposure(r.Value, collection.TargetUnit)
                        )
                        .GroupBy(r => r.Substance)
                        .ToDictionary(
                            g => g.Key,
                            g => combineHbmSubstanceTargetExposures(g.ToList())
                        );

                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.SimulatedIndividualId,
                        SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                        Individual = individualDay.Individual,
                        IndividualSamplingWeight = individualDay.Individual.SamplingWeight,
                        Day = individualDay.Day,
                        ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => o.Value)
                    };
                    result.Add(individualDay, individualDayConcentration);
                }
            }
            return result;
        }

        private HbmSubstanceTargetExposure combineHbmSubstanceTargetExposures(
            ICollection<HbmSubstanceTargetExposure> exposures
        ) {
            return new HbmSubstanceTargetExposure() {
                Concentration = exposures.Sum(r => r.Concentration),
                IsAggregateOfMultipleSamplingMethods = exposures.Any(r => r.IsAggregateOfMultipleSamplingMethods),
                SourceSamplingMethods = exposures.SelectMany(r => r.SourceSamplingMethods).Distinct().ToList(),
                Substance = exposures.First().Substance,
                Target = exposures.First().Target,
            };
        }

        private List<HbmIndividualDayConcentration> aggregateIndividualDayConcentrations(
            ICollection<HbmIndividualDayConcentration> individualDayConcentrationCollections,
            Dictionary<SimulatedIndividualDay, List<HbmIndividualDayConcentration>> otherMatrixImputationRecords,
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances
        ) {
            var individualDayConcentrations = individualDayConcentrationCollections
                .ToDictionary(c => (c.SimulatedIndividualId, c.Day));

            foreach (var individualDay in individualDays) {
                var key = (individualDay.SimulatedIndividualId, individualDay.Day);
                individualDayConcentrations.TryGetValue(key, out var mainRecord);
                otherMatrixImputationRecords.TryGetValue(individualDay, out var imputationRecords);

                if (mainRecord == null && imputationRecords != null) {
                    // If a main record does not yet exist for this individual day, then
                    // create it and add it to the individual day concentrations collection
                    mainRecord = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.SimulatedIndividualId,
                        Individual = individualDay.Individual,
                        Day = individualDay.Day
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
                                Concentration = getAverageConcentration(imputationValues),
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

        private double getAverageConcentration(
            List<HbmSubstanceTargetExposure> imputationValues
        ) {
            var conversionConcentration = new List<double>();
            foreach (var record in imputationValues) {
                conversionConcentration.Add(record.Concentration);
            }
            return conversionConcentration.Average();
        }
    }
}