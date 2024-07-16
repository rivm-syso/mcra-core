using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationFromDoseResponseModelCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardCharacterisationsFromPoDCalculation;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.HazardDoseTypeConversion;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation.KineticConversionFactorCalculation;
using MCRA.Simulation.Calculators.InterSpeciesConversion;
using MCRA.Simulation.Calculators.IntraSpeciesConversion;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {

    public sealed class HazardCharacterisationsCalculator {

        private readonly IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> _interSpeciesFactorModels;
        private readonly KineticConversionFactorCalculator _kineticConversionFactorCalculator;
        private readonly IDictionary<(Effect, Compound), IntraSpeciesFactorModel> _intraSpeciesFactorModelCollection;
        private readonly double _additionalAssessmentFactor;
        private readonly double _nominalBodyWeight;

        public HazardCharacterisationsCalculator(
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesFactorModelCollection,
            double additionalAssessmentFactor,
            KineticConversionFactorCalculator kineticConversionFactorCalculator,
            double nominalBodyWeight
        ) {
            _interSpeciesFactorModels = interSpeciesFactorModels;
            _kineticConversionFactorCalculator = kineticConversionFactorCalculator;
            _intraSpeciesFactorModelCollection = intraSpeciesFactorModelCollection;
            _additionalAssessmentFactor = additionalAssessmentFactor;
            _nominalBodyWeight = nominalBodyWeight;
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
                        && IsSourceForTargetMatrix(pointOfDeparture.TargetUnit.Target, targetUnit.Target, convertToSingleMatrix)) {
                        var hazardCharacterisationsFromPoDCalculator = new HazardCharacterisationsFromPoDCalculator();
                        var model = hazardCharacterisationsFromPoDCalculator.Compute(
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
                var focalSubstances = targetDosesCalculationMethod == TargetDosesCalculationMethod.CombineInVivoPodInVitroDrms ? new List<Compound>() { referenceCompound } : substances;

                var candidateDoseResponseModels = doseResponseModels
                    .Where(r => representativeResponses?.Contains(r.Response) ?? false)
                    .Where(r => IsDirectSourceForHazardCharacterisation(targetDosesCalculationMethod, r.Response.TestSystem, r.Substances.Contains(referenceCompound))
                                && IsSourceForTargetMatrix(TargetUnit.FromInternalDoseUnit(r.DoseUnit).Target, targetUnit.Target, convertToSingleMatrix))
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

        public List<(AggregateIndividualExposure, IHazardCharacterisationModel)> ComputeTargetDosesTimeCourses(
            ICollection<IHazardCharacterisationModel> hazardCharacterisationModels,
            ExposureType exposureType,
            KineticModelCalculatorFactory kineticModelCalculatorFactory,
            TargetUnit targetDoseUnit,
            IRandom kineticModelRandomGenerator
        ) {
            var result = new List<(AggregateIndividualExposure, IHazardCharacterisationModel)>();
            foreach (var model in hazardCharacterisationModels) {
                var kineticModelCalculator = kineticModelCalculatorFactory
                    .CreateHumanKineticModelCalculator(model.Substance);
                if (kineticModelCalculator == null) {
                    continue;
                }
                // Use the target dose, without intra-species factor and kinetic conversion factor
                // No correction for inter-species factor because plots reflect the human kinetics
                if (model.TestSystemHazardCharacterisation?.Organ != null) {
                    var externalDose = model.Value / model.TestSystemHazardCharacterisation.IntraSystemConversionFactor;
                    var individual = new Individual(0) {
                        BodyWeight = _nominalBodyWeight,
                    };
                    var internalDoseUnit = TargetUnit.FromInternalDoseUnit(
                        model.TestSystemHazardCharacterisation.DoseUnit,
                        BiologicalMatrixConverter.FromString(model.TestSystemHazardCharacterisation.Organ)
                    );

                    var exposure = ExternalIndividualDayExposure
                        .FromSingleDose(
                            targetDoseUnit.ExposureRoute.GetExposurePath(),
                            model.Substance,
                            externalDose,
                            targetDoseUnit.ExposureUnit,
                            individual
                        );

                    var substanceTargetExposure = kineticModelCalculator
                        .Forward(
                            exposure,
                            targetDoseUnit.ExposureRoute.GetExposurePath(),
                            targetDoseUnit.ExposureUnit,
                            internalDoseUnit,
                            exposureType,
                            kineticModelRandomGenerator
                        );
                    var aggregateIndividualExposure = new AggregateIndividualExposure() {
                        Individual = individual,
                        IndividualSamplingWeight = 1D,
                        InternalTargetExposures = new Dictionary<ExposureTarget, Dictionary<Compound, ISubstanceTargetExposure>>() {
                        {
                            targetDoseUnit.Target,
                            new Dictionary<Compound, ISubstanceTargetExposure>() {
                                { model.Substance, substanceTargetExposure }
                            }
                        }
                    },
                        ExternalIndividualDayExposures = [exposure]
                    };
                    result.Add((aggregateIndividualExposure, model));
                }
            }
            return result;
        }

        private bool IsDirectSourceForHazardCharacterisation(TargetDosesCalculationMethod targetDosesCalculationMethod, TestSystem testSystem, bool hasReference) {
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

        private bool IsSourceForTargetMatrix(ExposureTarget exposureTargetFrom, ExposureTarget exposureTargetTo, bool convertToSingleMatrix) {
            return exposureTargetFrom == exposureTargetTo || convertToSingleMatrix;
        }
    }
}
