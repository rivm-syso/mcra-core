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

        public virtual Compound InputSubstance {
            get {
                return _substance;
            }
        }

        public virtual List<Compound> OutputSubstances {
            get {
                return new List<Compound>() { _substance };
            }
        }

        public virtual ICollection<(string, double)> GetNominalRelativeCompartmentWeight() {
            //TODO, this needs further implementation. Not correct for combinations of kinetic model instances, should be the reference substance
            var result = new List<(string, double)> { (string.Empty, 1D) };
            return result;
        }

        public virtual List<IndividualDayTargetExposureCollection> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IDictionary<string, double> relativeCompartmentWeights,
            ProgressState progressState,
            IRandom generator = null
        ) {
            //TODO, needs further implementation
            var relativeCompartmentWeight = relativeCompartmentWeights.First().Value;
            var result = new List<IndividualDaySubstanceTargetExposure>();
            foreach (var id in individualDayExposures) {
                result.Add(new IndividualDaySubstanceTargetExposure() {
                    SimulatedIndividualDayId = id.SimulatedIndividualDayId,
                    SubstanceTargetExposures = new List<ISubstanceTargetExposure>(){new SubstanceTargetExposure() {
                            SubstanceAmount = exposureRoutes
                                .Sum(route => _absorptionFactors[route] * relativeCompartmentWeight * getRouteSubstanceIndividualDayExposures(id, substance, route)),
                            Substance = substance
                        }
                    }
                });
            }
            var collection = new IndividualDayTargetExposureCollection() {
                Compartment = relativeCompartmentWeights.First().Key,
                IndividualDaySubstanceTargetExposures = result
            };
            return new List<IndividualDayTargetExposureCollection>() { collection };
        }

        public virtual List<IndividualTargetExposureCollection> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            Compound substance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            IDictionary<string, double> relativeCompartmentWeights,
            ProgressState progressState,
            IRandom generator = null
        ) {
            //TODO, needs further implementation
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
                                        substance,
                                        route
                                    ).Average()
                                ),
                            Substance = substance,
                        }
                    }
                });
            }
            var collection = new IndividualTargetExposureCollection() {
                Compartment = relativeCompartmentWeights.First().Key,
                IndividualSubstanceTargetExposures = result
            };
            return new List<IndividualTargetExposureCollection>() { collection };
        }

        /// <summary>
        /// Computes the dose at the target organ given an external dose of the 
        /// specified exposure route.
        /// </summary>
        /// <param name="dose"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="exposureType"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="bodyWeight"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual double CalculateTargetDose(
            double dose,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IDictionary<string, double> relativeCompartmentWeights,
            IRandom generator
        ) {
            //TODO, needs further implementation
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
        /// <param name="dose"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoute"></param>
        /// <param name="exposureType"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="bodyWeight"></param>
        /// <param name="relativeCompartmentWeight"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual double Reverse(
            double dose,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            double bodyWeight,
            IDictionary<string, double> relativeCompartmentWeights,
            IRandom generator
        ) {
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
        /// <param name="externalIndividualDayExposures"></param>
        /// <param name="substance"></param>
        /// <param name="exposureRoute"></param>
        /// <returns></returns>
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
                            .Sum(r => r.Exposure);
                    } else {
                        return 0d;
                    }
                })
                .ToList();
            return routeExposures;
        }

        private double getRouteSubstanceIndividualDayExposures(IExternalIndividualDayExposure externalIndividualDayExposure, Compound compound, ExposurePathType exposureRoute) {
            if (externalIndividualDayExposure.ExposuresPerRouteSubstance.TryGetValue(exposureRoute, out var routeExposures)) {
                return routeExposures.Where(r => r.Compound == compound).Sum(r => r.Exposure);
            } else {
                return 0D;
            }
        }

        public virtual IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            Compound compound,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public virtual IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            Compound compound,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public virtual ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            Compound substance,
            ExposurePathType exposureRoute,
            ExposureType exposureType,
            ExposureUnitTriple exposureUnit,
            IDictionary<string, double> relativeCompartmentWeights,
            IRandom generator = null
        ) {
            //TODO, needs further implementation
            var relativeCompartmentWeight = relativeCompartmentWeights.First().Value;
            var substanceExposure = externalIndividualDayExposure.ExposuresPerRouteSubstance[exposureRoute]
                .Where(r => r.Compound == substance)
                .Sum(r => r.Exposure);
            return new SubstanceTargetExposure() {
                SubstanceAmount = _absorptionFactors[exposureRoute] * substanceExposure * relativeCompartmentWeight,
                Substance = substance,
            };
        }
    }
}
