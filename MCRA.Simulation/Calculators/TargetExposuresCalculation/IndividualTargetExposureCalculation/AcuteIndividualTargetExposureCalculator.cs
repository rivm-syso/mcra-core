using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.IndividualTargetExposureCalculation {
    public sealed class AcuteIndividualTargetExposureCalculator : IndividualTargetExposureCalculatorBase {

        public AcuteIndividualTargetExposureCalculator() {
        }

        /// <summary>
        /// Runs the acute simulation.
        /// </summary>
        /// <param name="activeSubstances"></param>
        /// <param name="nonDietaryExposures"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="dietaryIndividualUsualIntakes"></param>
        /// <param name="nonDietaryIntakeCalculator"></param>
        /// <param name="kineticModelCalculators"></param>
        /// <param name="targetExposuresCalculator"></param>
        /// <param name="exposureRoutes"></param>
        /// <param name="externalExposureUnit"></param>
        /// <param name="targetExposureUnit"></param>
        /// <param name="seedNonDietaryExposuresSampling"></param>
        /// <param name="seedKineticModelParameterSampling"></param>
        /// <param name="isFirstModelThanAdd"></param>
        /// <param name="kineticModelInstances"></param>
        /// <param name="population"></param>
        /// <param name="progressReport"></param>
        /// <returns></returns>
        public override TargetExposuresActionResult Compute(
            ICollection<Compound> activeSubstances,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposures,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            Compound referenceSubstance,
            List<DietaryIndividualIntake> dietaryIndividualUsualIntakes,
            NonDietaryExposureGenerator nonDietaryIntakeCalculator,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ITargetExposuresCalculator targetExposuresCalculator,
            ICollection<ExposureRouteType> exposureRoutes,
            TargetUnit externalExposureUnit,
            TargetUnit targetExposureUnit,
            int seedNonDietaryExposuresSampling,
            int seedKineticModelParameterSampling,
            bool isFirstModelThanAdd,
            ICollection<KineticModelInstance> kineticModelInstances,
            Population population,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Collect non-dietary exposures
            var relativeCompartmentWeight = (targetExposuresCalculator as InternalTargetExposuresCalculator)?
                .GetRelativeCompartmentWeight(kineticModelCalculators.Values) ?? 1D;
            var nonDietaryIndividualDayIntakes = nonDietaryIntakeCalculator?.CalculateAcuteNonDietaryIntakes(
                dietaryIndividualDayIntakes.Cast<IIndividualDay>().ToList(),
                activeSubstances,
                nonDietaryExposures.Keys,
                seedNonDietaryExposuresSampling,
                relativeCompartmentWeight,
                progressReport.CancellationToken
            );

            // Create aggregate individual day exposures
            var aggregateIndividualDayExposures = AggregateIntakeCalculator.CreateAggregateIndividualDayExposures(
                dietaryIndividualDayIntakes,
                nonDietaryIndividualDayIntakes,
                exposureRoutes
            );

            // Compute target exposures
            var kineticModelParametersRandomGenerator = new McraRandomGenerator(seedKineticModelParameterSampling);
            var targetIndividualDayExposures = targetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    aggregateIndividualDayExposures.Cast<IExternalIndividualDayExposure>().ToList(),
                    activeSubstances,
                    referenceSubstance,
                    exposureRoutes,
                    targetExposureUnit,
                    kineticModelParametersRandomGenerator,
                    kineticModelInstances,
                    new ProgressState(localProgress.CancellationToken)
                )
                .ToDictionary(r => r.SimulatedIndividualDayId);
            foreach (var record in aggregateIndividualDayExposures) {
                record.TargetExposuresBySubstance = targetIndividualDayExposures[record.SimulatedIndividualDayId].TargetExposuresBySubstance;
                record.RelativeCompartmentWeight = targetIndividualDayExposures[record.SimulatedIndividualDayId].RelativeCompartmentWeight;
            }

            // Compute kinetic conversion factors
            kineticModelParametersRandomGenerator.Reset();
            var kineticConversionFactors = targetExposuresCalculator
                .ComputeKineticConversionFactors(
                    activeSubstances,
                    exposureRoutes,
                    aggregateIndividualDayExposures,
                    targetExposureUnit,
                    population.NominalBodyWeight,
                    kineticModelParametersRandomGenerator
                );

            // Non-dietary exposures should contain compartment weight
            var result = new TargetExposuresActionResult() {
                ExposureRoutes = exposureRoutes,
                ExternalExposureUnit = externalExposureUnit,
                TargetExposureUnit = targetExposureUnit,
                NonDietaryIndividualDayIntakes = nonDietaryIndividualDayIntakes,
                KineticModelCalculators = kineticModelCalculators,
                AggregateIndividualDayExposures = aggregateIndividualDayExposures,
                KineticConversionFactors = kineticConversionFactors,
            };

            localProgress.Update(100);
            return result;
        }
    }
}
