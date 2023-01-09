using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation {

    public class LinearDoseAggregationCalculator : IKineticModelCalculator {

        private readonly Compound _substance;

        protected readonly Dictionary<ExposureRouteType, double> _absorptionFactors;

        public LinearDoseAggregationCalculator(
            Compound substance, 
            Dictionary<ExposureRouteType, double> absorptionFactors
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

        public virtual double GetNominalRelativeCompartmentWeight() {
            return 1D;
        }

        public virtual List<IndividualDaySubstanceTargetExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            ProgressState progressState,
            IRandom generator = null
        ) {
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
            return result;
        }

        public virtual List<IndividualSubstanceTargetExposure> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> individualExposures,
            Compound substance,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            ProgressState progressState,
            IRandom generator = null
        ) {
            var result = new List<IndividualSubstanceTargetExposure>();
            foreach (var externalIndividualExposure in individualExposures) {
                result.Add(new IndividualSubstanceTargetExposure() {
                    SimulatedIndividualId = externalIndividualExposure.SimulatedIndividualId,
                    SubstanceTargetExposures = new List<ISubstanceTargetExposure>(){ new SubstanceTargetExposure() {
                            SubstanceAmount = exposureRoutes
                                .Sum(route => _absorptionFactors[route] * relativeCompartmentWeight * getRouteSubstanceIndividualDayExposures(externalIndividualExposure.ExternalIndividualDayExposures, substance, route).Average()),
                            Substance = substance,
                        }
                    }
                });
            }
            return result;
        }

        /// <summary>
        /// Computes the dose at the target organ given an external dose of the specified exposure route.
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
            ExposureRouteType exposureRoute,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double bodyWeight,
            double relativeCompartmentWeight,
            IRandom generator
        ) {
            if (_absorptionFactors.TryGetValue(exposureRoute, out var factor)) {
                var targetDose = exposureUnit.IsPerBodyWeight()
                    ? dose * factor
                    : dose * factor * relativeCompartmentWeight;
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
            ExposureRouteType exposureRoute,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double bodyWeight,
            double relativeCompartmentWeight,
            IRandom generator
        ) {
            if (_absorptionFactors.TryGetValue(exposureRoute, out var factor)) {
                var targetDose = exposureUnit.IsPerBodyWeight()
                    ? dose / factor
                    : dose / factor * (1D / relativeCompartmentWeight);
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
            ExposureRouteType exposureRoute
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

        private double getRouteSubstanceIndividualDayExposures(IExternalIndividualDayExposure externalIndividualDayExposure, Compound compound, ExposureRouteType exposureRoute) {
            if (externalIndividualDayExposure.ExposuresPerRouteSubstance.TryGetValue(exposureRoute, out var routeExposures)) {
                return routeExposures.Where(r => r.Compound == compound).Sum(r => r.Exposure);
            } else {
                return 0D;
            }
        }

        public virtual Dictionary<ExposureRouteType, double> ComputeAbsorptionFactors(List<AggregateIndividualExposure> aggregateIndividualExposures,
            Compound compound,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public virtual Dictionary<ExposureRouteType, double> ComputeAbsorptionFactors(List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            Compound compound,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit exposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public virtual ISubstanceTargetExposure CalculateInternalDoseTimeCourse(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            Compound substance,
            ExposureRouteType exposureRoute,
            ExposureType exposureType,
            TargetUnit exposureUnit,
            double relativeCompartmentWeight,
            IRandom generator = null
        ) {
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
