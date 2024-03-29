﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationFromDoseResponseModelCalculation {
    class HazardCharacterisationFromDoseResponseModelCalculator {

        public List<IHazardCharacterisationModel> Compute(
            ICollection<Compound> substances,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetUnit,
            ExposureType exposureType,
            ILookup<Response, EffectRepresentation> representativeResponses,
            DoseResponseModel doseResponseModel,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            KineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            double additionalAssessmentFactor,
            bool useBMDL,
            IRandom kineticModelRandomGenerator
        ) {
            var result = new List<IHazardCharacterisationModel>();
            var response = doseResponseModel.Response;

            if (doseResponseModel.DoseResponseModelBenchmarkDoses != null) {
                var benchmarkDoses = doseResponseModel.DoseResponseModelBenchmarkDoses.Values
                    .Where(r => substances.Contains(r.Substance))
                    .ToList();
                var representations = representativeResponses[response].ToList();
                foreach (var representation in representations) {
                    if (representation.HasBenchmarkResponse()) {
                        foreach (var benchmarkDose in benchmarkDoses) {
                            var specifiedBenchMarkDose = useBMDL ? benchmarkDose.BenchmarkDoseLower: benchmarkDose.BenchmarkDose;
                            var alignedTestSystemHazardDose = hazardDoseTypeConverter.ConvertToTargetUnit(doseResponseModel.DoseUnit, benchmarkDose.Substance, specifiedBenchMarkDose);
                            var targetUnitAlignmentFactor = alignedTestSystemHazardDose / specifiedBenchMarkDose;
                            var expressionTypeConversionFactor = hazardDoseTypeConverter.GetExpressionTypeConversionFactor(PointOfDepartureType.Bmd);
                            var interSpeciesFactor = InterSpeciesFactorModelsBuilder
                                .GetInterSpeciesFactor(interSpeciesFactorModels, representation.Effect, response.TestSystem.Species, benchmarkDose.Substance);
                            var kineticConversionFactor = kineticConversionFactorCalculator
                                .ComputeKineticConversionFactor(
                                    alignedTestSystemHazardDose * (1D / interSpeciesFactor) * expressionTypeConversionFactor,
                                    targetUnit,
                                    benchmarkDose.Substance,
                                    response.TestSystem.Species,
                                    response.TestSystem.Organ,
                                    response.TestSystem.ExposureRoute,
                                    exposureType,
                                    kineticModelRandomGenerator
                                );
                            var intraSpeciesFactorModel = intraSpeciesVariabilityModels.Get(representation.Effect, benchmarkDose.Substance);
                            var intraSpeciesGeometricMean = intraSpeciesFactorModel?.Factor ?? 1D;
                            var intraSpeciesGeometricStandardDeviation = intraSpeciesFactorModel?.GeometricStandardDeviation ?? double.NaN;

                            var combinedAssessmentFactor = (1D / interSpeciesFactor)
                                    * (1D / intraSpeciesGeometricMean)
                                    * kineticConversionFactor
                                    * expressionTypeConversionFactor
                                    * (1D / additionalAssessmentFactor);
                            var hazardDose = new HazardCharacterisationModel() {
                                Code = doseResponseModel.IdDoseResponseModel,
                                Effect = representation.Effect,
                                Substance = benchmarkDose.Substance,
                                Target = targetUnit.Target,
                                PotencyOrigin = PotencyOrigin.Bmd,
                                Value = alignedTestSystemHazardDose * combinedAssessmentFactor,
                                DoseUnit = targetUnit.ExposureUnit,
                                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                                CombinedAssessmentFactor = combinedAssessmentFactor,
                                GeometricStandardDeviation = intraSpeciesGeometricStandardDeviation,
                                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                                    HazardDose = specifiedBenchMarkDose,
                                    DoseUnit = doseResponseModel.DoseUnit,
                                    TargetUnitAlignmentFactor = targetUnitAlignmentFactor,
                                    Effect = representation.Effect,
                                    Species = response.TestSystem.Species,
                                    Organ = response.TestSystem.Organ,
                                    ExposureRoute = response.TestSystem.ExposureRoute,
                                    ExpressionTypeConversionFactor = expressionTypeConversionFactor,
                                    InterSystemConversionFactor = 1D / interSpeciesFactor,
                                    IntraSystemConversionFactor = 1D / intraSpeciesGeometricMean,
                                    AdditionalConversionFactor = 1D / additionalAssessmentFactor,
                                    KineticConversionFactor = kineticConversionFactor,
                                    DoseResponseRelation = new DoseResponseRelation() {
                                        DoseResponseModelEquation = doseResponseModel.ModelEquation,
                                        DoseResponseModelParameterValues = benchmarkDose.ModelParameterValues,
                                        CriticalEffectSize = doseResponseModel.CriticalEffectSize,
                                    },
                                },
                            };
                            result.Add(hazardDose);
                        }
                    } else {
                        // TODO: benchmark response calibration
                    }
                }
            }
            return result;
        }
    }
}
