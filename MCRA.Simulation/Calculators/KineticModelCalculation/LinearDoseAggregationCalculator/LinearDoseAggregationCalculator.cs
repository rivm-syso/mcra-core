using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation {

    public class LinearDoseAggregationCalculator : IKineticModelCalculator {

        private readonly Compound _substance;

        protected readonly IDictionary<ExposurePathType, double> _absorptionFactors;

        public LinearDoseAggregationCalculator(
            Compound substance,
            IDictionary<ExposurePathType, double> absorptionFactors
        ) {
            _substance = substance;
            _absorptionFactors = absorptionFactors;
        }

        public virtual Compound Substance {
            get {
                return _substance;
            }
        }

        public virtual List<Compound> OutputSubstances {
            get {
                return new List<Compound>() { _substance };
            }
        }

        /// <summary>
        /// Returns the relative compartment weights of the output compartments that are supported
        /// by the kinetic model calculator.
        /// </summary>
        /// <returns>A collection (compartment, relative weight)</returns>
        protected virtual ICollection<(string compartment, double weight)> GetNominalRelativeCompartmentWeights() {
            //TODO, this needs further implementation. Not correct for combinations of kinetic model instances, should be the reference substance
            var result = new List<(string, double)> { (string.Empty, 1D) };
            return result;
        }

        public virtual List<IndividualDayTargetExposureCollection> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator = null
        ) {
            //TODO, needs further implementation
            var relativeCompartmentWeights = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);
            var relativeCompartmentWeight = relativeCompartmentWeights.First().Value;
            var result = new List<IndividualDaySubstanceTargetExposure>();
            foreach (var id in individualDayExposures) {
                result.Add(new IndividualDaySubstanceTargetExposure() {
                    SimulatedIndividualDayId = id.SimulatedIndividualDayId,
                    SubstanceTargetExposures = new List<ISubstanceTargetExposure>(){new SubstanceTargetExposure() {
                            SubstanceAmount = exposureRoutes
                                .Sum(route => _absorptionFactors[route] * relativeCompartmentWeight * getRouteSubstanceIndividualDayExposures(id, Substance, route)),
                            Substance = Substance
                        }
                    }
                });
            }
            var collection = new IndividualDayTargetExposureCollection() {
                Compartment = relativeCompartmentWeights.First().Key,
                IndividualDaySubstanceTargetExposures = result,
                TargetUnit = targetUnits.FirstOrDefault()           // TODO: linear dose model should specify which target(s) are supported by this calculator
            };
            return new List<IndividualDayTargetExposureCollection> { collection };
        }

        public virtual List<IndividualTargetExposureCollection> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator = null
        ) {
            //TODO, needs further implementation
            var relativeCompartmentWeights = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);
            var relativeCompartmentWeight = relativeCompartmentWeights.First().Value;
            var result = new List<IndividualSubstanceTargetExposure>();
            foreach (var externalIndividualExposure in individualExposures) {
                result.Add(new IndividualSubstanceTargetExposure() {
                    SimulatedIndividualId = externalIndividualExposure.SimulatedIndividualId,
                    SubstanceTargetExposures = new List<ISubstanceTargetExposure>(){ new SubstanceTargetExposure() {
                            SubstanceAmount = exposureRoutes
                                .Sum(route => _absorptionFactors[route]
                                    * relativeCompartmentWeight
                                    * getRouteSubstanceIndividualDayExposures(
                                        externalIndividualExposure.ExternalIndividualDayExposures,
                                        Substance,
                                        route
                                    ).Average()
                                ),
                            Substance = Substance,
                        }
                    }
                });
            }
            var collection = new IndividualTargetExposureCollection() {
                Compartment = relativeCompartmentWeights.First().Key,
                IndividualSubstanceTargetExposures = result,
                TargetUnit = targetUnits.FirstOrDefault()           // TODO: linear dose model should specify which target(s) are supported by this calculator
            };
            return new List<IndividualTargetExposureCollection>() { collection };
        }

        /// <summary>
        /// Computes the dose at the target organ given an external dose of the 
        /// specified exposure route.
        /// </summary>
        public virtual double CalculateTargetDose(
            double dose,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IRandom generator
        ) {
            //TODO, needs further implementation
            var relativeCompartmentWeights = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);
            if (_absorptionFactors.TryGetValue(exposureRoute, out var factor)) {
                var targetDose = exposureUnit.IsPerBodyWeight()
                    ? dose * factor
                    : dose * factor * relativeCompartmentWeights.First().Value;
                return targetDose;
            }
            return double.NaN;
        }

        /// <summary>
        /// Computes external dose that leads to the specified internal dose.
        /// </summary>
        public virtual double Reverse(
            double dose,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IRandom generator
        ) {
            var relativeCompartmentWeights = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);
            //TODO, needs further implementation
            if (_absorptionFactors.TryGetValue(exposureRoute, out var factor)) {
                var targetDose = exposureUnit.IsPerBodyWeight()
                    ? dose / factor
                    : dose / factor * (1D / relativeCompartmentWeights.First().Value);
                return targetDose;
            }
            return double.NaN;
        }

        /// <summary>
        /// Get external individual day exposures of the specified route and substance.
        /// </summary>
        protected List<double> getRouteSubstanceIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            Compound substance,
            ExposurePathType exposureRoute
        ) {
            var routeExposures = externalIndividualDayExposures
                .Select(individualDay => {
                    if (individualDay.ExposuresPerRouteSubstance.ContainsKey(exposureRoute)) {
                        return individualDay.ExposuresPerRouteSubstance[exposureRoute]
                            .Where(r => r.Compound == substance)
                            .Sum(r => r.Amount);
                    } else {
                        return 0d;
                    }
                })
                .ToList();
            return routeExposures;
        }

        private double getRouteSubstanceIndividualDayExposures(IExternalIndividualDayExposure externalIndividualDayExposure, Compound compound, ExposurePathType exposureRoute) {
            if (externalIndividualDayExposure.ExposuresPerRouteSubstance.TryGetValue(exposureRoute, out var routeExposures)) {
                return routeExposures.Where(r => r.Compound == compound).Sum(r => r.Amount);
            } else {
                return 0D;
            }
        }

        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public virtual ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            IRandom generator = null
        ) {
            var relativeCompartmentWeights = GetNominalRelativeCompartmentWeights().ToDictionary(c => c.Item1, c => c.Item2);

            //TODO, needs further implementation
            var relativeCompartmentWeight = relativeCompartmentWeights.First().Value;
            var substanceExposure = externalIndividualDayExposure.ExposuresPerRouteSubstance[exposureRoute]
                .Where(r => r.Compound == Substance)
                .Sum(r => r.Amount);
            return new SubstanceTargetExposure() {
                SubstanceAmount = _absorptionFactors[exposureRoute] * substanceExposure * relativeCompartmentWeight,
                Substance = Substance,
            };
        }
    }
}
