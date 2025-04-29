using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.PbkModelParameterDistributionModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.PbkModels {

    [ActionType(ActionType.PbkModels)]
    public sealed class PbkModelsActionCalculator : ActionCalculatorBase<IPbkModelsActionResult> {
        private PbkModelsModuleConfig ModuleConfig => (PbkModelsModuleConfig)_moduleSettings;

        public PbkModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            ModuleConfig.NumberOfDosesPerDayNonDietaryDermal = Math.Max(1, ModuleConfig.NumberOfDosesPerDayNonDietaryDermal);
            ModuleConfig.NumberOfDosesPerDayNonDietaryInhalation = Math.Max(1, ModuleConfig.NumberOfDosesPerDayNonDietaryInhalation);
            ModuleConfig.NumberOfDosesPerDayNonDietaryOral = Math.Max(1, ModuleConfig.NumberOfDosesPerDayNonDietaryOral);
            var showActiveSubstances = GetRawDataSources().Any()
                && ModuleConfig.MultipleSubstances
                && !ModuleConfig.FilterByAvailableHazardCharacterisation;

            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.KineticModelInstances][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.KineticModelInstances][ScopingType.KineticModelDefinitions].AlertTypeMissingData = AlertType.Notification;
            _actionDataSelectionRequirements[ScopingType.KineticModelInstances].AllowEmptyScope = true;
            _actionInputRequirements[ActionType.PbkModelDefinitions].IsVisible = true;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResamplePbkModelParameters) {
                result.Add(UncertaintySource.PbkModelParameters);
            }
            return result;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            var modelCodes = linkManager.GetCodesInScope(ScopingType.KineticModelInstances);
            return modelCodes.Any();
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new PbkModelsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var substances = data.ActiveSubstances ?? data.AllCompounds;
            var instances = subsetManager.AllPbkModels
                .Where(r => substances.Contains(r.Substances.First()))
                .ToList();
            data.KineticModelInstances = instances;
            if (data.KineticModelInstances != null && data.KineticModelInstances.Any()) {
                var modelSettings = ModuleConfig;
                foreach (var model in data.KineticModelInstances) {
                    model.PbkModelDefinition = data.AllPbkModelDefinitions?
                        .Where(c => c.IdModelDefinition.Equals(model.IdModelDefinition, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();
                }
            }
            localProgress.Update(100);
        }

        protected override void loadDefaultData(ActionData data) {
            data.KineticModelInstances = [];
        }

        protected override void summarizeActionResult(
            IPbkModelsActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new PbkModelsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.KineticModelInstances != null && factorialSet.Contains(UncertaintySource.PbkModelParameters)) {
                localProgress.Update("Resampling PBK model parameters.");
                var resampledModelInstances = resampleKineticModelParameters(
                    data.KineticModelInstances,
                    uncertaintySourceGenerators[UncertaintySource.PbkModelParameters]
                );
                data.KineticModelInstances = resampledModelInstances;
            }
            localProgress.Update(100);
        }

        /// <summary>
        /// Resampling parameters of kinetic model, uncertainty
        /// </summary>
        private ICollection<KineticModelInstance> resampleKineticModelParameters(
            ICollection<KineticModelInstance> kineticModelInstances,
            IRandom random
        ) {
            var instances = new List<KineticModelInstance>();
            foreach (var kineticModelinstance in kineticModelInstances) {
                var modelParameters = new Dictionary<string, KineticModelInstanceParameter>();
                foreach (var parameter in kineticModelinstance.KineticModelInstanceParameters.Values) {
                    var model = parameter.CvUncertainty.HasValue
                        ? PbkModelParameterDistributionModelFactory.Create(parameter.DistributionType)
                        : new PbkModelParameterDeterministicModel();
                    model.Initialize(parameter.Value, parameter.CvUncertainty);
                    modelParameters[parameter.Parameter] = parameter.Clone(model.Sample(random));
                }
                var clone = kineticModelinstance.Clone();
                clone.KineticModelInstanceParameters = modelParameters;
                instances.Add(clone);
            }
            return instances;
        }
    }
}
