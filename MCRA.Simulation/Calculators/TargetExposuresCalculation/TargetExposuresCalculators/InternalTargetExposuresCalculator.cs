using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class InternalTargetExposuresCalculator : ITargetExposuresCalculator {

        private readonly ICollection<IKineticModelCalculator> _kineticModelCalculators;

        public InternalTargetExposuresCalculator(
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators
        ) {
            _kineticModelCalculators = kineticModelCalculators.Values;
        }

        public ICollection<TargetIndividualDayExposureCollection> ComputeTargetIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            IRandom generator,
            ProgressState progressState
        ) {
            var result = new Dictionary<string, TargetIndividualDayExposureCollection>();

            var relativeCompartmentWeights = GetRelativeCompartmentWeight(_kineticModelCalculators);

            foreach (var calculator in _kineticModelCalculators) {
                var substanceIndividualDayTargetExposures = calculator
                    .CalculateIndividualDayTargetExposures(
                        externalIndividualDayExposures,
                        calculator.InputSubstance,
                        exposureRoutes,
                        exposureUnit,
                        relativeCompartmentWeights,
                        progressState,
                        generator
                    );
                foreach (var collection in substanceIndividualDayTargetExposures) {
                    var substanceIndividualDayTargetExposuresLookup = collection.IndividualDaySubstanceTargetExposures
                         .ToDictionary(
                            r => r.SimulatedIndividualDayId, 
                            r => r.SubstanceTargetExposures
                        );

                    if (!result.TryGetValue(collection.Compartment, out var targetExposureCollection)) {
                        var targetIndividualExposures = externalIndividualDayExposures
                            .Select(r => new TargetIndividualDayExposure() {
                                Individual = r.Individual,
                                IndividualSamplingWeight = r.IndividualSamplingWeight,
                                SimulatedIndividualId = r.SimulatedIndividualId,
                                SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                            })
                            .ToList();

                        foreach (var record in targetIndividualExposures) {
                            var kmSubstances = calculator.OutputSubstances
                                .Intersect(substances)
                                .ToList();
                            foreach (var kmSubstance in kmSubstances) {
                                var targetExposure = substanceIndividualDayTargetExposuresLookup[record.SimulatedIndividualDayId]
                                   .SingleOrDefault(c => c.Substance == kmSubstance);
                                record.TargetExposuresBySubstance.Add(kmSubstance, targetExposure);
                            }
                        }
                        targetExposureCollection = new TargetIndividualDayExposureCollection() {
                            Compartment = collection.Compartment,
                            TargetUnit = collection.TargetUnit,
                            TargetIndividualDayExposures = targetIndividualExposures,
                        };
                        result.Add(collection.Compartment, targetExposureCollection);
                    } else {
                        foreach (var record in targetExposureCollection.TargetIndividualDayExposures) {
                            var kmSubstances = calculator.OutputSubstances.Intersect(substances).ToList();
                            foreach (var kmSubstance in kmSubstances) {
                                var targetExposure = substanceIndividualDayTargetExposuresLookup[record.SimulatedIndividualDayId]
                                   .SingleOrDefault(c => c.Substance == kmSubstance);
                                record.TargetExposuresBySubstance.Add(kmSubstance, targetExposure);
                            }
                        }
                    };
                }
            }
            return result.Select(c => c.Value).ToList();
        }

        public ICollection<TargetIndividualExposureCollection> ComputeTargetIndividualExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            IRandom generator,
            ProgressState progressState
        ) {
            var relativeCompartmentWeight = GetRelativeCompartmentWeight(_kineticModelCalculators);

            var result = new Dictionary<string, TargetIndividualExposureCollection>();

            foreach (var calculator in _kineticModelCalculators) {
                var substanceIndividualTargetExposures = calculator
                    .CalculateIndividualTargetExposures(
                        externalIndividualExposures,
                        calculator.InputSubstance,
                        exposureRoutes,
                        exposureUnit,
                        relativeCompartmentWeight,
                        progressState,
                        generator
                    );

                foreach (var collection in substanceIndividualTargetExposures) {
                    var substanceIndividualTargetExposuresLookup = collection. IndividualSubstanceTargetExposures
                         .ToDictionary(r => r.SimulatedIndividualId, r => r.SubstanceTargetExposures);

                    if (!result.TryGetValue(collection.Compartment, out var targetExposureCollection)) {
                        var targetIndividualExposures = externalIndividualExposures
                            .Select(r => new TargetIndividualExposure() {
                                Individual = r.Individual,
                                IndividualSamplingWeight = r.IndividualSamplingWeight,
                                SimulatedIndividualId = r.SimulatedIndividualId,
                            })
                            .ToList();

                        foreach (var record in targetIndividualExposures) {
                            var kmSubstances = calculator.OutputSubstances.Intersect(substances).ToList();
                            foreach (var kmSubstance in kmSubstances) {
                                var targetExposure = substanceIndividualTargetExposuresLookup[record.SimulatedIndividualId]
                                   .SingleOrDefault(c => c.Substance == kmSubstance);
                                record.TargetExposuresBySubstance.Add(kmSubstance, targetExposure);
                            }
                        }
                        targetExposureCollection = new TargetIndividualExposureCollection() {
                            Compartment = collection.Compartment,
                            TargetUnit = collection.TargetUnit,
                            TargetIndividualExposures  = targetIndividualExposures,
                        };
                        result.Add(collection.Compartment, targetExposureCollection);
                    } else {
                        foreach (var record in targetExposureCollection.TargetIndividualExposures) {
                            var kmSubstances = calculator.OutputSubstances.Intersect(substances).ToList();
                            foreach (var kmSubstance in kmSubstances) {
                                var targetExposure = substanceIndividualTargetExposuresLookup[record.SimulatedIndividualId]
                                   .SingleOrDefault(c => c.Substance == kmSubstance);
                                record.TargetExposuresBySubstance.Add(kmSubstance, targetExposure);
                            }
                        }
                    };

                }
            }
            return result.Select(c => c.Value).ToList();
        }

        /// <summary>
        /// Computes the relative compartment weight.
        /// </summary>
        /// <param name="kineticModelCalculators"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Dictionary<string, double> GetRelativeCompartmentWeight(
            ICollection<IKineticModelCalculator> kineticModelCalculators
        ) {
            if (kineticModelCalculators != null) {
                var allRelativeCompartmentWeights = kineticModelCalculators
                    .Where(r => r is DesolvePbkModelCalculator)
                    .SelectMany(r => r.GetNominalRelativeCompartmentWeight())
                    .Distinct()
                    .ToList();
                if (allRelativeCompartmentWeights.Count == 0) {
                    allRelativeCompartmentWeights.Add((string.Empty, 1D));
                }
                //TODO this is not needed for multiple compartments
                //if (allRelativeCompartmentWeights.Count != 1) {
                //    throw new Exception("Kinetic model instances do not have matching relative compartment weights.");
                //}
                return allRelativeCompartmentWeights.ToDictionary(c => c.Item1, c => c.Item2);
            }
            return null;
        }

        public IDictionary<(ExposurePathType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            // TODO kinetic models: How to compute absorption factors for metabolites?
            var result = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var instanceCalculator in _kineticModelCalculators) {
                var fittedAbsorptionFactors = instanceCalculator
                    .ComputeAbsorptionFactors(
                        aggregateIndividualDayExposures,
                        instanceCalculator.InputSubstance,
                        exposureRoutes,
                        exposureUnit,
                        nominalBodyWeight,
                        generator
                    );
                foreach (var item in fittedAbsorptionFactors) {
                    result[(item.Key, instanceCalculator.InputSubstance)] = item.Value;
                }
            }
            return result;
        }

        public IDictionary<(ExposurePathType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var result = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var instanceCalculator in _kineticModelCalculators) {
                var inputSubstance = instanceCalculator.InputSubstance;
                var fittedAbsorptionFactors = instanceCalculator.ComputeAbsorptionFactors(
                    aggregateIndividualExposures,
                    inputSubstance,
                    exposureRoutes,
                    exposureUnit,
                    nominalBodyWeight,
                    generator
                );
                foreach (var item in fittedAbsorptionFactors) {
                    result[(item.Key, inputSubstance)] = item.Value;
                }
            }
            return result;
        }
    }
}
