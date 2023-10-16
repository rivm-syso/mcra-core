using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.Calculators.KineticModelCalculation.ParameterDistributionModels;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.KineticModels {

    [ActionType(ActionType.KineticModels)]
    public sealed class KineticModelsActionCalculator : ActionCalculatorBase<IKineticModelsActionResult> {
        public KineticModelsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _project.KineticModelSettings.NumberOfDosesPerDay = Math.Max(1, _project.KineticModelSettings.NumberOfDosesPerDay);
            _project.KineticModelSettings.NumberOfDosesPerDayNonDietaryDermal = Math.Max(1, _project.KineticModelSettings.NumberOfDosesPerDayNonDietaryDermal);
            _project.KineticModelSettings.NumberOfDosesPerDayNonDietaryInhalation = Math.Max(1, _project.KineticModelSettings.NumberOfDosesPerDayNonDietaryInhalation);
            _project.KineticModelSettings.NumberOfDosesPerDayNonDietaryOral = Math.Max(1, _project.KineticModelSettings.NumberOfDosesPerDayNonDietaryOral);
            var showActiveSubstances = GetRawDataSources().Any()
                && _project.AssessmentSettings.MultipleSubstances
                && !_project.EffectSettings.RestrictToAvailableHazardCharacterisations;
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
            if (_project.UncertaintyAnalysisSettings.ResampleKineticModelParameters) {
                result.Add(UncertaintySource.KineticModelParameters);
            }
            return result;
        }

        public override bool CheckDataDependentSettings(ICompiledLinkManager linkManager) {
            if (_project.KineticModelSettings.InternalModelType == InternalModelType.PBKModel
                && string.IsNullOrEmpty(_project.KineticModelSettings.CodeModel)) {
                return false;
            }
            if (_project.KineticModelSettings.InternalModelType == InternalModelType.PBKModel) {
                var modelCodes = linkManager.GetCodesInScope(ScopingType.KineticModelInstances);
                return modelCodes.Any();
            }
            return true;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new KineticModelsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);

            var substances = data.ActiveSubstances ?? data.AllCompounds;

            var isAggregate = _project.AssessmentSettings.Aggregate;
            if (_project.KineticModelSettings.InternalModelType == InternalModelType.PBKModel) {
                data.KineticModelInstances = subsetManager.AllKineticModels
                    .Where(r => r.IdModelDefinition == _project.KineticModelSettings.CodeModel)
                    .Where(r => substances.Contains(r.Substances.First()))
                    .ToList();

                var modelSettings = _project.KineticModelSettings;
                foreach (var model in data.KineticModelInstances) {
                    // TODO: the code below actually modifies compiled data objects
                    // this is not something that we want. Instead, we should probably
                    // create some wrapper class, and use that instead of the compiled
                    // object.
                    model.CodeCompartment = modelSettings.CodeCompartment;
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
                    model.SelectedEvents = modelSettings.SelectedEvents;
                }
            }

            var substanceSpecificAbsorptionFactors = subsetManager.AllKineticAbsorptionFactors?
                .Where(r => substances.Contains(r.Compound))
                .ToList();

            var absorptionFactorSettings = new AbsorptionFactorsCollectionBuilderSettings(_project.NonDietarySettings);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(absorptionFactorSettings);
            data.AbsorptionFactors = absorptionFactorsCollectionBuilder.Create(
                substances,
                substanceSpecificAbsorptionFactors
            );
            data.KineticAbsorptionFactors = subsetManager.AllKineticAbsorptionFactors;

            data.KineticConversionFactors = subsetManager.AllKineticConversionFactors;

            localProgress.Update(100);
        }

        protected override void loadDefaultData(ActionData data) {
            var settings = new AbsorptionFactorsCollectionBuilderSettings(_project.NonDietarySettings);
            var absorptionFactorsCollectionBuilder = new AbsorptionFactorsCollectionBuilder(settings);
            data.AbsorptionFactors = absorptionFactorsCollectionBuilder.Create(
                data.ActiveSubstances
            );
            data.KineticModelInstances = new List<KineticModelInstance>();
        }

        protected override void summarizeActionResult(IKineticModelsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new KineticModelsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
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
                var resampledModelInstances = resampleKineticModelParameters(data.KineticModelInstances, uncertaintySourceGenerators[UncertaintySource.KineticModelParameters]);
                data.KineticModelInstances = resampledModelInstances;
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
