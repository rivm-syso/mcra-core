using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation {

    public class LinearDoseAggregationCalculator : IKineticModelCalculator {

        private readonly Compound _substance;
        private readonly TargetUnit _inputUnit;
        private readonly TargetUnit _outputUnit;

        protected readonly IDictionary<ExposurePathType, double> _absorptionFactors;

        public LinearDoseAggregationCalculator(
            Compound substance,
            IDictionary<ExposurePathType, double> kineticConversionFactors
        ) {
            _substance = substance;
            _inputUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay, ExposureRoute.Oral);
            _outputUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerKg, BiologicalMatrix.WholeBody);
            _absorptionFactors = kineticConversionFactors;
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

        public List<AggregateIndividualDayExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<AggregateIndividualDayExposure>();
            foreach (var individualDayExposure in individualDayExposures) {
                var internalIndividualDayExposure = new AggregateIndividualDayExposure() {
                    SimulatedIndividualId = individualDayExposure.SimulatedIndividualId,
                    SimulatedIndividualDayId = individualDayExposure.SimulatedIndividualDayId,
                    IndividualSamplingWeight = individualDayExposure.IndividualSamplingWeight,
                    Individual = individualDayExposure.Individual,
                    Day = individualDayExposure.Day,
                    InternalTargetExposures = new(),
                    ExternalIndividualDayExposures = new List<IExternalIndividualDayExposure>() {
                        individualDayExposure
                    },
                };
                foreach (var target in targetUnits) {
                    var substanceTargetExposures = new Dictionary<Compound, ISubstanceTargetExposure>();
                    var exposureAlignmentFactor = getExposureAlignmentFactor(
                        exposureUnit,
                        target,
                        individualDayExposure.Individual.BodyWeight
                    );
                    foreach (var substance in OutputSubstances) {
                        var substanceTargetExposure = new SubstanceTargetExposure() {
                            Exposure = exposureRoutes
                                .Sum(route => _absorptionFactors[route]
                                    * exposureAlignmentFactor
                                    * getRouteSubstanceIndividualDayExposures(individualDayExposure, Substance, route)
                                ),
                            Substance = Substance
                        };
                        substanceTargetExposures[substance] = substanceTargetExposure;
                    }
                    internalIndividualDayExposure.InternalTargetExposures[target.Target]
                        = substanceTargetExposures;
                }
                result.Add(internalIndividualDayExposure);
            }
            return result;
        }

        public List<AggregateIndividualExposure> CalculateIndividualTargetExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<AggregateIndividualExposure>();
            foreach (var externalIndividualExposure in externalIndividualExposures) {
                var internalIndividualDayExposure = new AggregateIndividualExposure() {
                    SimulatedIndividualId = externalIndividualExposure.SimulatedIndividualId,
                    IndividualSamplingWeight = externalIndividualExposure.IndividualSamplingWeight,
                    Individual = externalIndividualExposure.Individual,
                    InternalTargetExposures = new(),
                    ExternalIndividualDayExposures = externalIndividualExposure.ExternalIndividualDayExposures
                };
                foreach (var target in targetUnits) {
                    var substanceTargetExposures = new Dictionary<Compound, ISubstanceTargetExposure>();
                    var exposureAlignmentFactor = getExposureAlignmentFactor(
                        exposureUnit,
                        target,
                        externalIndividualExposure.Individual.BodyWeight
                    );
                    foreach (var substance in OutputSubstances) {
                        var substanceTargetExposure = new SubstanceTargetExposure() {
                            Exposure = exposureRoutes
                                .Sum(route => _absorptionFactors[route]
                                    * exposureAlignmentFactor
                                    * getRouteSubstanceIndividualDayExposures(
                                        externalIndividualExposure.ExternalIndividualDayExposures,
                                        Substance,
                                        route
                                    ).Average()
                                ),
                            Substance = Substance
                        };
                        substanceTargetExposures[substance] = substanceTargetExposure;
                    }
                    internalIndividualDayExposure.InternalTargetExposures[target.Target]
                        = substanceTargetExposures;
                    result.Add(internalIndividualDayExposure);
                }
            }
            return result;
        }

        /// <summary>
        /// Computes the dose at the target organ given an external dose of the 
        /// specified exposure route.
        /// </summary>
        public double Forward(
            Individual individual,
            double dose,
            ExposurePathType exposureRoute,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {

            var inputAlignmentFactor = _inputUnit.GetAlignmentFactor(internalTargetUnit, Substance.MolecularMass, double.NaN);
            var outputAlignmentFactor = exposureUnit.GetAlignmentFactor(_outputUnit.ExposureUnit, Substance.MolecularMass, individual.BodyWeight);

            if (_absorptionFactors.TryGetValue(exposureRoute, out var factor)) {
                var targetDose = dose * factor * inputAlignmentFactor * outputAlignmentFactor;
                return targetDose;
            }
            return double.NaN;
        }

        /// <summary>
        /// Computes external dose that leads to the specified internal dose.
        /// </summary>
        public double Reverse(
            Individual individual,
            double internalDose,
            TargetUnit internalDoseUnit,
            ExposurePathType externalExposureRoute,
            ExposureUnitTriple externalExposureUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            if (_absorptionFactors.TryGetValue(externalExposureRoute, out var factor)) {
                var inputAlignmentFactor = internalDoseUnit.GetAlignmentFactor(_inputUnit, Substance.MolecularMass, double.NaN);
                var outputAlignmentFactor = _outputUnit.ExposureUnit.GetAlignmentFactor(externalExposureUnit, Substance.MolecularMass, individual.BodyWeight);
                var result = internalDose * inputAlignmentFactor * outputAlignmentFactor / factor;
                return result;
            }
            throw new Exception($"No absorption factor found for exposure route {externalExposureRoute}.");
        }

        public ISubstanceTargetExposure Forward(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposurePathType exposureRoute,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            //TODO, needs further implementation
            //throw new NotImplementedException();
            var concentrationMassAlignmentFactor = exposureUnit.IsPerBodyWeight()
                ? 1D / externalIndividualDayExposure.Individual.BodyWeight : 1D;
            var substanceExposure = externalIndividualDayExposure
                .ExposuresPerRouteSubstance[exposureRoute]
                .Where(r => r.Compound == Substance)
                .Sum(r => r.Amount);
            if (_absorptionFactors.TryGetValue(exposureRoute, out var factor)) {
                return new SubstanceTargetExposure() {
                    Exposure = factor * substanceExposure * concentrationMassAlignmentFactor,
                    Substance = Substance,
                };
            }
            if (exposureRoute == ExposurePathType.Undefined) {
                return null;
            }
            throw new Exception($"No absorption factor found for exposure route {exposureRoute}.");
        }

        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            return _absorptionFactors;
        }

        /// <summary>
        /// Alignment factor for the substance amount and concentration mass unit.
        /// </summary>
        private static double getExposureAlignmentFactor(
            ExposureUnitTriple exposureUnit,
            TargetUnit target,
            double bodyWeight
        ) {
            if (!exposureUnit.IsPerBodyWeight() && !target.IsPerBodyWeight()) {
                throw new NotImplementedException();
            }
            var amountUnitAlignment = exposureUnit.SubstanceAmountUnit.GetMultiplicationFactor(target.SubstanceAmountUnit);
            var concentrationMassUnitAlignment = (exposureUnit.IsPerBodyWeight() || target.IsPerBodyWeight())
                ? 1D / bodyWeight : 1D;
            var exposureAlignmentFactor = amountUnitAlignment * concentrationMassUnitAlignment;
            return exposureAlignmentFactor;
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

        private double getRouteSubstanceIndividualDayExposures(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            Compound compound,
            ExposurePathType exposureRoute
        ) {
            if (externalIndividualDayExposure.ExposuresPerRouteSubstance.TryGetValue(exposureRoute, out var routeExposures)) {
                return routeExposures.Where(r => r.Compound == compound).Sum(r => r.Amount);
            } else {
                return 0D;
            }
        }
    }
}
