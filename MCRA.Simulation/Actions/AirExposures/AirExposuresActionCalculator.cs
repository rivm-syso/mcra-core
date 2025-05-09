﻿using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.AirExposures {

    [ActionType(ActionType.AirExposures)]
    public class AirExposuresActionCalculator(ProjectDto project) : ActionCalculatorBase<AirExposuresActionResult>(project) {
        private AirExposuresModuleConfig ModuleConfig => (AirExposuresModuleConfig)_moduleSettings;

        protected override void verify() {
            var requireDietaryExposures = ModuleConfig.AirExposuresIndividualGenerationMethod == AirExposuresIndividualGenerationMethod.UseDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsRequired = requireDietaryExposures;
            _actionInputRequirements[ActionType.DietaryExposures].IsVisible = requireDietaryExposures;
            _actionInputRequirements[ActionType.Individuals].IsRequired = !requireDietaryExposures;
            _actionInputRequirements[ActionType.Individuals].IsVisible = !requireDietaryExposures;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new AirExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override AirExposuresActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of dust exposure determinants
            var airExposuresDeterminantsRandomGenerator = new McraRandomGenerator(
                RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.AIE_AirExposureDeterminants)
            );

            return compute(
                data,
                localProgress,
                airExposuresDeterminantsRandomGenerator
            );
        }

        protected override void updateSimulationData(ActionData data, AirExposuresActionResult result) {
            data.IndividualAirExposures = result.IndividualAirExposures;
            data.AirExposureUnit = result.AirExposureUnit;
        }

        protected override AirExposuresActionResult runUncertain(
          ActionData data,
          UncertaintyFactorialSet factorialSet,
          Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
          CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            // Random generator for computation of air exposure determinants
            var airExposuresDeterminantsRandomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.AIE_AirExposureDeterminants));

            return compute(
                data,
                localProgress,
                airExposuresDeterminantsRandomGenerator,
                factorialSet,
                uncertaintySourceGenerators
            );
        }

        protected override void updateSimulationDataUncertain(ActionData data, AirExposuresActionResult result) {
            updateSimulationData(data, result);
        }

        private AirExposuresActionResult compute(
            ActionData data,
            ProgressState localProgress,
            IRandom airExposureDeterminantsRandomGenerator,
            UncertaintyFactorialSet factorialSet = null,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators = null
        ) {
            var result = new AirExposuresActionResult();

            ICollection<IIndividualDay> individualDays = null;
            if (ModuleConfig.AirExposuresIndividualGenerationMethod == AirExposuresIndividualGenerationMethod.Simulate) {
                individualDays = data.Individuals;
            } else {
                individualDays = data.DietaryIndividualDayIntakes
                    .GroupBy(r => r.SimulatedIndividual.Id, (key, g) => g.First())
                    .Cast<IIndividualDay>()
                    .ToList();
            }

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var airVentilatoryFlowRates = data.AirVentilatoryFlowRates
                .OrderBy(r => r.AgeLower)
                .ToList();

            // For now, we assume ar exposures to be expressed in
            // - ug/day when output is expressed per person
            // - ug/kg bw/day when output is expressed per kilogram bodyweight
            var targetUnit = ModuleConfig.IsPerPerson
                ? ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerDay)
                : ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay);

            var individualAirExposureRecords = AirExposureCalculator
                .ComputeAirExposure(
                    individualDays,
                    substances,
                    ModuleConfig.SelectedExposureRoutes,
                    data.IndoorAirConcentrations,
                    data.OutdoorAirConcentrations,
                    data.AirIndoorFractions,
                    airVentilatoryFlowRates,
                    data.IndoorAirConcentrationUnit,
                    targetUnit,
                    airExposureDeterminantsRandomGenerator
                );

            result.IndividualAirExposures = individualAirExposureRecords;
            result.AirExposureUnit = targetUnit;

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(AirExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new AirExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, AirExposuresActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new AirExposuresSummarizer(ModuleConfig);
            summarizer.SummarizeUncertain(actionResult, data, header);
            localProgress.Update(100);
        }
    }
}
