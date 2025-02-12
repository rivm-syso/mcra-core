﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromPoDCalculation {
    public class HazardCharacterisationsFromPoDCalculator {

        /// <summary>
        /// Derives hazard characterisations from points of departure.
        /// </summary>
        public IHazardCharacterisationModel Compute(
            Data.Compiled.Objects.PointOfDeparture hazardDose,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            ExposureType exposureType,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            KineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            double additionalAssessmentFactor,
            IRandom kineticModelRandomGenerator
        ) {
            var expressionTypeConversionFactor = hazardDoseTypeConverter
                .GetExpressionTypeConversionFactor(hazardDose.PointOfDepartureType);
            var interSpeciesFactor = InterSpeciesFactorModelsBuilder
                .GetInterSpeciesFactor(interSpeciesFactorModels, hazardDose.Effect, hazardDose.Species, hazardDose.Compound);
            var alignedTestSystemHazardDose = hazardDoseTypeConverter
                .ConvertToTargetUnit(hazardDose.DoseUnit, hazardDose.Compound, hazardDose.LimitDose);
            var targetUnitAlignmentFactor = alignedTestSystemHazardDose / hazardDose.LimitDose;

            var kineticConversionFactor = kineticConversionFactorCalculator
                .ComputeKineticConversionFactor(
                    alignedTestSystemHazardDose * (1D / interSpeciesFactor) * expressionTypeConversionFactor,
                    targetUnit,
                    hazardDose.Compound,
                    exposureType,
                    hazardDose.TargetUnit,
                    kineticModelRandomGenerator
                );
            var intraSpeciesVariabilityModel = intraSpeciesVariabilityModels.Get(hazardDose.Effect, hazardDose.Compound);
            var intraSpeciesGeometricMean = intraSpeciesVariabilityModel?.Factor ?? 1D;
            var intraSpeciesGeometricStandardDeviation = intraSpeciesVariabilityModel?.GeometricStandardDeviation ?? double.NaN;
            var combinedAssessmentFactor = (1D / interSpeciesFactor)
                * (1D / intraSpeciesGeometricMean)
                * kineticConversionFactor
                * expressionTypeConversionFactor
                * (1D / additionalAssessmentFactor);

            var result = new HazardCharacterisationModel() {
                Code = hazardDose.Code,
                Substance = hazardDose.Compound,
                TargetUnit = targetUnit,
                Value = alignedTestSystemHazardDose * combinedAssessmentFactor,
                PotencyOrigin = hazardDose.PointOfDepartureType.ToPotencyOrigin(),
                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                GeometricStandardDeviation = intraSpeciesGeometricStandardDeviation,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                    PoD = hazardDose,
                    Species = hazardDose.Species,
                    Effect = hazardDose.Effect,
                    Organ = hazardDose.BiologicalMatrix == BiologicalMatrix.Undefined
                        ? null : hazardDose.BiologicalMatrix.GetShortDisplayName(),
                    ExpressionType = hazardDose.ExpressionType,
                    ExposureRoute = hazardDose.ExposureRoute,
                    HazardDose = hazardDose.LimitDose,
                    DoseUnit = hazardDose.DoseUnit,
                    TargetUnitAlignmentFactor = targetUnitAlignmentFactor,
                    ExpressionTypeConversionFactor = expressionTypeConversionFactor,
                    InterSystemConversionFactor = 1D / interSpeciesFactor,
                    IntraSystemConversionFactor = 1D / intraSpeciesGeometricMean,
                    AdditionalConversionFactor = 1D / additionalAssessmentFactor,
                    KineticConversionFactor = kineticConversionFactor,
                    DoseResponseRelation = new DoseResponseRelation() {
                        DoseResponseModelEquation = hazardDose.DoseResponseModelEquation,
                        DoseResponseModelParameterValues = hazardDose.DoseResponseModelParameterValues,
                        CriticalEffectSize = hazardDose.CriticalEffectSize,
                    },
                }
            };
            return result;
        }
    }
}
