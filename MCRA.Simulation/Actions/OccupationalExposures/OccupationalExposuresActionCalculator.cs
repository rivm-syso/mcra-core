using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.UnitDefinitions.Units;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.OccupationalExposureGenerators;
using MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.OccupationalExposures {

    [ActionType(ActionType.OccupationalExposures)]
    public class OccupationalExposuresActionCalculator(ProjectDto project) : ActionCalculatorBase<OccupationalExposuresActionResult>(project) {
        private OccupationalExposuresModuleConfig ModuleConfig => (OccupationalExposuresModuleConfig)_moduleSettings;

        protected override void verify() {
            var requireIndividuals = ModuleConfig.ExposureType == ExposureType.Chronic
                && ModuleConfig.ComputeExternalOccupationalDoses
                && ModuleConfig.OccupationalExposuresCalculationMethod == OccupationalExposuresCalculationMethod.IndividualSet;
            _actionInputRequirements[ActionType.Individuals].IsRequired = requireIndividuals;
            _actionInputRequirements[ActionType.Individuals].IsVisible = requireIndividuals;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new OccupationalExposuresSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override OccupationalExposuresActionResult run(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            return compute(data, progressReport);
        }

        protected override void updateSimulationData(ActionData data, OccupationalExposuresActionResult result) {
            data.OccupationalScenarioExposures = result.OccupationalScenarioExposures;
        }

        private OccupationalExposuresActionResult compute(
            ActionData data,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new OccupationalExposuresActionResult();

            var substances = data.ActiveSubstances ?? data.AllCompounds;
            var targetUnit = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerDay);

            result.OccupationalTaskExposureModels = OccupationalTaskExposureModelsCalculator
                .Compute(
                    substances,
                    data.OccupationalTaskExposures,
                    ModuleConfig.SelectedExposureRoutes,
                    ModuleConfig.OccupationalExposureModelType,
                    ModuleConfig.SelectedPercentage
                );

            if (ModuleConfig.ExposureApproachMethod == ExposureApproachMethod.Modelled) {
                var systemic = ModuleConfig.ComputeExternalOccupationalDoses;
                var seed = RandomUtils.CreateSeed(ModuleConfig.RandomSeed, (int)RandomSource.OCE_DrawOccupationalExposures);

                //Modelled exposures
                result.OccupationalScenarioTaskExposures = OccupationalExposuresCalculator
                    .Compute(
                        substances,
                        data.OccupationalScenarios.Values,
                        result.OccupationalTaskExposureModels,
                        ModuleConfig.SelectedExposureRoutes,
                        ModuleConfig.NominalBodySurfaceArea,
                        ModuleConfig.FractionSkinExposed,
                        ModuleConfig.VentilatoryFlowRate,
                        targetUnit.SubstanceAmountUnit,
                        systemic,
                        seed
                    );

                result.OccupationalScenarioExposures = OccupationalExposuresCalculator
                    .Compute(
                        substances,
                        data.OccupationalScenarios.Values,
                        result.OccupationalTaskExposureModels,
                        ModuleConfig.SelectedExposureRoutes,
                        ModuleConfig.NominalBodySurfaceArea,
                        ModuleConfig.FractionSkinExposed,
                        ModuleConfig.VentilatoryFlowRate,
                        targetUnit,
                        systemic,
                        seed
                    );

                if (ModuleConfig.ComputeExternalOccupationalDoses
                    && ModuleConfig.OccupationalExposuresCalculationMethod == OccupationalExposuresCalculationMethod.IndividualSet
                ) {
                    var referenceIndividualDays = data.Individuals;
                    var generator = new OccupationalExposureGenerator();
                    var seedOccupationalExposuresSampling = RandomUtils.CreateSeed(
                        ModuleConfig.RandomSeed,
                        (int)RandomSource.OE_DrawOccupationalExposures
                    );
                    var individualExposuresCollection = generator
                        .Generate(
                            referenceIndividualDays,
                            data.ActiveSubstances,
                            data.OccupationalScenarios.Values,
                            result.OccupationalScenarioExposures,
                            targetUnit.SubstanceAmountUnit,
                            ModuleConfig.ExposureType,
                            seedOccupationalExposuresSampling
                        );
                    result.ExternalSystemicExposureUnit = new OccupationalExposureUnit(
                        targetUnit.SubstanceAmountUnit,
                        JobTaskExposureUnitDenominator.None,
                        JobTaskExposureEstimateType.Undefined,
                        TimeUnit.Days
                    );
                    result.ExternalIndividualExposureCollection = individualExposuresCollection;
                }
            } else {
                throw new Exception("Calculation of occupational exposures from measured concentrations not implemented yet.");
            }

            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResult(OccupationalExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new OccupationalExposuresSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
