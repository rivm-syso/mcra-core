using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.KineticModels {

    [ActionType(ActionType.KineticModels)]
    public sealed class KineticModelsActionCalculator : ActionCalculatorBase<IKineticModelsActionResult> {
        private KineticModelsModuleConfig ModuleConfig => (KineticModelsModuleConfig)_moduleSettings;

        public KineticModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            var showActiveSubstances = GetRawDataSources().Any()
                && ModuleConfig.MultipleSubstances
                && !ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.KineticAbsorptionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.KineticConversionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleKineticModelParameters) {
                result.Add(UncertaintySource.KineticModelParameters);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new KineticModelsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var isAggregate = ModuleConfig.Aggregate;

            var allAbsorptionFactors = subsetManager.AllKineticAbsorptionFactors?.ToList() ?? [];

            var substanceSpecificAbsorptionFactors = allAbsorptionFactors?
                .Where(r => substances.Contains(r.Substance))
                .ToList();

            var absorptionFactorSettings = new AbsorptionFactorsCollectionBuilderSettings(ModuleConfig);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(absorptionFactorSettings);
            var absorptionFactors = absorptionFactorsCollectionBuilder
                .Create(substances, substanceSpecificAbsorptionFactors);

            var kineticConversionFactors = subsetManager.AllKineticConversionFactors?.ToList() ?? [];
            kineticConversionFactors.AddRange(
                absorptionFactors
                    .Select((c, ix) => KineticConversionFactor.FromDefaultAbsorptionFactor(c.ExposureRoute, c.Substance, c.AbsorptionFactor))
                    .ToList()
            );
            data.KineticAbsorptionFactors = absorptionFactors;
            data.KineticConversionFactorModels = kineticConversionFactors
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, ModuleConfig.KCFSubgroupDependent)
                )
                .ToList();

            localProgress.Update(100);
        }

        protected override void loadDefaultData(ActionData data) {
            var settings = new AbsorptionFactorsCollectionBuilderSettings(ModuleConfig);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(settings);
            var absorptionFactors = absorptionFactorsCollectionBuilder
                .Create(data.ActiveSubstances)
                .Select(r => new KineticAbsorptionFactor() {
                    Substance = r.Substance,
                    ExposureRoute = r.ExposureRoute,
                    AbsorptionFactor = r.AbsorptionFactor
                })
                .ToList();

            var kineticConversionFactors = new List<KineticConversionFactor>();
            kineticConversionFactors.AddRange(
                absorptionFactors
                    .Select((c, ix) => KineticConversionFactor.FromDefaultAbsorptionFactor(c.ExposureRoute, c.Substance, c.AbsorptionFactor))
                    .ToList()
            );

            data.KineticAbsorptionFactors = absorptionFactors;
            data.KineticConversionFactorModels = kineticConversionFactors
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, ModuleConfig.KCFSubgroupDependent)
                )
                .ToList();

        }

        protected override void summarizeActionResult(
            IKineticModelsActionResult actionResult, 
            ActionData data, 
            SectionHeader header, 
            int order, 
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new KineticModelsSummarizer(ModuleConfig);
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
            if (data.KineticConversionFactorModels != null && factorialSet.Contains(UncertaintySource.KineticModelParameters)) {
                localProgress.Update("Resampling kinetic conversion factors.");
                if (data.KineticConversionFactorModels?.Any() ?? false) {
                    var random = uncertaintySourceGenerators[UncertaintySource.KineticModelParameters];
                    foreach (var model in data.KineticConversionFactorModels) {
                        model.ResampleModelParameters(random);
                    }
                }
            }
            localProgress.Update(100);
        }
    }
}
