using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationFromDoseResponseModelCalculation {
    class HazardCharacterisationFromDoseResponseModelCalculator {

        public List<IHazardCharacterisationModel> Compute(
            ICollection<Compound> substances,
            PointOfDepartureType targetPodType,
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
                            var specifiedBenchMarkDose = useBMDL ? benchmarkDose.BenchmarkDoseLower : benchmarkDose.BenchmarkDose;
                            var targetUnitAlignmentFactor = doseResponseModel.DoseUnit
                                .GetDoseAlignmentFactor(targetUnit.ExposureUnit, benchmarkDose.Substance.MolecularMass);
                            var alignedTestSystemHazardDose = targetUnitAlignmentFactor * specifiedBenchMarkDose;

                            var expressionTypeConversionFactor = targetPodType.GetExpressionTypeConversionFactor(PointOfDepartureType.Bmd);
                            var interSpeciesFactor = InterSpeciesFactorModelsBuilder
                                .GetInterSpeciesFactor(interSpeciesFactorModels, representation.Effect, response.TestSystem.Species, benchmarkDose.Substance);

                            var testSystemHazardDoseUnit = new TargetUnit(
                                response.TestSystem.GetTarget(),
                                ExposureUnitTriple.FromDoseUnit(doseResponseModel.DoseUnit)
                            );

                            var kineticConversionFactor = kineticConversionFactorCalculator
                                .ComputeKineticConversionFactor(
                                    specifiedBenchMarkDose * (1D / interSpeciesFactor) * expressionTypeConversionFactor,
                                    testSystemHazardDoseUnit,
                                    benchmarkDose.Substance,
                                    exposureType,
                                    targetUnit,
                                    kineticModelRandomGenerator
                                ) / targetUnitAlignmentFactor;
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
                                TargetUnit = targetUnit,
                                PotencyOrigin = PotencyOrigin.Bmd,
                                Value = alignedTestSystemHazardDose * combinedAssessmentFactor,
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
                        var msg = $"No benchmark response for response {representation.Response.Name} ({representation.Response.Code})" +
                            $"and effect {representation.Effect.Name} ({representation.Effect.Code})";
                        throw new NotImplementedException(msg);
                    }
                }
            }
            return result;
        }
    }
}
