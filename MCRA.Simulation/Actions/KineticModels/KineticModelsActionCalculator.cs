using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.Sbml;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels;
using MCRA.Simulation.Calculators.KineticModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.SBML;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.KineticModels {

    [ActionType(ActionType.KineticModels)]
    public sealed class KineticModelsActionCalculator : ActionCalculatorBase<IKineticModelsActionResult> {
        private KineticModelsModuleConfig ModuleConfig => (KineticModelsModuleConfig)_moduleSettings;

        public KineticModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            ModuleConfig.NumberOfDosesPerDay = Math.Max(1, ModuleConfig.NumberOfDosesPerDay);
            ModuleConfig.NumberOfDosesPerDayNonDietaryDermal = Math.Max(1, ModuleConfig.NumberOfDosesPerDayNonDietaryDermal);
            ModuleConfig.NumberOfDosesPerDayNonDietaryInhalation = Math.Max(1, ModuleConfig.NumberOfDosesPerDayNonDietaryInhalation);
            ModuleConfig.NumberOfDosesPerDayNonDietaryOral = Math.Max(1, ModuleConfig.NumberOfDosesPerDayNonDietaryOral);
            var showActiveSubstances = GetRawDataSources().Any()
                && ModuleConfig.MultipleSubstances
                && !ModuleConfig.FilterByAvailableHazardCharacterisation;
            _actionInputRequirements[ActionType.ActiveSubstances].IsRequired = showActiveSubstances;
            _actionInputRequirements[ActionType.ActiveSubstances].IsVisible = showActiveSubstances;
            _actionDataLinkRequirements[ScopingType.KineticAbsorptionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.KineticModelInstances][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.KineticModelInstances][ScopingType.KineticModelDefinitions].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.KineticConversionFactors][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataSelectionRequirements[ScopingType.KineticModelInstances].AllowEmptyScope = true;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleKineticModelParameters) {
                result.Add(UncertaintySource.KineticModelParameters);
            }
            return result;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (ModuleConfig.InternalModelType == InternalModelType.PBKModel) {
                var modelCodes = linkManager.GetCodesInScope(ScopingType.KineticModelInstances);
                return modelCodes.Any();
            }
            return true;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new KineticModelsSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var isAggregate = ModuleConfig.Aggregate;
            if (ModuleConfig.InternalModelType == InternalModelType.PBKModel) {
                var instances = subsetManager.AllKineticModels
                    .Where(r => substances.Contains(r.Substances.First()))
                    .ToList();
                data.KineticModelInstances = instances;
            }

            if (data.KineticModelInstances != null && data.KineticModelInstances.Any()) {
                var modelSettings = ModuleConfig;
                foreach (var model in data.KineticModelInstances) {
                    // TODO: the code below actually modifies compiled data objects
                    // this is not something that we want. Instead, we should probably
                    // create some wrapper class, and use that instead of the compiled
                    // object.
                    model.NumberOfDays = modelSettings.NumberOfDays;
                    model.NumberOfDosesPerDay = modelSettings.NumberOfDosesPerDay;
                    if (isAggregate) {
                        model.NumberOfDosesPerDayNonDietaryDermal = modelSettings.NumberOfDosesPerDayNonDietaryDermal;
                        model.NumberOfDosesPerDayNonDietaryInhalation = modelSettings.NumberOfDosesPerDayNonDietaryInhalation;
                        model.NumberOfDosesPerDayNonDietaryOral = modelSettings.NumberOfDosesPerDayNonDietaryOral;
                    }
                    model.NonStationaryPeriod = modelSettings.NonStationaryPeriod;
                    model.UseParameterVariability = modelSettings.UseParameterVariability;
                    model.SpecifyEvents = modelSettings.SpecifyEvents;
                    model.SelectedEvents = modelSettings.SelectedEvents.ToArray();
                }
            }

            var substanceSpecificAbsorptionFactors = subsetManager.AllKineticAbsorptionFactors?
                .Where(r => substances.Contains(r.Compound))
                .ToList();

            var absorptionFactorSettings = new AbsorptionFactorsCollectionBuilderSettings(ModuleConfig);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(absorptionFactorSettings);
            data.AbsorptionFactors = absorptionFactorsCollectionBuilder.Create(
                substances,
                substanceSpecificAbsorptionFactors
            );
            data.KineticAbsorptionFactors = subsetManager.AllKineticAbsorptionFactors;
            data.KineticConversionFactors = subsetManager.AllKineticConversionFactors;
            data.KineticConversionFactorModels = data.KineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory
                    .Create(c, ModuleConfig.KCFSubgroupDependent)
                )
                .ToList();

            localProgress.Update(100);
        }

        protected override void loadDefaultData(ActionData data) {
            var settings = new AbsorptionFactorsCollectionBuilderSettings(ModuleConfig);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(settings);
            data.AbsorptionFactors = absorptionFactorsCollectionBuilder.Create(
                data.ActiveSubstances
            );
            data.KineticModelInstances = new List<KineticModelInstance>();
        }

        protected override void summarizeActionResult(IKineticModelsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
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
            if (data.KineticModelInstances != null && factorialSet.Contains(UncertaintySource.KineticModelParameters)) {
                localProgress.Update("Resampling kinetic model parameters.");
                var resampledModelInstances = resampleKineticModelParameters(data.KineticModelInstances, uncertaintySourceGenerators[UncertaintySource.KineticModelParameters]);
                data.KineticModelInstances = resampledModelInstances;
            }

            if (data.KineticConversionFactors != null && factorialSet.Contains(UncertaintySource.KineticModelParameters)) {
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

        /// <summary>
        /// Resampling parameters of kinetic model, uncertainty
        /// </summary>
        /// <param name="random"></param>
        private ICollection<KineticModelInstance> resampleKineticModelParameters(ICollection<KineticModelInstance> kineticModelInstances, IRandom random) {
            var instances = new List<KineticModelInstance>();
            foreach (var kineticModelinstance in kineticModelInstances) {
                var modelParameters = new Dictionary<string, KineticModelInstanceParameter>();
                foreach (var parameter in kineticModelinstance.KineticModelInstanceParameters.Values) {
                    var model = ProbabilityDistributionFactory.createProbabilityDistributionModel(parameter.DistributionType);
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
