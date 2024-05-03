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
        public override TargetExposuresActionResult Compute(
            ICollection<Compound> activeSubstances,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposures,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            Compound referenceSubstance,
            List<DietaryIndividualIntake> dietaryIndividualUsualIntakes,
            NonDietaryExposureGenerator nonDietaryIntakeCalculator,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators,
            ITargetExposuresCalculator targetExposuresCalculator,
            ICollection<ExposurePathType> exposureRoutes,
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
            var nonDietaryIndividualDayIntakes = nonDietaryIntakeCalculator?.CalculateAcuteNonDietaryIntakes(
                dietaryIndividualDayIntakes.Cast<IIndividualDay>().ToList(),
                activeSubstances,
                nonDietaryExposures.Keys,
                seedNonDietaryExposuresSampling,
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
            var targetIndividualDayExposuresCollection = targetExposuresCalculator
                .ComputeTargetIndividualDayExposures(
                    aggregateIndividualDayExposures.Cast<IExternalIndividualDayExposure>().ToList(),
                    activeSubstances,
                    referenceSubstance,
                    exposureRoutes,
                    externalExposureUnit,
                    kineticModelParametersRandomGenerator,
                    new ProgressState(localProgress.CancellationToken)
                );
            var aggregateIndividualDayExposuresCollection = new List<AggregateIndividualDayExposureCollection>();
            foreach (var collection in targetIndividualDayExposuresCollection) {
                var lookup = collection.TargetIndividualDayExposures.ToDictionary(r => r.SimulatedIndividualDayId);
                var records = aggregateIndividualDayExposures.Select(c => c.Clone()).ToList();
                records.ForEach(c => c.TargetExposuresBySubstance = lookup[c.SimulatedIndividualDayId].TargetExposuresBySubstance);
                var aideCollection = new AggregateIndividualDayExposureCollection() {
                    Compartment = collection.Compartment,
                    AggregateIndividualDayExposures = records
                };
                aggregateIndividualDayExposuresCollection.Add(aideCollection);
            }

            // Compute kinetic conversion factors
            kineticModelParametersRandomGenerator.Reset();
            var kineticConversionFactors = targetExposuresCalculator
                .ComputeKineticConversionFactors(
                    activeSubstances,
                    exposureRoutes,
                    aggregateIndividualDayExposures,
                    externalExposureUnit,
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
                //TODO, remove in the future
                AggregateIndividualDayExposures = aggregateIndividualDayExposuresCollection.First().AggregateIndividualDayExposures,
                AggregateIndividualDayExposureCollection = aggregateIndividualDayExposuresCollection,
                KineticConversionFactors = kineticConversionFactors,
                TargetIndividualDayExposureCollection = targetIndividualDayExposuresCollection,
            };

            localProgress.Update(100);
            return result;
        }
    }
}
