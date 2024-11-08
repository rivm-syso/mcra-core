using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationFromDoseResponseModelCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromPoDCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {

    public sealed class HazardCharacterisationsCalculator {

        private readonly IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> _interSpeciesFactorModels;
        private readonly KineticConversionFactorCalculator _kineticConversionFactorCalculator;
        private readonly IDictionary<(Effect, Compound), IntraSpeciesFactorModel> _intraSpeciesFactorModelCollection;
        private readonly double _additionalAssessmentFactor;

        public HazardCharacterisationsCalculator(
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesFactorModelCollection,
            double additionalAssessmentFactor,
            KineticConversionFactorCalculator kineticConversionFactorCalculator
        ) {
            _interSpeciesFactorModels = interSpeciesFactorModels;
            _kineticConversionFactorCalculator = kineticConversionFactorCalculator;
            _intraSpeciesFactorModelCollection = intraSpeciesFactorModelCollection;
            _additionalAssessmentFactor = additionalAssessmentFactor;
        }

        /// <summary>
        /// Compute hazardCharacterisationModel based on HazardDoses or DoseResponseModels
        /// </summary>
        public List<IHazardCharacterisationModel> CollectAvailableHazardCharacterisations(
            ICollection<Compound> substances,
            Compound referenceCompound,
            ICollection<Data.Compiled.Objects.PointOfDeparture> pointsOfDeparture,
            ICollection<DoseResponseModel> doseResponseModels,
            ICollection<EffectRepresentation> effectRepresentations,
            ExposureType exposureType,
            TargetDosesCalculationMethod targetDosesCalculationMethod,
            bool convertToSingleMatrix,
            HazardDoseConverter hazardDoseConverter,
            TargetUnit targetUnit,
            bool useBMDL,
            IRandom kineticModelRandomGenerator
        ) {
            var availableHazardCharacterisations = new List<IHazardCharacterisationModel>();

            // Create hazard characterisations from points of departure
            if (pointsOfDeparture != null) {
                foreach (var pointOfDeparture in pointsOfDeparture) {
                    if (IsDirectSourceForHazardCharacterisation(targetDosesCalculationMethod, null, pointOfDeparture.Compound == referenceCompound)
                        && IsSourceForTargetMatrix(pointOfDeparture.TargetUnit.Target, targetUnit.Target, convertToSingleMatrix)
                    ) {
                        var hazardCharacterisationsFromPoDCalculator = new HazardCharacterisationsFromPoDCalculator();
                        var model = hazardCharacterisationsFromPoDCalculator
                            .Compute(
                                pointOfDeparture,
                                hazardDoseConverter,
                                targetUnit,
                                exposureType,
                                _intraSpeciesFactorModelCollection,
                                _kineticConversionFactorCalculator,
                                _interSpeciesFactorModels,
                                _additionalAssessmentFactor,
                                kineticModelRandomGenerator
                            );
                        availableHazardCharacterisations.Add(model);
                    }
                }
            }

            // Create hazard characterisations from dose response models
            if (doseResponseModels != null && doseResponseModels.Any()) {
                var extractedPointsOfDeparture = new List<IHazardCharacterisationModel>();
                var representativeResponses = effectRepresentations?.ToLookup(r => r.Response);
                var focalSubstances = targetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms ? [referenceCompound] : substances;

                var candidateDoseResponseModels = doseResponseModels
                    .Where(r => representativeResponses?.Contains(r.Response) ?? false)
                    .Where(r => IsDirectSourceForHazardCharacterisation(targetDosesCalculationMethod, r.Response.TestSystem, r.Substances.Contains(referenceCompound))
                        && IsSourceForTargetMatrix(
                            r.Response.TestSystem.GetTarget(),
                            targetUnit.Target,
                            convertToSingleMatrix)
                        )
                    .GroupBy(r => r.IdExperiment)
                    .Select(g => g.MaxBy(r => r.LogLikelihood))
                    .ToList();
                var calculator = new HazardCharacterisationFromDoseResponseModelCalculator();
                foreach (var doseResponseModel in candidateDoseResponseModels) {
                    extractedPointsOfDeparture.AddRange(
                        calculator.Compute(
                            focalSubstances,
                            hazardDoseConverter,
                            targetUnit,
                            exposureType,
                            representativeResponses,
                            doseResponseModel,
                            _intraSpeciesFactorModelCollection,
                            _kineticConversionFactorCalculator,
                            _interSpeciesFactorModels,
                            _additionalAssessmentFactor,
                            useBMDL,
                            kineticModelRandomGenerator
                        ));
                }
                availableHazardCharacterisations.AddRange(extractedPointsOfDeparture);
            }

            return availableHazardCharacterisations;
        }

        private bool IsDirectSourceForHazardCharacterisation(
            TargetDosesCalculationMethod targetDosesCalculationMethod,
            TestSystem testSystem,
            bool hasReference
        ) {
            switch (targetDosesCalculationMethod) {
                case TargetDosesCalculationMethod.InVivoPods:
                    return (testSystem?.TestSystemType ?? TestSystemType.InVivo) == TestSystemType.InVivo;
                case TargetDosesCalculationMethod.InVitroBmds:
                    return (testSystem?.TestSystemType ?? TestSystemType.InVivo) != TestSystemType.InVivo;
                case TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms:
                    return hasReference && (testSystem?.TestSystemType ?? TestSystemType.InVivo) == TestSystemType.InVivo;
                default:
                    return false;
            }
        }

        private bool IsSourceForTargetMatrix(
            ExposureTarget exposureTargetFrom,
            ExposureTarget exposureTargetTo,
            bool convertToSingleMatrix
        ) {
            return exposureTargetFrom == exposureTargetTo || convertToSingleMatrix;
        }
    }
}
