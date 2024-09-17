using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.PointsOfDeparture {

    [ActionType(ActionType.PointsOfDeparture)]
    public sealed class PointsOfDepartureActionCalculator : ActionCalculatorBase<IPointsOfDepartureActionResult> {
        private PointsOfDepartureModuleConfig ModuleConfig => (PointsOfDepartureModuleConfig)_moduleSettings;

        public PointsOfDepartureActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = ModuleConfig.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = ModuleConfig.IncludeAopNetworks;
            _actionDataLinkRequirements[ScopingType.PointsOfDeparture][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.PointsOfDeparture][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HazardDosesUncertain][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.HazardDosesUncertain][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = base.GetRandomSources();
            if (ModuleConfig.ResampleRPFs) {
                result.Add(UncertaintySource.PointsOfDeparture);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new PointsOfDepartureSettingsSummarizer(ModuleConfig);
            return summarizer.Summarize(_isCompute, _project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressState
        ) {
            var relevantEffects = data.RelevantEffects ?? data.AllEffects;
            var pointsOfDeparture = subsetManager.AllPointsOfDeparture
                .Where(r => relevantEffects.Contains(r.Effect))
                .ToList();
            data.PointsOfDeparture = pointsOfDeparture;
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.PointsOfDeparture) && data.PointsOfDeparture != null) {
                localProgress.Update("Resampling points of departure.");
                data.PointsOfDeparture = resamplePointsOfDeparture(data.PointsOfDeparture, uncertaintySourceGenerators[UncertaintySource.PointsOfDeparture]);
            }
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(
            IPointsOfDepartureActionResult actionResult,
            ActionData data,
            SectionHeader header,
            int order,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            if (data.PointsOfDeparture != null) {
                var summarizer = new PointsOfDepartureSummarizer(ModuleConfig);
                summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            }
            localProgress.Update(100);
        }

        private static ICollection<Data.Compiled.Objects.PointOfDeparture> resamplePointsOfDeparture(
            ICollection<Data.Compiled.Objects.PointOfDeparture> pointsOfDeparture,
            IRandom generator
        ) {
            var capturingGenerator = new CapturingGenerator(generator);
            capturingGenerator.StartCapturing();

            var result = new List<Data.Compiled.Objects.PointOfDeparture>();
            foreach (var pointOfDeparture in pointsOfDeparture) {
                capturingGenerator.Repeat();
                if (pointOfDeparture.PointOfDepartureUncertains.Any()) {
                    var ix = generator.Next(0, pointOfDeparture.PointOfDepartureUncertains.Count);
                    var sampledUncertaintyValue = pointOfDeparture.PointOfDepartureUncertains.ElementAt(ix);
                    var sampled = pointOfDeparture.Clone();
                    sampled.DoseResponseModelParameterValues = sampledUncertaintyValue.DoseResponseModelParameterValues;
                    sampled.LimitDose = sampledUncertaintyValue.LimitDose;
                    result.Add(sampled);
                } else {
                    result.Add(pointOfDeparture);
                }
            }

            return result;
        }
    }
}
