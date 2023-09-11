using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class InternalTargetExposuresCalculator : ITargetExposuresCalculator {

        private readonly ICollection<IKineticModelCalculator> _kineticModelCalculators;

        public InternalTargetExposuresCalculator(IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators) {
            _kineticModelCalculators = kineticModelCalculators.Values;
        }

        public ICollection<TargetIndividualDayExposure> ComputeTargetIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposureRouteType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        ) {
            var relativeCompartmentWeight = GetRelativeCompartmentWeight(_kineticModelCalculators);
            var result = externalIndividualDayExposures
                .Select(r => new TargetIndividualDayExposure() {
                    Individual = r.Individual,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    SimulatedIndividualDayId = r.SimulatedIndividualDayId,
                    TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                    RelativeCompartmentWeight = relativeCompartmentWeight,
                })
                .ToList();
            foreach (var calculator in _kineticModelCalculators) {
                var substanceIndividualDayTargetExposures = calculator
                    .CalculateIndividualDayTargetExposures(
                        externalIndividualDayExposures,
                        calculator.InputSubstance,
                        exposureRoutes,
                        exposureUnit,
                        relativeCompartmentWeight,
                        progressState,
                        generator
                    );
                var substanceIndividualTargetExposuresLookup = substanceIndividualDayTargetExposures
                    .ToDictionary(r => r.SimulatedIndividualDayId, r => r.SubstanceTargetExposures);
                foreach (var record in result) {
                    var kmSubstances = calculator.OutputSubstances.Intersect(substances).ToList();
                    foreach (var outputSubstance in kmSubstances) {
                        var targetExposure = substanceIndividualTargetExposuresLookup[record.SimulatedIndividualDayId]
                            .SingleOrDefault(c => c.Substance == outputSubstance);
                        record.TargetExposuresBySubstance.Add(outputSubstance, targetExposure);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Computes the relative compartment weight.
        /// </summary>
        /// <param name="kineticModelCalculators"></param>
        /// 
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public double GetRelativeCompartmentWeight(
            ICollection<IKineticModelCalculator> kineticModelCalculators
        ) {
            if (kineticModelCalculators == null || !kineticModelCalculators.Any()) {
                return 1D;
            } else {
                var allRelativeCompartmentWeights = kineticModelCalculators
                    .Where(r => r is PbpkModelCalculator)
                    .Select(r => r.GetNominalRelativeCompartmentWeight())
                    .Distinct().ToList();
                if (allRelativeCompartmentWeights.Count == 0) {
                    return 1D;
                }
                if (allRelativeCompartmentWeights.Count != 1) {
                    throw new Exception("Kinetic model instances do not have matching relative compartment weights.");
                }
                return allRelativeCompartmentWeights.First();
            }
        }

        public ICollection<TargetIndividualExposure> ComputeTargetIndividualExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposureRouteType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        ) {
            var relativeCompartmentWeight = GetRelativeCompartmentWeight(_kineticModelCalculators);

            var result = externalIndividualExposures
                .Select(r => new TargetIndividualExposure() {
                    Individual = r.Individual,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                    RelativeCompartmentWeight = relativeCompartmentWeight,
                })
                .ToList();

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
                var substanceIndividualTargetExposuresLookup = substanceIndividualTargetExposures.ToDictionary(r => r.SimulatedIndividualId, r => r.SubstanceTargetExposures);
                foreach (var record in result) {
                    var kmSubstances = calculator.OutputSubstances.Intersect(substances).ToList();
                    foreach (var kmSubstance in kmSubstances) {
                        var targetExposure = substanceIndividualTargetExposuresLookup[record.SimulatedIndividualId]
                           .SingleOrDefault(c => c.Substance == kmSubstance);
                        record.TargetExposuresBySubstance.Add(kmSubstance, targetExposure);
                    }
                }
            }

            return result;
        }

        public IDictionary<(ExposureRouteType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            // TODO kinetic models: How to compute absorption factors for metabolites?
            var result = new Dictionary<(ExposureRouteType, Compound), double>();
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

        public IDictionary<(ExposureRouteType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var result = new Dictionary<(ExposureRouteType, Compound), double>();
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
