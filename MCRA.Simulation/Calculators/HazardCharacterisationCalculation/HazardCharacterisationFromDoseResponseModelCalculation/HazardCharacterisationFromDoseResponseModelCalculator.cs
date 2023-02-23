using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationFromDoseResponseModelCalculation {
    class HazardCharacterisationFromDoseResponseModelCalculator {

        public List<IHazardCharacterisationModel> Compute(
            ICollection<Compound> substances,
            HazardDoseConverter hazardDoseTypeConverter,
            TargetUnit targetDoseUnit,
            ExposureType exposureType,
            ILookup<Response, EffectRepresentation> representativeResponses,
            DoseResponseModel doseResponseModel,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            KineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            double additionalAssessmentFactor,
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
                            var alignedTestSystemHazardDose = hazardDoseTypeConverter.ConvertToTargetUnit(doseResponseModel.DoseUnit, benchmarkDose.Substance, benchmarkDose.BenchmarkDose);
                            var targetUnitAlignmentFactor = alignedTestSystemHazardDose / benchmarkDose.BenchmarkDose;
                            var expressionTypeConversionFactor = hazardDoseTypeConverter.GetExpressionTypeConversionFactor(PointOfDepartureType.Bmd);
                            var interSpeciesFactor = InterSpeciesFactorModelsBuilder
                                .GetInterSpeciesFactor(interSpeciesFactorModels, representation.Effect, response.TestSystem.Species, benchmarkDose.Substance);
                            var kineticConversionFactor = kineticConversionFactorCalculator.ComputeKineticConversionFactor(
                                alignedTestSystemHazardDose * (1D / interSpeciesFactor) * expressionTypeConversionFactor,
                                targetDoseUnit,
                                benchmarkDose.Substance,
                                response.TestSystem.Species,
                                response.TestSystem.Organ,
                                response.TestSystem.ExposureRouteType,
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
                                ExposureRoute = kineticConversionFactorCalculator.TargetDoseLevel == TargetLevelType.External ? ExposureRouteType.Dietary : ExposureRouteType.AtTarget,
                                PotencyOrigin = PotencyOrigin.Bmd,
                                Value = alignedTestSystemHazardDose * combinedAssessmentFactor,
                                DoseUnit = targetDoseUnit,
                                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                                CombinedAssessmentFactor = combinedAssessmentFactor,
                                GeometricStandardDeviation = intraSpeciesGeometricStandardDeviation,
                                TestSystemHazardCharacterisation = new TestSystemHazardCharacterisation() {
                                    HazardDose = benchmarkDose.BenchmarkDose,
                                    DoseUnit = doseResponseModel.DoseUnit,
                                    TargetUnitAlignmentFactor = targetUnitAlignmentFactor,
                                    Effect = representation.Effect,
                                    Species = response.TestSystem.Species,
                                    Organ = response.TestSystem.Organ,
                                    ExposureRoute = response.TestSystem.ExposureRouteType,
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
