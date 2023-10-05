using DocumentFormat.OpenXml.Wordprocessing;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.MixtureCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {

    /// <summary>
    /// Calculator/builder class for exposure matrices based on individual(day) exposure
    /// collections.
    /// </summary>
    public class ExposureMatrixBuilder {

        private readonly ICollection<Compound> _substances;
        private readonly IDictionary<Compound, double> _relativePotencyFactors;
        private readonly IDictionary<Compound, double> _membershipProbabilities;
        private readonly ExposureApproachType _exposureApproachType;
        private readonly ExposureType _exposureType;
        private readonly bool _isPerPerson;
        private readonly double _totalExposureCutOff;
        private readonly double _ratioCutOff;

        /// <summary>
        /// Creates a new <see cref="ExposureMatrixBuilder"/> instance.
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="exposureType"></param>
        /// <param name="isPerPerson"></param>
        /// <param name="exposureApproachType"></param>
        /// <param name="totalExposureCutOff"></param>
        /// <param name="ratioCutOff"></param>
        public ExposureMatrixBuilder(
            ICollection<Compound> substances = null,
            IDictionary<Compound, double> relativePotencyFactors = null,
            IDictionary<Compound, double> membershipProbabilities = null,
            ExposureType exposureType = ExposureType.Acute,
            bool isPerPerson = false,
            ExposureApproachType exposureApproachType = ExposureApproachType.RiskBased,
            double totalExposureCutOff = 0,
            double ratioCutOff = 0
        ) {
            _substances = substances;
            _relativePotencyFactors = relativePotencyFactors;
            _membershipProbabilities = membershipProbabilities;
            if (exposureApproachType == ExposureApproachType.UnweightedExposures) {
                _relativePotencyFactors = substances.ToDictionary(r => r, r => 1D);
                _membershipProbabilities = substances.ToDictionary(r => r, r => 1D);
            }
            if (_relativePotencyFactors == null && exposureApproachType == ExposureApproachType.ExposureBased) {
                _relativePotencyFactors = substances.ToDictionary(r => r, r => 1D);
            }
            if (_membershipProbabilities == null && exposureApproachType == ExposureApproachType.ExposureBased) {
                _membershipProbabilities = substances.ToDictionary(r => r, r => 1D);
            }
            _exposureApproachType = exposureApproachType;
            _exposureType = exposureType;
            _isPerPerson = isPerPerson;
            _totalExposureCutOff = totalExposureCutOff;
            _ratioCutOff = ratioCutOff;
        }

        /// <summary>
        /// Compute exposure matrix for target level external
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <returns></returns>
        public ExposureMatrix Compute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            if (_exposureType == ExposureType.Chronic) {
                return computeDietaryChronic(
                    dietaryIndividualDayIntakes
                );
            } else {
                return computeDietaryAcute(
                    dietaryIndividualDayIntakes
                );
            }
        }

        /// <summary>
        /// Compute exposure matrix for target level internal, combined dietary and non-dietary.
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="aggregateIndividualExposures"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public ExposureMatrix Compute(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureTarget target
        ) {
            if (_exposureType == ExposureType.Chronic) {
                return computeAggregateChronic(
                    aggregateIndividualExposures,
                    target
                );
            } else {
                return computeAggregateAcute(
                    aggregateIndividualDayExposures,
                    target
                );
            }
        }

        /// <summary>
        /// Compute exposure matrix for target level internal, human monitoring data.
        /// </summary>
        /// <param name="hbmIndividualDayConcentrationsCollections"></param>
        /// <param name="hbmIndividualConcentrationsCollections"></param>
        /// <returns></returns>
        public ExposureMatrix Compute(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayConcentrationsCollections,
            ICollection<HbmIndividualCollection> hbmIndividualConcentrationsCollections
        ) {
            if (_exposureType == ExposureType.Chronic) {
                return computeHumanMonitoringChronic(hbmIndividualConcentrationsCollections);
            } else {
                return computeHumanMonitoringAcute(hbmIndividualDayConcentrationsCollections);
            }
        }

        /// <summary>
        /// Create NMF exposure matrix with 1) with standardized exposures (depending on the 
        /// selected option), and 2) only the top exposures rows, i.e., those above the cutoff 
        /// percentile (including the exposure associated with the cutoff percentile).
        /// </summary>
        /// <param name="exposureMatrix"></param>
        /// <returns></returns>
        public (ExposureMatrix, double) Compute(ExposureMatrix exposureMatrix) {
            var exposureTranspose = exposureMatrix.Exposures.Transpose();
            var totalExposureCutOffPercentile = 0d;
            if (_totalExposureCutOff > 0) {
                totalExposureCutOffPercentile = exposureTranspose.Array
                    .Select(c => c.Sum())
                    .ToList()
                    .Percentile(_totalExposureCutOff);
            }

            // The indices of the selected individual(day)s
            var numberOfDays = exposureMatrix.Exposures.ColumnDimension / exposureMatrix.Individuals.Count;
            var selectedColumnIndices = exposureTranspose.Array
                .AsParallel()
                .Select((c, ix) => {
                    var items = c.ToList();
                    var maximum = items.Max();
                    var cumulativeExposure = items.Sum();
                    if (cumulativeExposure / maximum >= _ratioCutOff && cumulativeExposure > totalExposureCutOffPercentile) {
                        return ix / numberOfDays;
                    } else {
                        return -1;
                    }
                })
                .Where(c => c >= 0)
                .ToList();

            GeneralMatrix exposures = null;
            ExposureMatrix resultMatrix = null;

            if (selectedColumnIndices.Any()) {
                exposures = exposureMatrix.Exposures.GetMatrix(0, exposureMatrix.Exposures.RowDimension - 1, selectedColumnIndices.ToArray());
            } else {
                throw new Exception($"The specified ratio cutoff or exposure cutoff for MCR are too high. There are no exposures that fulfill the criteria.");
            }
            var individuals = selectedColumnIndices.Any() ? selectedColumnIndices.Select(ix => exposureMatrix.Individuals.ElementAt(ix)).ToList() : exposureMatrix.Individuals;

            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                var substanceTargetsWithExposure = exposureMatrix.RowRecords.Values
                    .Select(c => (c.Substance, c.Target))
                    .ToList();
                var sd = exposureMatrix.RowRecords.Values.Select(c => c.Stdev).ToArray();
                var recalculatedExposures = GeneralMatrix.CreateDiagonal(sd).Multiply(exposures);
                resultMatrix = calculateStandardizedExposureMatrix(
                    individuals,
                    substanceTargetsWithExposure,
                    recalculatedExposures
                );
            } else {
                resultMatrix = new ExposureMatrix() {
                    Exposures = exposures,
                    Individuals = individuals,
                    RowRecords = exposureMatrix.RowRecords
                };
            }

            return (resultMatrix, totalExposureCutOffPercentile);
        }

        /// <summary>
        /// Compute exposure matrix from chronic HBM individual collections.
        /// </summary>
        /// <param name="hbmIndividualCollections"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private ExposureMatrix computeHumanMonitoringChronic(
            ICollection<HbmIndividualCollection> hbmIndividualCollections
        ) {
            if (_exposureApproachType == ExposureApproachType.RiskBased && hbmIndividualCollections.Count > 1) {
                // TODO (issue 1721): implement risk based exposure matrix calculation for multiple targets.
                throw new NotImplementedException("Risk based exposure matrix calculation (using RPFs) not implemented for multiple exposure targets.");
            }

            var positiveIndividualConcentrations = hbmIndividualCollections
                .SelectMany(c => c.HbmIndividualConcentrations)
                .AsParallel()
                .Where(c => c.ConcentrationsBySubstance.Values.Any(r => r.Concentration > 0))
                .ToList();

            if (!positiveIndividualConcentrations.Any()) {
                throw new Exception("No positive HBM individual exposures for computing exposure matrix.");
            }

            var concentrationsBySubstance = hbmIndividualCollections
               .SelectMany(c => c.HbmIndividualConcentrations, (tu, hic) => (
                   targetUnit: tu.TargetUnit,
                   hbmIndividualConcentrations: hic
               ))
               .SelectMany(r =>
                   r.hbmIndividualConcentrations.ConcentrationsBySubstance.Values,
                   (ic, sc) => (
                       individualId: ic.hbmIndividualConcentrations.SimulatedIndividualId,
                       substance: sc.Substance,
                       concentration: sc.Concentration,
                       individual: ic.hbmIndividualConcentrations.Individual,
                       target: ic.targetUnit.Target
                   ))
               .GroupBy(gr => (substance: gr.substance, target: gr.target))
               .Select(c => {
                   var lookUp = c.ToLookup(c => c.individualId);
                   lookUp.ForAll(i => i.Select(r => r.concentration * _relativePotencyFactors[c.Key.substance] * _membershipProbabilities[c.Key.substance]));
                   return (
                       substance: c.Key.substance,
                       target: c.Key.target,
                       lookUp: lookUp
                   );
               })
               .Where(r => r.lookUp.Distinct(t => t.Average(v => v.concentration)).Count() > 1)
               .OrderBy(r => r.substance.Code)
               .ThenBy(r => r.target.Code)
               .ToList();

            var substanceTargetsWithExposure = concentrationsBySubstance
                .Select(c => (c.substance, c.target))
                .ToList();
            var identifierIds = concentrationsBySubstance
                .SelectMany(c => c.lookUp.Select(c => c.Key))
                .Distinct()
                .ToList();

            double exposureDelegate(int i, int j) => concentrationsBySubstance[i].lookUp.Contains(j)
                ? concentrationsBySubstance[i].lookUp[j].Average(c => c.concentration)
                : 0d;

            var exposureMatrix = new GeneralMatrix(
                concentrationsBySubstance.Count,
                identifierIds,
                exposureDelegate
            );
            var individuals = concentrationsBySubstance
                .SelectMany(c => c.lookUp.Select(c => c.FirstOrDefault().individual))
                .Distinct()
                .ToList();
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(
                    individuals,
                    substanceTargetsWithExposure,
                    exposureMatrix
                );
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Individuals = individuals,
                    RowRecords = createRowRecords(substanceTargetsWithExposure)
                };
            }
        }

        /// <summary>
        /// Acute HBM assessment
        /// </summary>
        /// <param name="hbmIndividualDayCollections"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private ExposureMatrix computeHumanMonitoringAcute(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections
        ) {
            if (_exposureApproachType == ExposureApproachType.RiskBased && hbmIndividualDayCollections.Count > 1) {
                // TODO (issue 1721): implement risk based exposure matrix calculation for multiple targets.
                throw new NotImplementedException("Risk based exposure matrix calculation (using RPFs) not implemented for multiple exposure targets.");
            }

            var positiveIndividualDayConcentrations = hbmIndividualDayCollections
                .SelectMany(c => c.HbmIndividualDayConcentrations)
                .AsParallel()
                .Where(c => c.ConcentrationsBySubstance.Values.Any(r => r.Concentration > 0))
                .ToList();
            if (!positiveIndividualDayConcentrations.Any()) {
                throw new Exception("No positive HBM individual day exposures for computing exposure matrix.");
            }

            var concentrationsBySubstance = hbmIndividualDayCollections
                .SelectMany(c => c.HbmIndividualDayConcentrations, (tu, hic) => (
                    targetUnit: tu.TargetUnit,
                    hbmIndividualDayConcentrations: hic
                ))
                .SelectMany(r =>
                    r.hbmIndividualDayConcentrations.ConcentrationsBySubstance.Values,
                    (ic, sc) => (
                        individualDayId: ic.hbmIndividualDayConcentrations.SimulatedIndividualDayId,
                        substance: sc.Substance,
                        concentration: sc.Concentration,
                        individual: ic.hbmIndividualDayConcentrations.Individual,
                        target: ic.targetUnit.Target
                    ))

                 .GroupBy(gr => (substance: gr.substance, target: gr.target))
                 .Select(c => {
                     var lookUp = c.ToLookup(c => c.individualDayId);
                     lookUp.ForAll(i => i.Select(r => r.concentration * _relativePotencyFactors[c.Key.substance] * _membershipProbabilities[c.Key.substance]));
                     return (
                         substance: c.Key.substance,
                         target: c.Key.target,
                         lookUp: lookUp
                     );
                 })
                .Where(r => r.lookUp.Distinct(t => t.Average(v => v.concentration)).Count() > 1)
                .OrderBy(r => r.substance.Code)
                .ThenBy(r => r.target.Code)
                .ToList();

            // The (row) substance/target combinations.
            var substanceTargetsWithExposure = concentrationsBySubstance
                .Select(c => (c.substance, c.target))
                .ToList();

            // The (column) individual(day) identifiers.
            var identifierIds = concentrationsBySubstance
                .SelectMany(c => c.lookUp.Select(c => c.Key))
                .Distinct()
                .ToList();

            double exposureDelegate(int i, int j) => concentrationsBySubstance[i].lookUp.Contains(j)
                ? concentrationsBySubstance[i].lookUp[j].Average(c => c.concentration)
                : 0d;

            var exposureMatrix = new GeneralMatrix(
                concentrationsBySubstance.Count,
                identifierIds,
                exposureDelegate
            );

            var individuals = concentrationsBySubstance
                .SelectMany(c => c.lookUp.Select(c => c.FirstOrDefault().individual))
                .Distinct()
                .ToList();

            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(
                    individuals,
                    substanceTargetsWithExposure,
                    exposureMatrix
                );
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Individuals = individuals,
                    RowRecords = createRowRecords(substanceTargetsWithExposure)
                };
            }
        }

        /// <summary>
        /// Creates a exposure matrix object from the aggregate individual day exposures.
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private ExposureMatrix computeAggregateAcute(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureTarget target
        ) {
            var individualDaysWithExposure = aggregateIndividualDayExposures
                .AsParallel()
                .Where(c => c.TotalConcentrationAtTarget(_relativePotencyFactors, _membershipProbabilities, _isPerPerson) > 0)
                .ToList();

            if (!individualDaysWithExposure.Any()) {
                throw new Exception("No positive individual day exposures for computing exposure matrix.");
            }

            // Compute exposures per individual day and substance.
            var exposures = individualDaysWithExposure
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .SelectMany(idi => {
                    var exposuresPerSubstance = idi.TargetExposuresBySubstance;
                    return _substances
                        .Select(substance => {
                            exposuresPerSubstance.TryGetValue(substance, out var exposurePerSubstance);
                            var exposure = exposurePerSubstance?.SubstanceAmount / (_isPerPerson ? 1 : idi.CompartmentWeight) ?? 0D;
                            return (
                                individualDayId: idi.SimulatedIndividualDayId,
                                substance: substance,
                                intake: exposure,
                                individual: idi.Individual
                            );
                        });
                })
                .ToList();

            var intakesPerSubstance = exposures
                .GroupBy(gr => gr.substance)
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .Select(c => {
                    var ordered = c.OrderBy(r => r.individualDayId).ToList();
                    var exposurePerSubstance = ordered
                        .Select(i => i.intake * _relativePotencyFactors[c.Key] * _membershipProbabilities[c.Key])
                        .ToList();
                    return (
                        substance: c.Key,
                        intake: exposurePerSubstance,
                        sum: exposurePerSubstance.Sum(),
                        identifierIds: ordered.Select(ic => ic.individualDayId).ToList(),
                        individuals: ordered.Select(ic => ic.individual),
                        distinct: exposurePerSubstance.Distinct().Count()
                    );
                })
                .Where(r => r.sum > 0)
                .Where(r => r.distinct > 1)
                .OrderBy(r => r.substance.Code)
                .ToList();

            // The substance/target combinations (rows of the matrix).
            var substanceTargetRecords = intakesPerSubstance.Select(r => (r.substance, target)).ToList();

            // The individuals of the matrix (columns).
            var individuals = intakesPerSubstance.First().individuals.ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].intake[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, individualDaysWithExposure.Count, exposureDelegate);

            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(
                    individuals,
                    substanceTargetRecords,
                    exposureMatrix
                );
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Individuals = individuals,
                    RowRecords = createRowRecords(substanceTargetRecords)
                };
            }
        }

        /// <summary>
        /// Creates a exposure matrix object from the aggregate individual exposures.
        /// </summary>
        /// <param name="aggregateIndividualExposures"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private ExposureMatrix computeAggregateChronic(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureTarget target
        ) {
            var individualsWithExposure = aggregateIndividualExposures
                .AsParallel()
                .Where(c => c.TotalConcentrationAtTarget(_relativePotencyFactors, _membershipProbabilities, _isPerPerson) > 0)
                .ToList();

            if (!individualsWithExposure.Any()) {
                throw new Exception("No positive individual exposures for computing exposure matrix.");
            }

            var results = individualsWithExposure
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .SelectMany(idi => {
                    var exposuresPerSubstance = idi.TargetExposuresBySubstance;
                    return _substances
                        .Select(substance => {
                            exposuresPerSubstance.TryGetValue(substance, out var exposurePerSubstance);
                            var exposure = exposurePerSubstance?.SubstanceAmount / (_isPerPerson ? 1 : idi.CompartmentWeight) ?? 0D;
                            return (
                                individualId: idi.SimulatedIndividualId,
                                substance: substance,
                                intake: exposure,
                                individual: idi.Individual
                            );
                        });
                })
                .ToList();

            var intakesPerSubstance = results
                .GroupBy(gr => gr.substance)
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .Select(c => {
                    var ordered = c.OrderBy(r => r.individualId).ToList();
                    var exposurePerSubstance = ordered.Select(i => i.intake * _relativePotencyFactors[c.Key] * _membershipProbabilities[c.Key]).ToList();
                    return (
                        substance: c.Key,
                        intake: exposurePerSubstance,
                        sum: exposurePerSubstance.Sum(),
                        individuals: ordered.Select(ic => ic.individual),
                        identifierIds: ordered.Select(ic => ic.individualId).ToList(),
                        distinct: exposurePerSubstance.Distinct().Count()
                    );
                })
                .Where(r => r.sum > 0)
                .Where(r => r.distinct > 1)
                .OrderBy(r => r.substance.Code)
                .ToList();

            // The substance/target combinations (rows of the matrix).
            var substanceTargetsWithExposure = intakesPerSubstance.Select(c => (c.substance, target)).ToList();

            // The individuals of the matrix (columns).
            var individuals = intakesPerSubstance.First().individuals.ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].intake[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, individualsWithExposure.Count, exposureDelegate);

            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substanceTargetsWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Individuals = individuals,
                    RowRecords = createRowRecords(substanceTargetsWithExposure)
                };
            }
        }

        /// <summary>
        /// Creates a exposure matrix object from the dietary individual day intakes.
        /// </summary>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <returns></returns>
        private ExposureMatrix computeDietaryAcute(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes
        ) {
            var individualDaysWithIntake = dietaryIndividualDayIntakes
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .Where(c => c.TotalExposurePerMassUnit(_relativePotencyFactors, _membershipProbabilities, _isPerPerson) * c.IndividualSamplingWeight > 0)
                .ToList();

            if (!individualDaysWithIntake.Any()) {
                throw new Exception("No positive individual day exposures for computing exposure matrix.");
            }

            var results = individualDaysWithIntake
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .SelectMany(idi => {
                    return _substances
                    .Select(substance => {
                        return (
                            individualId: idi.SimulatedIndividualDayId,
                            intake: idi.GetSubstanceTotalExposure(substance) / (_isPerPerson ? 1 : idi.Individual.BodyWeight),
                            substance: substance,
                            individual: idi.Individual
                        );
                    });
                }).ToList();

            var intakesPerSubstance = results
                .GroupBy(gr => gr.substance)
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .Select(c => {
                    var ordered = c.OrderBy(r => r.individualId).ToList();
                    var exposurePerSubstance = ordered
                        .Select(i => i.intake * _relativePotencyFactors[c.Key] * _membershipProbabilities[c.Key])
                        .ToList();
                    return (
                        substance: c.Key,
                        intake: exposurePerSubstance,
                        sum: exposurePerSubstance.Sum(),
                        identifierIds: ordered.Select(ic => ic.individualId).ToList(),
                        individuals: ordered.Select(ic => ic.individual),
                        distinct: exposurePerSubstance.Distinct().Count()
                    );
                })
                .Where(r => r.sum > 0)
                .Where(r => r.distinct > 1)
                .OrderBy(r => r.substance.Code)
                .ToList();

            // The substance/target combinations (rows of the matrix).
            var target = ExposureTarget.DietaryExposureTarget;
            var substanceTargetsWithExposure = intakesPerSubstance.Select(r => (r.substance, target)).ToList();

            // The individuals of the matrix (columns).
            var individuals = intakesPerSubstance.First().individuals.ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].intake[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, individualDaysWithIntake.Count, exposureDelegate);

            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substanceTargetsWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Individuals = individuals,
                    RowRecords = createRowRecords(substanceTargetsWithExposure)
                };
            }
        }

        /// <summary>
        /// Creates a exposure matrix object from the dietary individual exposures.
        /// </summary>
        /// <param name="dietaryIndividualIntakes"></param>
        /// <returns></returns>
        private ExposureMatrix computeDietaryChronic(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualIntakes
        ) {
            var results = dietaryIndividualIntakes
                .GroupBy(c => c.SimulatedIndividualId)
                .Where(c => c.Sum(r => r.TotalExposurePerMassUnit(_relativePotencyFactors, _membershipProbabilities, _isPerPerson)) > 0)
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .SelectMany(idi => {
                    return _substances
                        .Select(substance => {
                            return (
                                individualId: idi.First().SimulatedIndividualId,
                                intake: idi.Sum(r => r.GetSubstanceTotalExposure(substance) / (_isPerPerson ? 1 : idi.First().Individual.BodyWeight)) / idi.Count(),
                                substance: substance,
                                individual: idi.First().Individual
                            );
                        });
                })
                .ToList();

            if (!results.Any()) {
                throw new Exception("No positive individual exposures for computing exposure matrix.");
            }

            var intakesPerSubstance = results
                .GroupBy(gr => gr.substance)
                .AsParallel()
                .WithDegreeOfParallelism(50)
                .Select(c => {
                    var ordered = c.OrderBy(r => r.individualId).ToList();
                    var exposurePerSubstance = ordered
                        .Select(i => i.intake * _relativePotencyFactors[c.Key] * _membershipProbabilities[c.Key])
                        .ToList();
                    return (
                        substance: c.Key,
                        concentration: exposurePerSubstance,
                        sum: exposurePerSubstance.Sum(),
                        identifierIds: ordered.Select(ic => ic.individualId).ToList(),
                        individuals: ordered.Select(ic => ic.individual),
                        distinct: exposurePerSubstance.Distinct().Count()
                      );
                })
                .Where(r => r.sum > 0)
                .Where(r => r.distinct > 1)
                .OrderBy(r => r.substance.Code)
                .ToList();

            // The substance/target combinations (rows of the matrix).
            var target = ExposureTarget.DietaryExposureTarget;
            var substanceTargetsWithExposure = intakesPerSubstance.Select(c => (c.substance, target)).ToList();

            // The individuals of the matrix (columns).
            var identifierIds = intakesPerSubstance.First().identifierIds;
            var individuals = intakesPerSubstance.First().individuals.ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].concentration[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, identifierIds.Count, exposureDelegate);

            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substanceTargetsWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Individuals = individuals,
                    RowRecords = createRowRecords(substanceTargetsWithExposure)
                };
            }
        }

        /// <summary>
        /// Computes a standardized exposure matrix.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="rowRecords"></param>
        /// <param name="exposureMatrix"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static ExposureMatrix calculateStandardizedExposureMatrix(
           ICollection<Individual> individuals,
           List<(Compound substance, ExposureTarget exposureTarget)> rowRecords,
           GeneralMatrix exposureMatrix
       ) {
            var sd = exposureMatrix.Array.Select(c => Math.Sqrt(c.Variance())).ToArray();
            var zeroVarianceSubstances = sd.Select((c, i) => (variance: c, code: rowRecords[i].substance.Code))
                .Where(c => c.variance == 0).Select(c => c.code)
                .ToList();
            if (zeroVarianceSubstances.Any()) {
                throw new Exception($"For substances: {string.Join(", ", zeroVarianceSubstances)} the variance is equal to zero, which is not allowed for standardized exposures");
            }
            var sdDiag = GeneralMatrix.CreateDiagonal(sd);
            var sdInverse = sdDiag.Inverse();

            return new ExposureMatrix() {
                Exposures = sdInverse.Multiply(exposureMatrix),
                Individuals = individuals,
                RowRecords = createRowRecords(rowRecords, sd)
            };
        }

        /// <summary>
        /// Creates exposure matrix row records for the substance/target combinations.
        /// Also fills the standard deviations.
        /// </summary>
        /// <param name="substanceTargetsWithExposure"></param>
        /// <param name="sd"></param>
        /// <returns></returns>
        private static Dictionary<int, ExposureMatrixRowRecord> createRowRecords(
            List<(Compound substance, ExposureTarget exposureTarget)> substanceTargetsWithExposure,
            double[] sd = null
        ) {
            sd = sd ?? Enumerable.Repeat(1d, substanceTargetsWithExposure.Count).ToArray();
            var rowRecords = substanceTargetsWithExposure
                .Select((x, ix) => (ix, rowRecord: new ExposureMatrixRowRecord() {
                    Substance = substanceTargetsWithExposure[ix].substance,
                    Target = substanceTargetsWithExposure[ix].exposureTarget,
                    Stdev = sd[ix]
                }))
                .ToDictionary(c => c.ix, c => c.rowRecord);
            return rowRecords;
        }
    }
}
