using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmBiologicalMatrixConcentrationConversion;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationsCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmIndividualDayConcentrationsCalculator {

        private readonly IHbmIndividualDayConcentrationsCalculatorSettings _settings;
        private readonly IBiologicalMatrixConcentrationConversionCalculator _biologicalMatrixConcentrationConversionCalculator;

        public HbmIndividualDayConcentrationsCalculator(
            IHbmIndividualDayConcentrationsCalculatorSettings settings,
            IBiologicalMatrixConcentrationConversionCalculator biologicalMatrixConcentrationConversionCalculator
        ) {
            _settings = settings;
            _biologicalMatrixConcentrationConversionCalculator = biologicalMatrixConcentrationConversionCalculator;
        }

        /// <summary>
        /// Computes the HBM individual day concentrations for the specified
        /// target biological matrix based on the HBM data in the sample 
        /// substance collection.
        /// </summary>
        /// <param name="hbmSampleSubstanceCollections"></param>
        /// <param name="individualDays"></param>
        /// <param name="substances"></param>
        /// <param name="targetBiologicalMatrix"></param>
        /// <returns></returns>
        public List<HbmIndividualDayConcentration> Calculate(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            string targetBiologicalMatrix
        ) {

            // Compute HBM individual concentrations for the sample substance
            // collection matching the target biological matrix.
            // TODO: account for the cases when the same matrix is measured with
            // multiple sampling methods (e.g., 24h and spot urine).
            var mainSampleSubstanceCollection = hbmSampleSubstanceCollections
                .FirstOrDefault(x => x.SamplingMethod.Compartment == targetBiologicalMatrix);
            var individualDayConcentrations = Compute(
                mainSampleSubstanceCollection,
                individualDays,
                substances,
                targetBiologicalMatrix
            );

            // Depending on the setting, impute from other matrices
            if (_settings.ImputeHbmConcentrationsFromOtherMatrices) {

                // Sample substance collections of other (non-target) matrices
                var otherSampleSubstanceCollections = hbmSampleSubstanceCollections
                    .Where(r => r != mainSampleSubstanceCollection)
                    .ToList();

                // Collect imputation records for all other sample substance collections
                // for each individual day
                var otherMatrixImputationRecords = individualDays
                    .ToDictionary(r => (r.Individual, r.IdDay), r => new List<HbmIndividualDayConcentration>());
                foreach (var sampleSubstanceCollection in otherSampleSubstanceCollections) {
                    // Compute HBM individual day concentrations for sample substance collection
                    var individualConcentrations = Compute(
                        sampleSubstanceCollection,
                        individualDays,
                        substances,
                        targetBiologicalMatrix
                    );

                    // Store the calculated HBM individual day concentrations
                    foreach (var individualDay in individualDays) {
                        var key = (individualDay.Individual, individualDay.IdDay);
                        if (individualConcentrations.TryGetValue(key, out var hbmConcentration) && hbmConcentration != null) {
                            otherMatrixImputationRecords[key].Add(hbmConcentration);
                        }
                    }
                }

                // Aggregate imputation records
                foreach (var individualDay in individualDays) {
                    var key = (individualDay.Individual, individualDay.IdDay);
                    var mainRecord = individualDayConcentrations[key];
                    var imputationRecords = otherMatrixImputationRecords[key];

                    if (mainRecord == null && imputationRecords.Any()) {
                        // If a main record does not yet exist for this individual day, then
                        // create it and add it to the individual day concentrations collection
                        mainRecord = new HbmIndividualDayConcentration() {
                            SimulatedIndividualId = individualDay.Individual.Id,
                            Individual = individualDay.Individual,
                            Day = individualDay.IdDay
                        };
                        individualDayConcentrations[key] = mainRecord;
                    }
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
                                    Concentration = imputationValues
                                        .Select(r => r.Concentration)
                                        .Average(),
                                    SourceSamplingMethods = imputationValues
                                        .SelectMany(r => ((IHbmSubstanceTargetExposure)r).SourceSamplingMethods)
                                        .Distinct()
                                        .ToList(),
                                    IsAggregateOfMultipleSamplingMethods = true,
                                };
                                mainRecord.ConcentrationsBySubstance[substance] = imputationRecord;
                            }
                        }
                    }
                }
            }
            return individualDayConcentrations.Values.ToList();
        }

        private Dictionary<(Individual Individual, string IdDay), HbmIndividualDayConcentration> Compute(
            HumanMonitoringSampleSubstanceCollection sampleSubstanceCollection,
            ICollection<IndividualDay> individualDays,
            ICollection<Compound> substances,
            string targetCompartment
        ) {
            var individualDayConcentrations = new Dictionary<(Individual Individual, string IdDay), HbmIndividualDayConcentration>();

            var samplingMethod = sampleSubstanceCollection.SamplingMethod;
            var measuredBiologicalMatrix = sampleSubstanceCollection.SamplingMethod.Compartment;

            var samplesPerIndividualDay = sampleSubstanceCollection?
                .HumanMonitoringSampleSubstanceRecords
                .ToLookup(r => (Individual: r.Individual, IdDay: r.Day));

            var sourceCompartment = sampleSubstanceCollection.SamplingMethod.Compartment;

            var individualDayIdCounter = 0;
            foreach (var individualDay in individualDays) {
                if (samplesPerIndividualDay.Contains((individualDay.Individual, individualDay.IdDay))) {
                    var groupedSample = samplesPerIndividualDay[(individualDay.Individual, individualDay.IdDay)];
                    var concentrationsBySubstance = computeConcentrationsBySubstance(
                        groupedSample.ToList(),
                        substances,
                        samplingMethod,
                        targetCompartment
                    );
                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualId = individualDay.Individual.Id,
                        SimulatedIndividualDayId = individualDayIdCounter++,
                        Individual = individualDay.Individual,
                        IndividualSamplingWeight = individualDay.Individual.SamplingWeight, 
                        Day = individualDay.IdDay,
                        ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => (IHbmSubstanceTargetExposure)o.Value)
                    };
                    individualDayConcentrations[(individualDay.Individual, individualDay.IdDay)] = individualDayConcentration;
                }
            }

            return individualDayConcentrations;
        }

        private Dictionary<Compound, HbmSubstanceTargetExposure> computeConcentrationsBySubstance(
            ICollection<HumanMonitoringSampleSubstanceRecord> individualDaySamples,
            ICollection<Compound> substances,
            HumanMonitoringSamplingMethod samplingMethod,
            string targetBiologicalMatrix
        ) {
            var result = individualDaySamples
                .SelectMany(sample => {
                    var specificGravityCorrectionFactor = computeConcentrationCorrectionFactor(sample);
                    var sampleIntakesBySubstance = sample.HumanMonitoringSampleSubstances.Values
                        .SelectMany(r => getConcentrationsBySubstance(r, specificGravityCorrectionFactor))
                        .GroupBy(r => r.Substance)
                        .Select(g => (
                            substance: g.Key,
                            concentration: g.Any() ? g.Average(r => r.Concentration) : double.NaN
                        )
                    );
                    return sampleIntakesBySubstance;
                })
                .GroupBy(r => r.substance)
                .Where(r => substances.Contains(r.Key))
                .ToDictionary(
                    g => g.Key,
                    g => {
                        var averageConcentration = g.Any() ? g.Average(r => r.concentration) : double.NaN;
                        var concentration = _biologicalMatrixConcentrationConversionCalculator
                            .GetTargetConcentration(samplingMethod, targetBiologicalMatrix, averageConcentration);
                        return new HbmSubstanceTargetExposure() {
                            Substance = g.Key,
                            Concentration = concentration,
                            SourceSamplingMethods = new List<HumanMonitoringSamplingMethod>() { samplingMethod },
                            BiologicalMatrix = targetBiologicalMatrix
                        };
                    }
                );
            return result;
        }

        private List<HbmSubstanceTargetExposure> getConcentrationsBySubstance(
            SampleCompound sampleSubstance,
            double specificGravityCorrectionFactor
        ) {
            var result = new List<HbmSubstanceTargetExposure>();
            if (sampleSubstance.IsPositiveResidue || sampleSubstance.IsZeroConcentration) {
                var exposure = new HbmSubstanceTargetExposure() {
                    Substance = sampleSubstance.ActiveSubstance,
                    Concentration = sampleSubstance.Residue * specificGravityCorrectionFactor,
                };
                result.Add(exposure);
            }
            return result;
        }

        private static double computeConcentrationCorrectionFactor(HumanMonitoringSampleSubstanceRecord sample) {
            // TODO: get sg correction from sample directly (may be computed; not from data record)
            return sample.SpecificGravityCorrectionFactor ?? 1;
        }
    }
}
