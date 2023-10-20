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

namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {

    public sealed class HazardCharacterisationsCalculator {

        private IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> _interSpeciesFactorModels { get; set; }
        private KineticConversionFactorCalculator _kineticConversionFactorCalculator { get; set; }
        private IDictionary<(Effect, Compound), IntraSpeciesFactorModel> _intraSpeciesFactorModelCollection { get; set; }
        private double _additionalAssessmentFactor { get; set; }
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
            IRandom kineticModelRandomGenerator
        ) {
            var availableHazardCharacterisations = new List<IHazardCharacterisationModel>();

            // Create hazard charactrisations from points of departure
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
                            kineticModelRandomGenerator
                        ));
                }
                availableHazardCharacterisations.AddRange(extractedPointsOfDeparture);
            }

            return availableHazardCharacterisations;
        }

        public List<AggregateIndividualExposure> ComputeTargetDosesTimeCourses(
            ICollection<IHazardCharacterisationModel> hazardCharacterisationModels,
            ExposureType exposureType,
            TargetLevelType targetDoseLevel,
            KineticModelCalculatorFactory kineticModelCalculatorFactory,
            TargetUnit targetDoseUnit,
            IRandom kineticModelRandomGenerator
        ) {
            var result = new List<AggregateIndividualExposure>();
            bool isAtTarget(ExposureRouteType route) => (targetDoseLevel == TargetLevelType.Internal)
                ? route == ExposureRouteType.AtTarget
                : route != ExposureRouteType.AtTarget;
            var drillDownHazardCharacterisations = hazardCharacterisationModels
                .Where(r => r.TestSystemHazardCharacterisation != null && !isAtTarget(r.TestSystemHazardCharacterisation.ExposureRoute))
                .ToList();
            foreach (var model in drillDownHazardCharacterisations) {
                var kineticModelCalculator = kineticModelCalculatorFactory.CreateHumanKineticModelCalculator(model.Substance);
                // Use the target dose, without intra-species factor and kinetic conversion factor
                // No correction for inter-species factor because plots reflect the human kinetics
                var dose = model.Value / model.TestSystemHazardCharacterisation.KineticConversionFactor / model.TestSystemHazardCharacterisation.IntraSystemConversionFactor;
                var route = model.TestSystemHazardCharacterisation.ExposureRoute;
                var relativeCompartmentWeight = kineticModelCalculator.GetNominalRelativeCompartmentWeight();
                if (!double.IsNaN(dose)) {
                    if (route == ExposureRouteType.AtTarget) {
                        route = ExposureRouteType.Dietary;
                        dose = kineticModelCalculator.Reverse(
                            dose,
                            model.Substance,
                            ExposureRouteType.Dietary,
                            exposureType,
                            targetDoseUnit.ExposureUnit,
                            _nominalBodyWeight,
                            relativeCompartmentWeight,
                            kineticModelRandomGenerator
                        );
                    }
                }
                var individual = new Individual(0) {
                    BodyWeight = _nominalBodyWeight,
                };
                var exposure = ExternalIndividualDayExposure
                    .FromSingleDose(
                        route,
                        model.Substance,
                        dose,
                        targetDoseUnit.ExposureUnit,
                        individual
                    );
                var substanceTargetExposure = kineticModelCalculator
                    .CalculateInternalDoseTimeCourse(
                        exposure,
                        model.Substance,
                        route,
                        exposureType,
                        targetDoseUnit.ExposureUnit,
                        relativeCompartmentWeight,
                        kineticModelRandomGenerator
                    );
                var aggregateIndividualExposure = new AggregateIndividualExposure() {
                    TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>() {
                        { model.Substance, substanceTargetExposure }
                    },
                    Individual = individual,
                    IndividualSamplingWeight = 1D,
                    RelativeCompartmentWeight = relativeCompartmentWeight,
                    ExposuresPerRouteSubstance = exposure.ExposuresPerRouteSubstance,
                    ExternalIndividualDayExposures = new List<IExternalIndividualDayExposure>() { exposure }
                };
                result.Add(aggregateIndividualExposure);
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
