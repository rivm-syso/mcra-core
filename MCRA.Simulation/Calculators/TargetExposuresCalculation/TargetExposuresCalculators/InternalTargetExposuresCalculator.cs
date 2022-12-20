using MCRA.Utils.Collections;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class InternalTargetExposuresCalculator : ITargetExposuresCalculator {

        private IDictionary<Compound, IKineticModelCalculator> _kineticModelCalculators;

        public InternalTargetExposuresCalculator(IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators) {
            _kineticModelCalculators = kineticModelCalculators;
        }

        public ICollection<TargetIndividualDayExposure> ComputeTargetIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit targetExposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        ) {
            var relativeCompartmentWeight = GetRelativeCompartmentWeight(indexSubstance, kineticModelInstances, _kineticModelCalculators);
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

            foreach (var substance in substances) {
                var calculator = _kineticModelCalculators[substance];
                var substanceIndividualDayTargetExposures = calculator.CalculateIndividualDayTargetExposures(externalIndividualDayExposures, substance, exposureRoutes, targetExposureUnit, relativeCompartmentWeight, progressState, generator);
                var substanceIndividualTargetExposuresLookup = substanceIndividualDayTargetExposures.ToDictionary(r => r.SimulatedIndividualDayId, r => r.SubstanceTargetExposure);
                foreach (var record in result) {
                    record.TargetExposuresBySubstance.Add(substance, substanceIndividualTargetExposuresLookup[record.SimulatedIndividualDayId]);
                }
            }

            return result;
        }

        public double GetRelativeCompartmentWeight(
            Compound indexSubstance,
            ICollection<KineticModelInstance> kineticModelInstances,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators
        ) {
            if (kineticModelInstances == null) {
                return 1D;
            } else if (!kineticModelInstances.Any()) {
                if (indexSubstance != null && kineticModelCalculators.TryGetValue(indexSubstance, out var calculator)) {
                    return calculator.GetNominalRelativeCompartmentWeight();
                } else {
                    return kineticModelCalculators.Values.First().GetNominalRelativeCompartmentWeight();
                }
            } else {
                var allRelativeCompartmentWeights = kineticModelInstances
                    .Select(r => kineticModelCalculators[r.Substance].GetNominalRelativeCompartmentWeight())
                    .Distinct();
                if (allRelativeCompartmentWeights.Count() != 1) {
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
            TargetUnit targetExposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        ) {
            var relativeCompartmentWeight = GetRelativeCompartmentWeight(indexSubstance, kineticModelInstances, _kineticModelCalculators);

            var result = externalIndividualExposures
                .Select(r => new TargetIndividualExposure() {
                    Individual = r.Individual,
                    IndividualSamplingWeight = r.IndividualSamplingWeight,
                    SimulatedIndividualId = r.SimulatedIndividualId,
                    TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                    RelativeCompartmentWeight = relativeCompartmentWeight,
                })
                .ToList();

            foreach (var substance in substances) {
                var calculator = _kineticModelCalculators[substance];
                var compoundIndividualTargetExposures = calculator.CalculateIndividualTargetExposures(externalIndividualExposures, substance, exposureRoutes, targetExposureUnit, relativeCompartmentWeight, progressState, generator);
                var compoundIndividualTargetExposuresLookup = compoundIndividualTargetExposures.ToDictionary(r => r.SimulatedIndividualId, r => r.SubstanceTargetExposure);
                foreach (var record in result) {
                    record.TargetExposuresBySubstance.Add(substance, compoundIndividualTargetExposuresLookup[record.SimulatedIndividualId]);
                }
            }

            return result;
        }

        public TwoKeyDictionary<ExposureRouteType, Compound, double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            TargetUnit targetExposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var result = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var substance in substances) {
                var calculator = _kineticModelCalculators[substance];
                var fittedAbsorptionFactors = calculator.ComputeAbsorptionFactors(
                    aggregateIndividualDayExposures, 
                    substance, 
                    exposureRoutes, 
                    targetExposureUnit, 
                    nominalBodyWeight, 
                    generator
                );
                foreach (var item in fittedAbsorptionFactors) {
                    result[item.Key, substance] = item.Value;
                }
            }
            return result;
        }

        public TwoKeyDictionary<ExposureRouteType, Compound, double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            TargetUnit targetExposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var result = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var substance in substances) {
                var calculator = _kineticModelCalculators[substance];
                var fittedAbsorptionFactors = calculator.ComputeAbsorptionFactors(
                    aggregateIndividualExposures,
                    substance,
                    exposureRoutes,
                    targetExposureUnit,
                    nominalBodyWeight,
                    generator
                );
                foreach (var item in fittedAbsorptionFactors) {
                    result[item.Key, substance] = item.Value;
                }
            }
            return result;
        }
    }
}
