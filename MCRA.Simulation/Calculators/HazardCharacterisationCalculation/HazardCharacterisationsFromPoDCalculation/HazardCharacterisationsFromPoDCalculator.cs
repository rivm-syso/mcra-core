using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using SQLitePCL;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromPoDCalculation {
    public class HazardCharacterisationsFromPoDCalculator {

        /// <summary>
        /// Derives hazard characterisations from points of departure.
        /// </summary>
        /// <param name="hazardDose"></param>
        /// <param name="hazardDoseTypeConverter"></param>
        /// <param name="targetDoseUnit"></param>
        /// <param name="exposureType"></param>
        /// <param name="intraSpeciesVariabilityModels"></param>
        /// <param name="kineticConversionFactorCalculator"></param>
        /// <param name="interSpeciesFactorModels"></param>
        /// <param name="additionalAssessmentFactor"></param>
        /// <param name="kineticModelRandomGenerator"></param>
        /// <returns></returns>
        public IHazardCharacterisationModel Compute(
            Data.Compiled.Objects.PointOfDeparture hazardDose,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetDoseUnit,
            ExposureType exposureType,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            KineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            double additionalAssessmentFactor,
            IRandom kineticModelRandomGenerator
        ) {
            // TODO: get correct specific target (biological matrix or external target)
            var target = kineticConversionFactorCalculator.TargetDoseLevel == TargetLevelType.External 
                ? new ExposureTarget(ExposureRouteType.Dietary)
                : new ExposureTarget(BiologicalMatrix.WholeBody);
            var expressionTypeConversionFactor = hazardDoseTypeConverter.GetExpressionTypeConversionFactor(hazardDose.PointOfDepartureType);
            var interSpeciesFactor = InterSpeciesFactorModelsBuilder
                .GetInterSpeciesFactor(interSpeciesFactorModels, hazardDose.Effect, hazardDose.Species, hazardDose.Compound);
            var alignedTestSystemHazardDose = hazardDoseTypeConverter.ConvertToTargetUnit(hazardDose.DoseUnit, hazardDose.Compound, hazardDose.LimitDose);
            var targetUnitAlignmentFactor = alignedTestSystemHazardDose / hazardDose.LimitDose;
            var kineticConversionFactor = kineticConversionFactorCalculator.ComputeKineticConversionFactor(
                alignedTestSystemHazardDose * (1D / interSpeciesFactor) * expressionTypeConversionFactor,
                targetDoseUnit,
                hazardDose.Compound,
                hazardDose.Species,
                hazardDose.Effect.KeyEventOrgan,
                hazardDose.ExposureRoute,
                exposureType,
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
            var plower = alignedTestSystemHazardDose * combinedAssessmentFactor * Math.Exp(NormalDistribution.InvCDF(0, 1, 0.025) * Math.Log(intraSpeciesGeometricStandardDeviation));
            var pupper = alignedTestSystemHazardDose * combinedAssessmentFactor * Math.Exp(NormalDistribution.InvCDF(0, 1, 0.975) * Math.Log(intraSpeciesGeometricStandardDeviation));

            var result = new HazardCharacterisationModel() {
                Code = hazardDose.Code,
                Substance = hazardDose.Compound,
                Target = target,
                Value = alignedTestSystemHazardDose * combinedAssessmentFactor,
                PLower = plower, 
                PUpper = pupper,
                PotencyOrigin = hazardDose.PointOfDepartureType.ToPotencyOrigin(),
                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                GeometricStandardDeviation = intraSpeciesGeometricStandardDeviation,
                CombinedAssessmentFactor = combinedAssessmentFactor,
                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                    PoD = hazardDose,
                    Species = hazardDose.Species,
                    Effect = hazardDose.Effect,
                    Organ = null,
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
                },
                DoseUnit = targetDoseUnit.ExposureUnit,
            };
            return result;
        }
    }
}
