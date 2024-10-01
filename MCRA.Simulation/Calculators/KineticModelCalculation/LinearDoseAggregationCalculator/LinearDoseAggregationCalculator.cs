using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.OpexProductDefinitions.Dto;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.AspNetCore.Routing;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation {

    public class LinearDoseAggregationCalculator : IKineticModelCalculator {

        private readonly Compound _substance;

        protected readonly IDictionary<(ExposurePathType, ExposureTarget), IKineticConversionFactorModel> _kineticConversionFactorModels;

        public LinearDoseAggregationCalculator(
            Compound substance,
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels
        ) {
            _kineticConversionFactorModels = kineticConversionFactorModels
                .Where(r => r.MatchesFromSubstance(substance))
                .GroupBy(r => (r.ConversionRule.ExposurePathType, r.ConversionRule.TargetTo))
                .ToDictionary(r => r.Key, g => g.OrderByDescending(r => r.IsSubstanceFromSpecific()).First());
            _substance = substance;
        }

        public virtual Compound Substance {
            get {
                return _substance;
            }
        }

        public virtual List<Compound> OutputSubstances {
            get {
                return [_substance];
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
                    foreach (var substance in OutputSubstances) {
                        var substanceTargetExposure = new SubstanceTargetExposure() {
                            Exposure = exposureRoutes
                                .Sum(route => computeInternalConcentration(exposureUnit, route, individualDayExposure, target, substance)),
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
                    foreach (var substance in OutputSubstances) {
                        CheckKineticConversionModels(exposureRoutes, target, substance);
                        var substanceTargetExposure = new SubstanceTargetExposure() {
                            Exposure = exposureRoutes
                               .Sum(route => computeInternalConcentration(exposureUnit, route, externalIndividualExposure, target, substance)),
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

        private void CheckKineticConversionModels(
            ICollection<ExposurePathType> exposureRoutes,
            TargetUnit target,
            Compound substance
        ) {
            foreach (var route in exposureRoutes) {
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
            Individual individual,
            double dose,
            ExposurePathType exposureRoute,
            ExposureUnitTriple exposureUnit,
            TargetUnit internalTargetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            if (_kineticConversionFactorModels.TryGetValue((exposureRoute, internalTargetUnit.Target), out var model)
                // TODO: remove fallback on default exposure route. Breaking change for old projects where absorption
                // factors were used as kinetic conversion factors to internal targets linking to specific biological
                // matrices.
                || _kineticConversionFactorModels.TryGetValue((exposureRoute, ExposureTarget.DefaultInternalExposureTarget), out model)
            ) {
                var inputAlignmentFactor = model
                    .ConversionRule
                    .DoseUnitFrom
                    .GetAlignmentFactor(internalTargetUnit.ExposureUnit, Substance.MolecularMass, double.NaN);
                var outputAlignmentFactor = exposureUnit
                    .GetAlignmentFactor(model.ConversionRule.DoseUnitTo, Substance.MolecularMass, individual.BodyWeight);
                var targetDose = dose
                    * model.ConversionRule.ConversionFactor
                    * inputAlignmentFactor
                    * outputAlignmentFactor;
                return targetDose;
            }
            var msg = $"No kinetic conversion factor found for " +
                $"exposure route [{exposureRoute}], " +
                $"target [{internalTargetUnit.Target}], " +
                $"and substance [{Substance.Name} ({Substance.Code})].";
            throw new Exception(msg);
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
            if (_kineticConversionFactorModels.TryGetValue((externalExposureRoute, internalDoseUnit.Target), out var model)
                // TODO: remove fallback on default exposure route. Breaking change for old projects where absorption
                // factors were used as kinetic conversion factors to internal targets linking to specific biological
                // matrices.
                || _kineticConversionFactorModels.TryGetValue((externalExposureRoute, ExposureTarget.DefaultInternalExposureTarget), out model)
            ) {
                var inputAlignmentFactor = model
                    .ConversionRule
                    .DoseUnitFrom
                    .GetAlignmentFactor(externalExposureUnit, Substance.MolecularMass, double.NaN);
                var outputAlignmentFactor = internalDoseUnit
                    .ExposureUnit
                    .GetAlignmentFactor(model.ConversionRule.DoseUnitTo, Substance.MolecularMass, individual.BodyWeight);
                var result = internalDose
                    * inputAlignmentFactor
                    * outputAlignmentFactor
                    / model.ConversionRule.ConversionFactor;
                return result;
            }
            var msg = $"No kinetic conversion factor found for " +
                $"exposure route [{externalExposureRoute}], " +
                $"target [{internalDoseUnit.Target}], " +
                $"and substance [{Substance.Name} ({Substance.Code})].";
            throw new Exception(msg);
        }

        public ISubstanceTargetExposure Forward(
            IExternalIndividualDayExposure externalIndividualDayExposure,
            ExposurePathType exposureRoute,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            ExposureType exposureType,
            IRandom generator
        ) {
            // TODO refactor KCF: include/fix unit conversion
            var concentrationMassAlignmentFactor = exposureUnit.IsPerBodyWeight()
                ? 1D / externalIndividualDayExposure.Individual.BodyWeight : 1D;
            var substanceExposure = externalIndividualDayExposure
                .ExposuresPerRouteSubstance[exposureRoute]
                .Where(r => r.Compound == Substance)
                .Sum(r => r.Amount);
            if (_kineticConversionFactorModels.TryGetValue((exposureRoute, targetUnit.Target), out var model)
                // TODO: remove fallback on default exposure route. Breaking change for old projects where absorption
                // factors were used as kinetic conversion factors to internal targets linking to specific biological
                // matrices.
                || _kineticConversionFactorModels.TryGetValue((exposureRoute, ExposureTarget.DefaultInternalExposureTarget), out model)
            ) {
                return new SubstanceTargetExposure() {
                    Exposure = model.ConversionRule.ConversionFactor * substanceExposure * concentrationMassAlignmentFactor,
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
            // TODO refactor KCF: include/fix unit conversion
            return _kineticConversionFactorModels
                .Where(c => c.Key.Item2 == targetUnit.Target)
                .ToDictionary(c => c.Key.Item1, c => c.Value.ConversionRule.ConversionFactor);
        }

        public IDictionary<ExposurePathType, double> ComputeAbsorptionFactors(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple exposureUnit,
            TargetUnit targetUnit,
            IRandom generator
        ) {
            // TODO refactor KCF: include/fix unit conversion
            return _kineticConversionFactorModels
                .Where(c => c.Key.Item2 == targetUnit.Target)
                .ToDictionary(c => c.Key.Item1, c => c.Value.ConversionRule.ConversionFactor);
        }

        private double computeInternalConcentration(
            ExposureUnitTriple exposureUnit,
            ExposurePathType route,
            IExternalIndividualDayExposure externalIndividualDayExposure,
            TargetUnit target,
            Compound substance
        ) {
            var individual = externalIndividualDayExposure.Individual;
            var exposure = (externalIndividualDayExposure.ExposuresPerRouteSubstance.TryGetValue(route, out var routeExposures))
                ? routeExposures.Where(r => r.Compound == substance).Sum(r => r.Amount)
                : 0D;
            return getTargetConcentration(exposureUnit, route, target, substance, individual, exposure);
        }

        private double computeInternalConcentration(
            ExposureUnitTriple exposureUnit,
            ExposurePathType route,
            IExternalIndividualExposure externalIndividualExposure,
            TargetUnit target,
            Compound substance
        ) {
            var individual = externalIndividualExposure.Individual;
            var routeExposure = externalIndividualExposure.ExternalIndividualDayExposures
                .Select(individualDay => {
                    if (individualDay.ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                        return exposures
                            .Where(r => r.Compound == substance)
                            .Sum(r => r.Amount);
                    } else {
                        return 0d;
                    }
                })
                .Average();
            return getTargetConcentration(exposureUnit, route, target, substance, individual, routeExposure);
        }

        private double getTargetConcentration(ExposureUnitTriple exposureUnit, ExposurePathType route, TargetUnit target, Compound substance, Individual individual, double routeExposure) {
            var conversionRule = _kineticConversionFactorModels[(route, target.Target)].ConversionRule;
            var factor = _kineticConversionFactorModels[(route, target.Target)].GetConversionFactor(individual.GetAge(), individual.GetGender());
            var doseUnitAlignment = exposureUnit
                .GetAlignmentFactor(
                    conversionRule.DoseUnitFrom,
                    substance.MolecularMass,
                    individual.BodyWeight
                );
            var targetUnitAlignment = conversionRule.DoseUnitTo
                .GetAlignmentFactor(
                    target.ExposureUnit,
                    Substance.MolecularMass,
                    individual.BodyWeight
                );
            var result = factor
                * doseUnitAlignment
                * targetUnitAlignment
                * routeExposure;
            return result;
        }
    }
}
