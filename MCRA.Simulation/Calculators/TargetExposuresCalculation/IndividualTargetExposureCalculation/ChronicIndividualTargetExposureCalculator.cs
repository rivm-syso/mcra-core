using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Actions.TargetExposures;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.IndividualTargetExposureCalculation {
    public sealed class ChronicIndividualTargetExposureCalculator : IndividualTargetExposureCalculatorBase {

        public ChronicIndividualTargetExposureCalculator() {
        }

        /// <summary>
        /// Runs the chronic simulation.
        /// </summary>
        /// <param name="activeSubstances"></param>
        /// <param name="nonDietaryExposures"></param>
        /// <param name="dietaryIndividualDayIntakes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="dietaryModelAssistedIntakes"></param>
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
            List<DietaryIndividualIntake> dietaryModelAssistedIntakes,
            NonDietaryExposureGenerator nonDietaryIntakeCalculator,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ITargetExposuresCalculator targetExposuresCalculator,
            ICollection<ExposureRouteType> exposureRoutes,
            ExposureUnitTriple externalExposureUnit,
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
            var nonDietaryIndividualDayIntakes = nonDietaryIntakeCalculator?
                .CalculateChronicNonDietaryIntakes(
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

            // Create aggregate individual exposures
            var aggregateIndividualExposures = AggregateIntakeCalculator.CreateAggregateIndividualExposures(
                aggregateIndividualDayExposures
            );

            // Compute target exposures
            var kineticModelParametersRandomGenerator = new McraRandomGenerator(seedKineticModelParameterSampling);
            var targetIndividualExposures = targetExposuresCalculator.ComputeTargetIndividualExposures(
                    aggregateIndividualExposures.Cast<IExternalIndividualExposure>().ToList(),
                    activeSubstances,
                    referenceSubstance,
                    exposureRoutes,
                    externalExposureUnit,
                    kineticModelParametersRandomGenerator,
                    kineticModelInstances,
                    new ProgressState(localProgress.CancellationToken)
                )
                .ToDictionary(r => r.SimulatedIndividualId);

            foreach (var record in aggregateIndividualExposures) {
                record.TargetExposuresBySubstance = targetIndividualExposures[record.SimulatedIndividualId].TargetExposuresBySubstance;
                record.RelativeCompartmentWeight = targetIndividualExposures[record.SimulatedIndividualId].RelativeCompartmentWeight;
            }

            // Compute kinetic conversion factors
            kineticModelParametersRandomGenerator.Reset();
            var kineticConversionFactors = targetExposuresCalculator.ComputeKineticConversionFactors(
                activeSubstances,
                exposureRoutes,
                aggregateIndividualExposures,
                externalExposureUnit,
                population.NominalBodyWeight,
                kineticModelParametersRandomGenerator
            );

            var result = new TargetExposuresActionResult() {
                ExposureRoutes = exposureRoutes,
                ExternalExposureUnit = externalExposureUnit,
                TargetExposureUnit = targetExposureUnit,
                NonDietaryIndividualDayIntakes = nonDietaryIndividualDayIntakes,
                KineticModelCalculators = kineticModelCalculators,
                KineticConversionFactors = kineticConversionFactors,
                AggregateIndividualExposures = aggregateIndividualExposures
            };

            localProgress.Update(100);
            return result;
        }
    }
}
