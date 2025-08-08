using System.Diagnostics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.KineticConversions {
    public sealed class HbmIndividualDayMatrixExtrapolationCalculator {

        private readonly TargetMatrixKineticConversionCalculator _targetMatrixConversionCalculator;

        public HbmIndividualDayMatrixExtrapolationCalculator(
            TargetMatrixKineticConversionCalculator targetMatrixConversionCalculator
        ) {
            _targetMatrixConversionCalculator = targetMatrixConversionCalculator;
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
            ICollection<Compound> substances,
            ExposureType exposureType,
            CompositeProgressState progress,
            IRandom kineticModelParametersRandomGenerator = null
        ) {
            var otherMatrixImputationRecords = collectConvertedOtherMatrixIndividualDayConcentrationCollections(
                hbmIndividualDayCollections,
                individualDays,
                targetIndividualDayCollection.TargetUnit,
                exposureType,
                kineticModelParametersRandomGenerator,
                progress
            );

            var targetIndividualDayCollectionClone = targetIndividualDayCollection.Clone();
            var hbmIndividualDayConcentrations = aggregateIndividualDayConcentrations(
                targetIndividualDayCollectionClone.HbmIndividualDayConcentrations,
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
            TargetUnit targetUnit,
            ExposureType exposureType,
            IRandom kineticModelParametersRandomGenerator,
            CompositeProgressState progress
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
            for (int i = 0; i < otherHbmIndividualDayCollections.Count; i++) {
                var collection = otherHbmIndividualDayCollections[i];

                // Compute HBM individual day concentrations for all collections
                var individualConcentrations = collectOtherMatrixIndividualDayConcentrations(
                    collection,
                    individualDays,
                    targetUnit,
                    exposureType,
                    kineticModelParametersRandomGenerator,
                    progress.NewProgressState(100D / otherHbmIndividualDayCollections.Count)
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

        private IDictionary<SimulatedIndividualDay, HbmIndividualDayConcentration> collectOtherMatrixIndividualDayConcentrations(
            HbmIndividualDayCollection collection,
            ICollection<SimulatedIndividualDay> individualDays,
            TargetUnit targetUnit,
            ExposureType exposureType,
            IRandom kineticModelParametersRandomGenerator,
            ProgressState localProgress
        ) {
            var result = new Dictionary<SimulatedIndividualDay, HbmIndividualDayConcentration>();

            // Group by simulated individual day
            var hbmIndividualDayConcentrationsLookup = collection.HbmIndividualDayConcentrations
                .ToDictionary(r => r.SimulatedIndividualDayId);

            // Initialise progress logger
            var progressTracker = ProgressTracker.CreateAndStart(individualDays.Count);

            for (int i = 0; i < individualDays.Count; i++) {
                var individualDay = individualDays.ElementAt(i);
                if (hbmIndividualDayConcentrationsLookup.TryGetValue(
                    individualDay.SimulatedIndividualDayId,
                    out var hbmIndividualDayConcentration)
                ) {
                    // If target is external, then the compartment weight is the bodyweight
                    var compartmentWeight = targetUnit.TargetLevelType == TargetLevelType.External
                        ? individualDay.SimulatedIndividual.BodyWeight
                        : double.NaN;

                    var concentrationsBySubstance = new Dictionary<Compound, HbmSubstanceTargetExposure>();
                    concentrationsBySubstance = hbmIndividualDayConcentration.ConcentrationsBySubstance
                        .SelectMany(r => {
                            return _targetMatrixConversionCalculator
                                .GetSubstanceTargetExposures(
                                    r.Value,
                                    individualDay,
                                    collection.TargetUnit,
                                    exposureType,
                                    targetUnit,
                                    compartmentWeight,
                                    kineticModelParametersRandomGenerator,
                                    false
                                );
                        })
                        .GroupBy(r => r.Substance)
                        .ToDictionary(
                            g => g.Key,
                            g => combineHbmSubstanceTargetExposures([.. g])
                        );

                    var individualDayConcentration = new HbmIndividualDayConcentration() {
                        SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId,
                        SimulatedIndividual = individualDay.SimulatedIndividual,
                        Day = individualDay.Day,
                        ConcentrationsBySubstance = concentrationsBySubstance
                            .ToDictionary(o => o.Key, o => o.Value)
                    };
                    result.Add(individualDay, individualDayConcentration);

                    // Update progress
                    progressTracker.Update(localProgress, i);
                }
            }

            localProgress.MarkCompleted();
            return result;
        }

        private HbmSubstanceTargetExposure combineHbmSubstanceTargetExposures(
            IEnumerable<HbmSubstanceTargetExposure> exposures
        ) {
            return new HbmSubstanceTargetExposure() {
                Exposure = exposures.Sum(r => r.Exposure),
                IsAggregateOfMultipleSamplingMethods = exposures.Any(r => r.IsAggregateOfMultipleSamplingMethods),
                SourceSamplingMethods = [.. exposures.SelectMany(r => r.SourceSamplingMethods).Distinct()],
                Substance = exposures.First().Substance
            };
        }

        private List<HbmIndividualDayConcentration> aggregateIndividualDayConcentrations(
            ICollection<HbmIndividualDayConcentration> individualDayConcentrationCollections,
            Dictionary<SimulatedIndividualDay, List<HbmIndividualDayConcentration>> otherMatrixImputationRecords,
            ICollection<SimulatedIndividualDay> simulatedIndividualDays,
            ICollection<Compound> substances
        ) {
            var individualDayConcentrations = individualDayConcentrationCollections
                .ToDictionary(c => (c.SimulatedIndividual.Id, c.Day));

            foreach (var individualDay in simulatedIndividualDays) {
                var key = (individualDay.SimulatedIndividual.Id, individualDay.Day);
                individualDayConcentrations.TryGetValue(key, out var mainRecord);
                otherMatrixImputationRecords.TryGetValue(individualDay, out var imputationRecords);

                if (mainRecord == null && imputationRecords != null) {
                    // If a main record does not yet exist for this individual day, then
                    // create it and add it to the individual day concentrations collection
                    mainRecord = new HbmIndividualDayConcentration() {
                        SimulatedIndividual = individualDay.SimulatedIndividual,
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
                                Exposure = getAverageConcentration(imputationValues),
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
            return [.. individualDayConcentrations.Values];
        }

        private static double getAverageConcentration(
            List<HbmSubstanceTargetExposure> imputationValues
        ) {
            var conversionConcentration = new List<double>();
            foreach (var record in imputationValues) {
                conversionConcentration.Add(record.Exposure);
            }
            return conversionConcentration.Average();
        }

        internal class ProgressTracker {
            private readonly Stopwatch _sw;
            private readonly int _totalCount;

            public ProgressTracker(int totalCount) {
                _totalCount = totalCount;
                _sw = new Stopwatch();
            }

            public void Start() {
                _sw.Start();
            }

            public void Update(ProgressState progressState, int ix) {
                _sw.Stop();
                var progress = (double)(ix+1) / _totalCount;
#if DEBUG
                var debugMode = true;
                if (debugMode) {
                    var expectedTotalSeconds = (int)(_sw.Elapsed.TotalSeconds / progress);
                    var expectedRemainingSeconds = (int)(expectedTotalSeconds - _sw.Elapsed.TotalSeconds);
                    var expectedRemaining = TimeSpan.FromSeconds(expectedRemainingSeconds);
                    var expectedTotal = TimeSpan.FromSeconds(expectedTotalSeconds);
                    var message = $"Kinetic conversion modeling. Done processing individual {ix + 1} (out of {_totalCount}). "
                        + $"Expected total processing time: {expectedTotal:c}. "
                        + $"Expected time to finish: {expectedRemaining:c}.";
                    progressState.Update(message);
                    Debug.WriteLine(message);
                }
#endif
                progressState.Update(progress * 100D);
                _sw.Start();
            }

            public static ProgressTracker CreateAndStart(int totalCount) {
                var result = new ProgressTracker(totalCount);
                result.Start();
                return result;
            }
        }
    }
}
