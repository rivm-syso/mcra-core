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
    public sealed class ChronicIndividualTargetExposureCalculator : IndividualTargetExposureCalculatorBase {

        public ChronicIndividualTargetExposureCalculator() {
        }

        /// <summary>
        /// Runs the chronic simulation.
        /// </summary>
        public override TargetExposuresActionResult Compute(
            ICollection<Compound> activeSubstances,
            IDictionary<NonDietarySurvey, List<NonDietaryExposureSet>> nonDietaryExposures,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            Compound referenceSubstance,
            List<DietaryIndividualIntake> dietaryModelAssistedIntakes,
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
            var relativeCompartmentWeight = ((InternalTargetExposuresCalculator)targetExposuresCalculator)?
                .GetRelativeCompartmentWeight(kineticModelCalculators.Values) ?? null;
            var nonDietaryIndividualDayIntakes = nonDietaryIntakeCalculator?
                .CalculateChronicNonDietaryIntakes(
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

            // Create aggregate individual exposures
            var aggregateIndividualExposures = AggregateIntakeCalculator.CreateAggregateIndividualExposures(
                aggregateIndividualDayExposures
            );

            // Compute target exposures
            var kineticModelParametersRandomGenerator = new McraRandomGenerator(seedKineticModelParameterSampling);
            var targetIndividualExposuresCollection = targetExposuresCalculator.ComputeTargetIndividualExposures(
                    aggregateIndividualExposures.Cast<IExternalIndividualExposure>().ToList(),
                    activeSubstances,
                    referenceSubstance,
                    exposureRoutes,
                    externalExposureUnit,
                    kineticModelParametersRandomGenerator,
                    kineticModelInstances,
                    new ProgressState(localProgress.CancellationToken)
                );

            var aggregateIndividualExposuresCollection = new List<AggregateIndividualExposureCollection>();
            foreach (var collection in targetIndividualExposuresCollection) {
                var lookup = collection.TargetIndividualExposures.ToDictionary(r => r.SimulatedIndividualId);
                var records = aggregateIndividualExposures.Select(c => c.Clone()).ToList();
                records.ForEach(c => c.TargetExposuresBySubstance = lookup[c.SimulatedIndividualId].TargetExposuresBySubstance);
                var aieCollection = new AggregateIndividualExposureCollection() {
                    Compartment = collection.Compartment,
                    RelativeCompartmentWeight = collection.RelativeCompartmentWeight,
                    TargetUnit = collection.TargetUnit,
                    AggregateIndividualExposures = records
                };
                aggregateIndividualExposuresCollection.Add(aieCollection);
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
                //TODO, remove in the future
                AggregateIndividualExposures = aggregateIndividualExposuresCollection.First().AggregateIndividualExposures,
                AggregateIndividualExposureCollection = aggregateIndividualExposuresCollection,
                TargetIndividualExposureCollection = targetIndividualExposuresCollection,
            };

            localProgress.Update(100);
            return result;
        }
    }
}
