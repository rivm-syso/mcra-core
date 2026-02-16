using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Constants;
using MCRA.Simulation.Objects;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionCalculation {

    public class LinearDoseAggregationCalculator : IKineticConversionCalculator {

        public KineticConversionModelType ModelType => KineticConversionModelType.ConversionFactorModel;

        private readonly Compound _substance;

        private readonly List<Compound> _outputSubstances;

        protected readonly IDictionary<(ExposureRoute, ExposureTarget), IKineticConversionFactorModel> _kineticConversionFactorModels;

        public LinearDoseAggregationCalculator(
            Compound substance,
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels
        ) {
            _kineticConversionFactorModels = kineticConversionFactorModels
                .Where(r => r.SubstanceFrom == substance || r.SubstanceFrom == null)
                .GroupBy(r => (r.TargetFrom.ExposureRoute, r.TargetTo))
                .ToDictionary(
                    r => r.Key,
                    g => g.OrderByDescending(r => r.SubstanceFrom != null ? 1 : 0).First()
                );
            _outputSubstances = _kineticConversionFactorModels.Values
                .Select(r => r.SubstanceTo ?? substance)
                .ToList();
            _substance = substance;
        }

        public List<AggregateIndividualDayExposure> CalculateIndividualDayTargetExposures(
            ICollection<IExternalIndividualDayExposure> individualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<AggregateIndividualDayExposure>();
            foreach (var individualDayExposure in individualDayExposures) {
                var internalIndividualDayExposure = new AggregateIndividualDayExposure() {
                    SimulatedIndividualDayId = individualDayExposure.SimulatedIndividualDayId,
                    SimulatedIndividual = individualDayExposure.SimulatedIndividual,
                    Day = individualDayExposure.Day,
                    InternalTargetExposures = [],
                    ExternalIndividualDayExposures = [
                        individualDayExposure
                    ],
                };
                foreach (var target in targetUnits) {
                    var substanceTargetExposures = new Dictionary<Compound, ISubstanceTargetExposure>();
                    foreach (var substance in _outputSubstances) {
                        CheckKineticConversionModels(routes, target, substance);
                        var substanceTargetExposure = new SubstanceTargetExposure() {
                            Exposure = routes.Sum(route => computeInternalConcentration(
                                route,
                                substance,
                                exposureUnit,
                                individualDayExposure,
                                target)
                            ),
                            Substance = _substance
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
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            ICollection<TargetUnit> targetUnits,
            ProgressState progressState,
            IRandom generator
        ) {
            var result = new List<AggregateIndividualExposure>();
            foreach (var externalIndividualExposure in externalIndividualExposures) {
                var internalIndividualDayExposure = new AggregateIndividualExposure() {
                    SimulatedIndividual = externalIndividualExposure.SimulatedIndividual,
                    InternalTargetExposures = [],
                    ExternalIndividualDayExposures = externalIndividualExposure.ExternalIndividualDayExposures
                };
                foreach (var target in targetUnits) {
                    var substanceTargetExposures = new Dictionary<Compound, ISubstanceTargetExposure>();
                    foreach (var substance in _outputSubstances) {
                        CheckKineticConversionModels(routes, target, substance);
                        var substanceTargetExposure = new SubstanceTargetExposure() {
                            Exposure = routes
                               .Sum(route => computeInternalConcentration(exposureUnit, route, externalIndividualExposure, target, substance)),
                            Substance = _substance
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

        private void CheckKineticConversionModels(
            ICollection<ExposureRoute> routes,
            TargetUnit target,
            Compound substance
        ) {
            foreach (var route in routes) {
                if (!_kineticConversionFactorModels.TryGetValue((route, target.Target), out var kcm)) {
                    throw new Exception($"For substance: {substance.Code}, route: {route} and biological matrix: {target.Target.GetDisplayName()} no kinetic conversion factor is available. " +
                        $"Please upload missing kinetic conversion factors.");
                }
            }
        }

        /// <summary>
        /// Computes the dose at the target organ given an external dose of the
        /// specified exposure route.
        /// </summary>
        public double Forward(
            SimulatedIndividual individual,
            double dose,
            ExposureRoute route,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            if (_kineticConversionFactorModels.TryGetValue((route, internalTargetUnit.Target), out var model)
                // TODO: remove fallback on default exposure route. Breaking change for old projects where absorption
                // factors were used as kinetic conversion factors to internal targets linking to specific biological
                // matrices.
                || _kineticConversionFactorModels.TryGetValue((route, ExposureTarget.DefaultInternalExposureTarget), out model)
            ) {
                var inputAlignmentFactor = model
                    .UnitFrom
                    .GetAlignmentFactor(internalTargetUnit.ExposureUnit, _substance.MolecularMass, double.NaN);
                var outputAlignmentFactor = exposureUnit
                    .GetAlignmentFactor(model.UnitTo, _substance.MolecularMass, individual.BodyWeight);
                var targetDose = dose
                    * model.GetConversionFactor(individual.Age, individual.Gender)
                    * inputAlignmentFactor
                    * outputAlignmentFactor;
                return targetDose;
            }
            var msg = $"No kinetic conversion factor found for " +
                $"exposure route [{route}], " +
                $"target [{internalTargetUnit.Target}], " +
                $"and substance [{_substance.Name} ({_substance.Code})].";
            throw new Exception(msg);
        }

        /// <summary>
        /// Computes external dose that leads to the specified internal dose.
        /// </summary>
        public double Reverse(
            SimulatedIndividual individual,
            double internalDose,
            TargetUnit internalDoseUnit,
            ExposureRoute externalExposureRoute,
            ExposureUnitTriple externalExposureUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            if (_kineticConversionFactorModels.TryGetValue((externalExposureRoute, internalDoseUnit.Target), out var model)
                // TODO: remove fallback on default exposure route. Breaking change for old projects where absorption
                // factors were used as kinetic conversion factors to internal targets linking to specific biological
                // matrices.
                || _kineticConversionFactorModels.TryGetValue((externalExposureRoute, ExposureTarget.DefaultInternalExposureTarget), out model)
            ) {
                var inputAlignmentFactor = model
                    .UnitFrom
                    .GetAlignmentFactor(externalExposureUnit, _substance.MolecularMass, double.NaN);
                var outputAlignmentFactor = internalDoseUnit
                    .ExposureUnit
                    .GetAlignmentFactor(model.UnitTo, _substance.MolecularMass, individual.BodyWeight);
                var result = internalDose
                    * inputAlignmentFactor
                    * outputAlignmentFactor
                    / model.GetConversionFactor(individual.Age, individual.Gender);
                return result;
            }
            var msg = $"No kinetic conversion factor found for " +
                $"exposure route [{externalExposureRoute}], " +
                $"target [{internalDoseUnit.Target}], " +
                $"and substance [{_substance.Name} ({_substance.Code})].";
            throw new Exception(msg);
        }

        public ISubstanceTargetExposure Forward(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposureRoute route,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            // TODO refactor KCF: include/fix unit conversion
            var concentrationMassAlignmentFactor = exposureUnit.IsPerBodyWeight
                ? 1D / externalIndividualDayExposure.SimulatedIndividual.BodyWeight : 1D;
            var substanceExposure = externalIndividualDayExposure.GetExposure(route, _substance);
            var individual = externalIndividualDayExposure.SimulatedIndividual;
            if (_kineticConversionFactorModels.TryGetValue((route, targetUnit.Target), out var model)
                // TODO: remove fallback on default exposure route. Breaking change for old projects where absorption
                // factors were used as kinetic conversion factors to internal targets linking to specific biological
                // matrices.
                || _kineticConversionFactorModels.TryGetValue((route, ExposureTarget.DefaultInternalExposureTarget), out model)
            ) {
                return new SubstanceTargetExposure() {
                    Exposure = model.GetConversionFactor(individual.Age, individual.Gender) * substanceExposure * concentrationMassAlignmentFactor,
                    Substance = _substance,
                };
            }
            if (route == ExposureRoute.Undefined) {
                return null;
            }
            throw new Exception($"No absorption factor found for exposure route {route}.");
        }

        public List<KineticConversionFactorResultRecord> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new List<KineticConversionFactorResultRecord>();
            foreach (var exposureRoute in routes) {
                if (_kineticConversionFactorModels.TryGetValue((exposureRoute, targetUnit.Target), out var model)) {
                    var factor = getAlignedConversionFactor(externalIndividualExposures, exposureUnit, targetUnit, model);
                    var record = new KineticConversionFactorResultRecord() {
                        ExposureRoute = exposureRoute,
                        Substance = _substance,
                        ExternalExposureUnit = exposureUnit,
                        Factor = factor,
                        TargetUnit = targetUnit
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        public List<KineticConversionFactorResultRecord> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            var result = new List<KineticConversionFactorResultRecord>();
            foreach (var exposureRoute in routes) {
                if (_kineticConversionFactorModels.TryGetValue((exposureRoute, targetUnit.Target), out var model)) {
                    var factor = getAlignedAbsorptionFactor(externalIndividualDayExposures, exposureUnit, targetUnit, model);
                    var record = new KineticConversionFactorResultRecord() {
                        ExposureRoute = exposureRoute,
                        Substance = _substance,
                        ExternalExposureUnit = exposureUnit,
                        Factor = factor,
                        TargetUnit = targetUnit
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        /// <summary>
        /// Align the absorption factor
        /// </summary>
        /// <param name="externalIndividualExposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private double getAlignedConversionFactor(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IKineticConversionFactorModel model
        ) {
            return externalIndividualExposures
                .Select(c => {
                    var (doseUnitAlignment, targetUnitAlignment) = getConversionAlignmentFactor(exposureUnit, targetUnit, model, c.SimulatedIndividual);
                    return doseUnitAlignment * targetUnitAlignment * model.GetConversionFactor(c.SimulatedIndividual.Age, c.SimulatedIndividual.Gender);
                })
                .Average();
        }

        /// <summary>
        /// Align the absorption factor
        /// </summary>
        /// <param name="externalIndividualDatExposures"></param>
        /// <param name="exposureUnit"></param>
        /// <param name="targetUnit"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private double getAlignedAbsorptionFactor(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IKineticConversionFactorModel model
        ) {
            return externalIndividualDayExposures
                .Select(c => {
                    var (doseUnitAlignment, targetUnitAlignment) = getConversionAlignmentFactor(exposureUnit, targetUnit, model, c.SimulatedIndividual);
                    return doseUnitAlignment * targetUnitAlignment * model.GetConversionFactor(c.SimulatedIndividual.Age, c.SimulatedIndividual.Gender);
                })
                .Average();
        }


        private double computeInternalConcentration(
            ExposureRoute route,
            Compound substance,
            ExposureUnitTriple exposureUnit,
            IExternalIndividualDayExposure externalIndividualDayExposure,
            TargetUnit target
        ) {
            var simulatedIndividual = externalIndividualDayExposure.SimulatedIndividual;
            var routeExposure = externalIndividualDayExposure.GetExposure(route, substance);
            routeExposure = exposureUnit.IsPerBodyWeight ? routeExposure / simulatedIndividual.BodyWeight : routeExposure;
            return getTargetConcentration(exposureUnit, route, target, substance, simulatedIndividual, routeExposure);
        }

        private double computeInternalConcentration(
            ExposureUnitTriple exposureUnit,
            ExposureRoute route,
            IExternalIndividualExposure externalIndividualExposure,
            TargetUnit target,
            Compound substance
        ) {
            var simulatedIndividual = externalIndividualExposure.SimulatedIndividual;
            var routeExposure = externalIndividualExposure.ExternalIndividualDayExposures
                .Select(d => d.GetExposure(route, substance))
                .Average();
            routeExposure = exposureUnit.IsPerBodyWeight ? routeExposure / simulatedIndividual.BodyWeight : routeExposure;
            return getTargetConcentration(exposureUnit, route, target, substance, simulatedIndividual, routeExposure);
        }

        private double getTargetConcentration(
            ExposureUnitTriple exposureUnit,
            ExposureRoute route,
            TargetUnit target,
            Compound substance,
            SimulatedIndividual individual,
            double routeExposure
        ) {
            var conversionFactor = _kineticConversionFactorModels[(route, target.Target)];
            var factor = _kineticConversionFactorModels[(route, target.Target)].GetConversionFactor(individual.Age, individual.Gender);
            var (doseUnitAlignment, targetUnitAlignment) = getConversionAlignmentFactor(exposureUnit, target, conversionFactor, individual);
            var result = factor
                * doseUnitAlignment
                * targetUnitAlignment
                * routeExposure;
            return result;
        }

        /// <summary>
        /// The unit correction factor for aligning the kinetic conversion factor with the input unit and target unit.
        /// Two parts:
        /// 1) the input unit with the dose from of the kinetic conversion factor,
        /// 2) the output unit of the kinetic conversion factor with the target unit.
        /// </summary>
        private (double doseUnitAlignment, double targetUnitAlignment) getConversionAlignmentFactor(
           ExposureUnitTriple exposureUnit,
           TargetUnit targetUnit,
           IKineticConversionFactorModel conversionFactor,
           SimulatedIndividual individual
        ) {
            var doseUnitFrom = conversionFactor.UnitFrom;
            var doseUnitTo = conversionFactor.UnitTo;
            var doseUnitAlignment = exposureUnit
                .GetAlignmentFactor(
                    doseUnitFrom,
                    _substance.MolecularMass,
                    individual.BodyWeight
                );
            var targetUnitAlignment = doseUnitTo
                .GetAlignmentFactor(
                    targetUnit.ExposureUnit,
                    _substance.MolecularMass,
                    individual.BodyWeight
                );
            return (doseUnitAlignment, targetUnitAlignment);
        }
    }
}
