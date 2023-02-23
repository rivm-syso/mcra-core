using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.NonDietaryExposures {

    [ActionType(ActionType.NonDietaryExposures)]
    public class NonDietaryExposuresActionCalculator : ActionCalculatorBase<INonDietaryExposuresActionResult> {

        public NonDietaryExposuresActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.NonDietaryExposures][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.NonDietaryExposuresUncertain][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.NonDietaryExposuresUncertain][ScopingType.NonDietarySurveys].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.NonDietarySurveyProperties][ScopingType.NonDietarySurveys].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleNonDietaryExposures) {
                result.Add(UncertaintySource.NonDietaryExposures);
            }
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var nonDietaryExposureSets = subsetManager.NonDietaryExposureSets;
            var selectedNonDietaryExposureSets = nonDietaryExposureSets.Any(c => string.IsNullOrEmpty(c.Code))
                ? nonDietaryExposureSets.Where(c => string.IsNullOrEmpty(c.Code)).ToList()
                : nonDietaryExposureSets;
            data.NonDietaryExposureSets = subsetManager.NonDietaryExposureSets;
            data.NonDietaryExposures = selectedNonDietaryExposureSets
                .GroupBy(r => r.NonDietarySurvey)
                .ToDictionary(r => r.Key, g => g.ToList());
            data.NonDietaryExposureRoutes = new List<ExposureRouteType>() {
                ExposureRouteType.Dermal,
                ExposureRouteType.Inhalation,
                ExposureRouteType.Oral
            };
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            if (factorialSet.Contains(UncertaintySource.NonDietaryExposures)) {
                var hasNonDietaryUncertainties = data.NonDietaryExposureSets?.Any(c => !string.IsNullOrEmpty(c.Code)) ?? false;
                if (hasNonDietaryUncertainties) {
                    data.NonDietaryExposures = ResampleNondietaryExposureUncertainSets(data, uncertaintySourceGenerators[UncertaintySource.NonDietaryExposures])
                        .GroupBy(r => r.NonDietarySurvey)
                        .ToDictionary(r => r.Key, r => r.ToList());
                } else if (!_project.NonDietarySettings.MatchSpecificIndividuals) {
                    // Only bootstrap nominal non-dietary exposures for unmatched
                    data.NonDietaryExposures = ResampleNondietaryExposures(data, uncertaintySourceGenerators[UncertaintySource.NonDietaryExposures], progressReport)
                        .GroupBy(r => r.NonDietarySurvey)
                        .ToDictionary(r => r.Key, r => r.ToList());
                }
            }
        }

        protected override void summarizeActionResult(INonDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new NonDietaryExposuresSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        public ICollection<NonDietaryExposureSet> ResampleNondietaryExposureUncertainSets(ActionData data, IRandom random) {
            var codes = data.NonDietaryExposureSets
                .Where(c => !string.IsNullOrEmpty(c.Code))
                .Select(c => c.Code)
                .Distinct()
                .ToList();
            var uncertaintySet = codes.ElementAt(random.Next(0, codes.Count));
            var result = data.NonDietaryExposureSets.Where(c => c.Code == uncertaintySet).ToList();
            return result;
        }

        public ICollection<NonDietaryExposureSet> ResampleNondietaryExposures(ActionData data, IRandom random, CompositeProgressState progressState = null) {
            var seed = random.Next();
            var result = data.NonDietaryExposureSets
                .GroupBy(r => r.NonDietarySurvey)
                .AsParallel()
                .WithDegreeOfParallelism(100)
                .SelectMany(g => {
                    var randomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, g.Key.Code), true);
                    var newExposureSets = g
                        .Where(c => string.IsNullOrEmpty(c.Code))
                        .ToList();

                    newExposureSets = newExposureSets
                        .Resample(randomGenerator)
                        .Select((r, ix) => new NonDietaryExposureSet() {
                            Code = r.Code,
                            NonDietarySurvey = r.NonDietarySurvey,
                            IndividualCode = ix.ToString(), // Create new individual code
                            NonDietaryExposures = r.NonDietaryExposures
                        })
                        .ToList();
                    return newExposureSets;
                })
                .OrderBy(r => r.IndividualCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.NonDietarySurvey.Code, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return result;
        }
    }
}
