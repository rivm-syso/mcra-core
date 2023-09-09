using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayMatrixExtrapolationCalculator : HbmIndividualDayConcentrationCalculatorBase {

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
            HbmIndividualDayCollection individualDayConcentrationCollection,
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances
        ) {
            var otherMatrixImputationRecords = CreateIndividualDayConcentrationRecords(
                hbmSampleSubstanceCollections,
                individualDays,
                substances,
                individualDayConcentrationCollection.TargetUnit
            );

            var hbmIndividualDayConcentrations = aggregateIndividualDayConcentrations(
                individualDayConcentrationCollection.HbmIndividualDayConcentrations,
                otherMatrixImputationRecords,
                individualDays,
                substances,
                individualDayConcentrationCollection.Target
            );

            var result = new HbmIndividualDayCollection() {
                TargetUnit = individualDayConcentrationCollection.TargetUnit,
                HbmIndividualDayConcentrations = hbmIndividualDayConcentrations
            };

            return result;
        }

        private Dictionary<(Individual Individual, string IdDay), List<HbmIndividualDayConcentration>> CreateIndividualDayConcentrationRecords(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            TargetUnit targetUnit
        ) {
            // Note that this collection is not equal to all collections except the main (it is
            // all except all the collections equal to the target biological matrix)
            var otherSampleSubstanceCollections = hbmSampleSubstanceCollections
                .Where(r => r.SamplingMethod.BiologicalMatrix != targetUnit.BiologicalMatrix)
                .ToList();

            // Collect imputation records for all other sample substance collections for each individual day
            var otherMatrixImputationRecords = individualDays
                .ToDictionary(r => (r.Individual, r.Day), r => new List<HbmIndividualDayConcentration>());

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
                    var key = (individualDay.Individual, individualDay.Day);
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
            ICollection<SimulatedIndividualDay> individualDays,
            ICollection<Compound> substances,
            ExposureTarget target
        ) {
            var individualDayConcentrations = individualDayConcentrationCollections.ToDictionary(c => (c.Individual, c.Day));

            foreach (var individualDay in individualDays) {
                var key = (individualDay.Individual, individualDay.Day);
                individualDayConcentrations.TryGetValue(key, out var mainRecord);
                otherMatrixImputationRecords.TryGetValue(key, out var imputationRecords);

                if (mainRecord == null && imputationRecords != null) {
                    // If a main record does not yet exist for this individual day, then
                    // create it and add it to the individual day concentrations collection
                    mainRecord = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.Individual.Id,
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
            List<HbmSubstanceTargetExposure> imputationValues
        ) {
            var conversionConcentration = new List<double>();
            foreach (var record in imputationValues) {
                conversionConcentration.Add(record.Concentration);
            }
            return conversionConcentration.Average();
        }

        protected override double getTargetConcentration(
            HumanMonitoringSamplingMethod samplingMethodSource, 
            ExpressionType expressionTypeSource, 
            ConcentrationUnit sourceConcentrationUnit, 
            TargetUnit targetUnit, 
            Compound substance, 
            double averageConcentration
        ) {
            return BiologicalMatrixConversionCalculator
                .GetTargetConcentration(
                    averageConcentration,
                    substance,
                    expressionTypeSource,
                    samplingMethodSource.BiologicalMatrix,
                    sourceConcentrationUnit,
                    targetUnit
                );
        }
    }
}
