using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromIviveCalculation {

    /// <summary>
    /// Calculator for deriving hazard characterisations using IVIVE.
    /// </summary>
    public sealed class HazardCharacterisationsFromIviveCalculator {

        /// <summary>
        /// Computes hazard characterisations using IVIVE.
        /// </summary>
        public ICollection<IviveHazardCharacterisation> Compute(
            Effect effect,
            ICollection<DoseResponseModel> doseResponseModels,
            ICollection<Compound> substances,
            IHazardCharacterisationModel referenceRecord,
            ICollection<EffectRepresentation> effectRepresentations,
            TargetUnit targetUnit,
            ExposureType exposureType,
            HazardDoseConverter hazardDoseTypeConverter,
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IKineticConversionFactorCalculator kineticConversionFactorCalculator,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesVariabilityModels,
            double additionalAssessmentFactor, 
            IRandom kineticModelRandomGenerator
        ) {
            var representativeResponses = effectRepresentations?.ToLookup(r => r.Response);
            var candidateDoseResponseModels = doseResponseModels?
                .Where(r => representativeResponses?.Contains(r.Response) ?? false)
                .Where(r => r.Response.TestSystem.TestSystemType != TestSystemType.InVivo)
                .GroupBy(r => r.IdExperiment)
                .Select(g => g.MaxBy(r => r.LogLikelihood))
                .ToList();

            if (!candidateDoseResponseModels?.Any() ?? true) {
                throw new Exception("No RPF sets found for IVIVE");
            }

            if (candidateDoseResponseModels.Count > 1) {
                throw new Exception("IVIVE not possible for multiple RPF sets");
            }

            double internalHazardCharacterisationValue;
            var kineticConversionFactor = kineticConversionFactorCalculator.ComputeKineticConversionFactor(
                referenceRecord.Value,
                targetUnit,
                referenceRecord.Substance,
                "Human",
                null,
                referenceRecord.Target.ExposureRoute,
                exposureType,
                TargetLevelType.Internal,
                kineticModelRandomGenerator
            );
            internalHazardCharacterisationValue = kineticConversionFactor * referenceRecord.Value;

            // If have an intra-species factor for the reference substance, we assume that this factor
            // was used for constructing the reference hazard characterisation. If this factor was a
            // conservative/deterministic safety factor, we revert the application of this safety factor
            // for IVIVE so that we can re-apply a possibly different substance specific safety factor
            // again for the imputed substance.
            var referenceIntraSpeciesVariabilityModel = intraSpeciesVariabilityModels.Get(referenceRecord.Effect, referenceRecord.Substance);
            var referenceIntraSpeciesFactor = referenceIntraSpeciesVariabilityModel?.Factor ?? 1D;
            internalHazardCharacterisationValue *= referenceIntraSpeciesFactor;

            // Create hazard characterisations from dose response models
            var result = new List<IviveHazardCharacterisation>();

            foreach (var doseResponseModel in candidateDoseResponseModels) {
                var benchmarkDoses = doseResponseModel.DoseResponseModelBenchmarkDoses.Values
                    .Where(r => substances.Contains(r.Substance))
                    .ToDictionary(r => r.Substance);
                if (benchmarkDoses.TryGetValue(referenceRecord.Substance, out var referenceBmd)) {
                    var doseResponseModelOrgan = doseResponseModel.Response?.TestSystem?.Organ;
                    var doseResponseModelSpecies = doseResponseModel.Response?.TestSystem?.Species;

                    // Conversion to target unit is needed for translating mol-based RPFs to weight-based RPFs
                    var bmdRef = hazardDoseTypeConverter.ConvertToTargetUnit(doseResponseModel.DoseUnit, referenceBmd.Substance, referenceBmd.BenchmarkDose);

                    // TODO: convert BMD to human/target using substance specific inter-species factors
                    var referenceInterSpeciesFactor = InterSpeciesFactorModelsBuilder
                        .GetInterSpeciesFactor(interSpeciesFactorModels, effect, doseResponseModelSpecies, referenceRecord.Substance);

                    foreach (var substance in substances) {
                        if (benchmarkDoses.TryGetValue(substance, out var substanceBmd)) {
                            var molBasedRpf = doseResponseModel.DoseUnit.GetSubstanceAmountUnit().IsInMoles() ? referenceBmd.BenchmarkDose / substanceBmd.BenchmarkDose : double.NaN;
                            var bmdSub = hazardDoseTypeConverter.ConvertToTargetUnit(doseResponseModel.DoseUnit, substanceBmd.Substance, substanceBmd.BenchmarkDose);
                            var rpf = bmdRef / bmdSub;
                            var internalHazardDose = (internalHazardCharacterisationValue * referenceInterSpeciesFactor) / rpf;
                            var substanceInterSpeciesFactor = InterSpeciesFactorModelsBuilder
                                .GetInterSpeciesFactor(interSpeciesFactorModels, effect, doseResponseModelSpecies, substance);

                            var substanceKineticConversionFactor = kineticConversionFactorCalculator.ComputeKineticConversionFactor(
                                internalHazardDose * (1D / substanceInterSpeciesFactor),
                                targetUnit,
                                substance,
                                "Human",
                                doseResponseModelOrgan,
                                ExposureRouteType.AtTarget,
                                exposureType,
                                kineticModelRandomGenerator);

                            var intraSpeciesVariabilityModel = intraSpeciesVariabilityModels.Get(effect, substance);
                            var intraSpeciesGeometricMean = intraSpeciesVariabilityModel?.Factor ?? 1D;
                            var intraSpeciesGeometricStandardDeviation = intraSpeciesVariabilityModel?.GeometricStandardDeviation ?? double.NaN;
                            var hazardDose = new IviveHazardCharacterisation() {
                                Code = $"HC-IVIVE-{substance.Code}",
                                Effect = effect,
                                Substance = substance,
                                Target = referenceRecord.Target,
                                Value = internalHazardDose
                                    * (1D / substanceInterSpeciesFactor)
                                    * substanceKineticConversionFactor
                                    * (1D / intraSpeciesGeometricMean)
                                    * (1D / additionalAssessmentFactor),
                                HazardCharacterisationType = HazardCharacterisationType.Unspecified,
                                PotencyOrigin = PotencyOrigin.Ivive,
                                InternalHazardDose = internalHazardDose,
                                MolBasedRpf = molBasedRpf,
                                KineticConversionFactor = substanceKineticConversionFactor,
                                NominalInterSpeciesConversionFactor = (1D / substanceInterSpeciesFactor),
                                NominalIntraSpeciesConversionFactor = (1D / intraSpeciesGeometricMean),
                                AdditionalConversionFactor = (1D / additionalAssessmentFactor),
                                GeometricStandardDeviation = intraSpeciesGeometricStandardDeviation,
                                DoseUnit = targetUnit.ExposureUnit,
                            };
                            result.Add(hazardDose);
                        }
                    }
                } else {
                    throw new Exception("Index substance for IVIVE not part of RPF sets from dose response models");
                }

            }
            return result;
        }
    }
}
