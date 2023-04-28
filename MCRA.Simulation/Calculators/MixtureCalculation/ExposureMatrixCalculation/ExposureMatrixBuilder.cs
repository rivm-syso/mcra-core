using System.Linq;
using DocumentFormat.OpenXml.ExtendedProperties;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation {
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
                return computeDietaryChronic(dietaryIndividualDayIntakes);
            } else {
                return computeDietaryAcute(dietaryIndividualDayIntakes);
            }
        }

        /// <summary>
        /// Compute exposure matrix for target level internal, combined dietary and non-dietary.
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <param name="aggregateIndividualExposures"></param>
        /// <returns></returns>
        public ExposureMatrix Compute(ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures, ICollection<AggregateIndividualExposure> aggregateIndividualExposures) {
            if (_exposureType == ExposureType.Chronic) {
                return computeAggregateChronic(aggregateIndividualExposures);
            } else {
                return computeAggregateAcute(aggregateIndividualDayExposures);
            }
        }

        /// <summary>
        /// Compute exposure matrix for target level internal, human monitoring data.
        /// </summary>
        /// <param name="hbmIndividualDayConcentrations"></param>
        /// <param name="hbmIndividualConcentrations"></param>
        /// <returns></returns>
        public ExposureMatrix Compute(
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations,
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations
        ) {
            if (_exposureType == ExposureType.Chronic) {
                return computeHumanMonitoringChronic(hbmIndividualConcentrations);
            } else {
                return computeHumanMonitoringAcute(hbmIndividualDayConcentrations);
            }
        }

        private ExposureMatrix computeHumanMonitoringChronic(
            ICollection<HbmIndividualConcentration> hbmIndividualConcentrations
        ) {
            if (hbmIndividualConcentrations == null) {
                return null;
            }

            var positiveIndividualConcentrations = hbmIndividualConcentrations
                .AsParallel()
                .Where(c => c.ConcentrationsBySubstance.Values.Any(r => r.Concentration > 0))
                .ToList();

            if (!positiveIndividualConcentrations.Any()) {
                return null;
            }
            var concentrationsBySubstance = hbmIndividualConcentrations
                .SelectMany(r =>
                    r.ConcentrationsBySubstance.Values,
                    (ic, sc) => (
                        individualId: ic.SimulatedIndividualId,
                        substance: sc.Substance,
                        concentration: sc.Concentration,
                        individual: ic.Individual
                    ))
                .GroupBy(gr => gr.substance)
                .Select(c => {
                    var ordered = c.OrderBy(r => r.individualId).ToList();
                    var concentrationsPerSubstance = ordered
                        .Select(i => i.concentration * _relativePotencyFactors[c.Key] * _membershipProbabilities[c.Key])
                        .ToList();
                    return (
                        substance: c.Key,
                        concentration: concentrationsPerSubstance,
                        sum: concentrationsPerSubstance.Sum(),
                        identifierIds: ordered.Select(ic => ic.individualId).ToList(),
                        individuals: ordered.Select(ic => ic.individual),
                        distinct: concentrationsPerSubstance.Distinct().Count()
                    );
                })
                .Where(r => r.sum > 0)
                .Where(r => r.distinct > 1)
                .OrderBy(r => r.substance.Code)
                .ToList();

            var substancesWithExposure = concentrationsBySubstance.Select(c => c.substance).ToList();

            double exposureDelegate(int i, int j) => concentrationsBySubstance[i].concentration[j];
            var exposureMatrix = new GeneralMatrix(
                concentrationsBySubstance.Count,
                hbmIndividualConcentrations.Count,
                exposureDelegate 
            );
            var identifierIds = concentrationsBySubstance.First().identifierIds;
            var individuals = concentrationsBySubstance.First().individuals.ToList();
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substancesWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Substances = substancesWithExposure,
                    Individuals = individuals,
                    Sds = Enumerable.Repeat(1d, substancesWithExposure.Count).ToList()
                };
            }
        }

        private ExposureMatrix computeHumanMonitoringAcute(
            ICollection<HbmIndividualDayConcentration> hbmIndividualDayConcentrations
        ) {
            if (hbmIndividualDayConcentrations == null) {
                return null;
            }

            var positiveIndividualDayConcentrations = hbmIndividualDayConcentrations
                .Where(r => r.ConcentrationsBySubstance.Values.Any(c => c.Concentration > 0))
                .ToList();

            if (!positiveIndividualDayConcentrations.Any()) {
                return null;
            }

            var concentrationsBySubstance = hbmIndividualDayConcentrations
                .SelectMany(gr => {
                    return _substances
                        .Select(substance => {
                            return (
                                individualDayId: gr.SimulatedIndividualDayId,
                                substance: substance,
                                concentration: gr.AverageEndpointSubstanceExposure(substance),
                                individual: gr.Individual
                            );
                        });
                })
                .GroupBy(gr => gr.substance)
                .Select(c => {
                    var ordered = c.OrderBy(r => r.individualDayId).ToList();
                    var concentrationsPerSubstance = ordered
                        .Select(i => i.concentration * _relativePotencyFactors[c.Key] * _membershipProbabilities[c.Key])
                        .ToList();
                    return (
                        substance: c.Key,
                        concentration: concentrationsPerSubstance,
                        sum: concentrationsPerSubstance.Sum(),
                        identifierIds: ordered.Select(ic => ic.individualDayId).ToList(),
                        individuals: ordered.Select(ic => ic.individual),
                        distinct: concentrationsPerSubstance.Distinct().Count()
                    );
                })
                .Where(r => r.sum > 0)
                .Where(r => r.distinct > 1)
                .OrderBy(r => r.substance.Code)
                .ToList();

            var substancesWithExposure = concentrationsBySubstance.Select(c => c.substance).ToList();

            double exposureDelegate(int i, int j) => concentrationsBySubstance[i].concentration[j];
            var exposureMatrix = new GeneralMatrix(
                concentrationsBySubstance.Count,
                hbmIndividualDayConcentrations.Count,
                exposureDelegate
            );
            var identifierIds = concentrationsBySubstance.First().identifierIds.Select((c, ix) => ix).ToList();
            var individuals = concentrationsBySubstance.First().individuals.ToList();
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substancesWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Substances = substancesWithExposure,
                    Individuals = individuals,
                    Sds = Enumerable.Repeat(1d, substancesWithExposure.Count).ToList()
                };
            }
        }

        /// <summary>
        /// Creates a exposure matrix object from the aggregate individual day exposures.
        /// </summary>
        /// <param name="aggregateIndividualDayExposures"></param>
        /// <returns></returns>
        private ExposureMatrix computeAggregateAcute(
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures
        ) {
            if (aggregateIndividualDayExposures == null) {
                return null;
            }
            var individualDaysWithExposure = aggregateIndividualDayExposures
                .AsParallel()
                .Where(c => c.TotalConcentrationAtTarget(_relativePotencyFactors, _membershipProbabilities, _isPerPerson) > 0)
                .ToList();

            var results = individualDaysWithExposure
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

            var intakesPerSubstance = results
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

            var substancesWithExposure = intakesPerSubstance.Select(r => r.substance).ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].intake[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, individualDaysWithExposure.Count, exposureDelegate);
            var individuals = intakesPerSubstance.First().individuals.ToList();
            var identifierIds = intakesPerSubstance.First().identifierIds;
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substancesWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Substances = substancesWithExposure,
                    Individuals = individuals,
                    Sds = Enumerable.Repeat(1d, substancesWithExposure.Count).ToList()
                };
            }
        }

        /// <summary>
        /// Creates a exposure matrix object from the aggregate individual exposures.
        /// </summary>
        /// <param name="aggregateIndividualExposures"></param>
        /// <returns></returns>
        private ExposureMatrix computeAggregateChronic(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures
        ) {
            if (aggregateIndividualExposures == null) {
                return null;
            }
            var individualsWithExposure = aggregateIndividualExposures
                .AsParallel()
                .Where(c => c.TotalConcentrationAtTarget(_relativePotencyFactors, _membershipProbabilities, _isPerPerson) > 0)
                .ToList();

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

            var substancesWithExposure = intakesPerSubstance.Select(c => c.substance).ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].intake[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, individualsWithExposure.Count, exposureDelegate);
            var individuals = intakesPerSubstance.First().individuals.ToList();
            var identifierIds = intakesPerSubstance.First().identifierIds;
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substancesWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Substances = substancesWithExposure,
                    Individuals = individuals,
                    Sds = Enumerable.Repeat(1d, substancesWithExposure.Count).ToList()
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

            var substancesWithExposure = intakesPerSubstance.Select(r => r.substance).ToList();

            double exposureDelegate(int i, int j) => intakesPerSubstance[i].intake[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, individualDaysWithIntake.Count, exposureDelegate);
            var identifierIds = intakesPerSubstance.First().identifierIds;
            var individuals = intakesPerSubstance.First().individuals.ToList();
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substancesWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Substances = substancesWithExposure,
                    Individuals = individuals,
                    Sds = Enumerable.Repeat(1d, substancesWithExposure.Count).ToList()
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

            var substancesWithExposure = intakesPerSubstance.Select(c => c.substance).ToList();
            var identifierIds = intakesPerSubstance.First().identifierIds;
            var individuals = intakesPerSubstance.First().individuals.ToList();
            double exposureDelegate(int i, int j) => intakesPerSubstance[i].concentration[j];
            var exposureMatrix = new GeneralMatrix(intakesPerSubstance.Count, identifierIds.Count, exposureDelegate);
            if (_exposureApproachType == ExposureApproachType.ExposureBased) {
                return calculateStandardizedExposureMatrix(individuals, substancesWithExposure, exposureMatrix);
            } else {
                return new ExposureMatrix() {
                    Exposures = exposureMatrix,
                    Substances = substancesWithExposure,
                    Individuals = individuals,
                    Sds = Enumerable.Repeat(1d, substancesWithExposure.Count).ToList()
                };
            }
        }

        private static ExposureMatrix calculateStandardizedExposureMatrix(
            ICollection<Individual> individuals,
            List<Compound> substancesWithExposure,
            GeneralMatrix exposureMatrix
        ) {
            var sd = exposureMatrix.Array.Select(c => Math.Sqrt(c.Variance())).ToArray();
            var zeroVarianceSubstances = sd.Select((c, i) => (variance: c, code: substancesWithExposure[i].Code ))
                .Where(c => c.variance == 0 ).Select(c => c.code)
                .ToList();
            if (zeroVarianceSubstances.Any()) {
                throw new Exception($"For substances: {string.Join(", ", zeroVarianceSubstances)} the variance is equal to zero, which is not allowed for standardized exposures");
            }
            var sdDiag = GeneralMatrix.CreateDiagonal(sd);
            var sdInverse = sdDiag.Inverse();
            return new ExposureMatrix() {
                Exposures = sdInverse.Multiply(exposureMatrix),
                Substances = substancesWithExposure,
                Individuals = individuals,
                Sds = sd.ToList()
            };
        }

        public (ExposureMatrix, double) Compute(ExposureMatrix exposureMatrix) {
            if (exposureMatrix == null) {
                return (null, double.NaN);
            }

            var exposureTranspose = exposureMatrix.Exposures.Transpose();

            var totalExposureCutOffPercentile = 0d;
            if (_totalExposureCutOff > 0) {
                totalExposureCutOffPercentile = exposureTranspose.Array.Select(c => c.Sum()).ToList().Percentile(_totalExposureCutOff);
            }

            var index = exposureTranspose.Array
                .AsParallel()
                .Select((c, ix) => {
                    var items = c.ToList();
                    var maximum = items.Max();
                    var cumulativeExposure = items.Sum();
                    if (cumulativeExposure / maximum >= _ratioCutOff && cumulativeExposure > totalExposureCutOffPercentile) {
                        return ix;
                    } else {
                        return -1;
                    }
                })
                .Where(c => c >= 0)
                .ToList();

            GeneralMatrix exposures = null;
            if (index.Count > 0) {
                exposures = exposureMatrix.Exposures.GetMatrix(0, exposureMatrix.Exposures.RowDimension - 1, index.ToArray());
            }
            var resultMatrix = new ExposureMatrix() {
                Exposures = exposures,
                Substances = exposureMatrix.Substances,
                Individuals = index.Select(ix => exposureMatrix.Individuals.ElementAt(ix)).ToList(),
                Sds = exposureMatrix.Sds,
            };
            return (resultMatrix, totalExposureCutOffPercentile);
        }
    }
}
