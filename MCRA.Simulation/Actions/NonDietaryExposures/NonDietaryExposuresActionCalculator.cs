using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Actions.NonDietaryExposures {

    [ActionType(ActionType.NonDietaryExposures)]
    public class NonDietaryExposuresActionCalculator(ProjectDto project) : ActionCalculatorBase<INonDietaryExposuresActionResult>(project) {
        private NonDietaryExposuresModuleConfig ModuleConfig => (NonDietaryExposuresModuleConfig)_moduleSettings;

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.NonDietaryExposures][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.NonDietaryExposuresUncertain][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.NonDietaryExposuresUncertain][ScopingType.NonDietarySurveys].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.NonDietarySurveyProperties][ScopingType.NonDietarySurveys].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleNonDietaryExposures) {
                result.Add(UncertaintySource.NonDietaryExposures);
            }
            return result;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var nonDietaryExposureSets = subsetManager.NonDietaryExposureSets;
            data.NonDietaryExposureSets = nonDietaryExposureSets;
            data.NonDietaryExposures = nonDietaryExposureSets
                .Where(c => !c.IsUncertaintySet())
                .GroupBy(r => r.NonDietarySurvey)
                .ToDictionary(r => r.Key, g => g.ToList());
            data.NonDietaryExposureRoutes = [
                ExposureRoute.Dermal,
                ExposureRoute.Inhalation,
                ExposureRoute.Oral
            ];

            var exposureUnits = subsetManager.NonDietaryExposureSets
                .Select(c => c.NonDietarySurvey.ExposureUnit)
                .ToHashSet();
            if (exposureUnits.Count == 1) {
                data.NonDietaryExposureUnit = exposureUnits.First();
            } else {
                throw new Exception("The non-dietary surveys have different exposure units, which is currently not allowed.");
            }
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            if (factorialSet.Contains(UncertaintySource.NonDietaryExposures)) {
                var hasNonDietaryUncertainties = data.NonDietaryExposureSets?.Any(c => !string.IsNullOrEmpty(c.Code)) ?? false;
                if (hasNonDietaryUncertainties) {
                    data.NonDietaryExposures = ResampleNondietaryExposureUncertainSets(data, uncertaintySourceGenerators[UncertaintySource.NonDietaryExposures])
                        .GroupBy(r => r.NonDietarySurvey)
                        .ToDictionary(r => r.Key, r => r.ToList());
                } else if (ModuleConfig.NonDietaryPopulationAlignmentMethod != PopulationAlignmentMethod.MatchIndividualID) {
                    // Only bootstrap nominal non-dietary exposures for unmatched
                    data.NonDietaryExposures = ResampleNondietaryExposures(data, uncertaintySourceGenerators[UncertaintySource.NonDietaryExposures], progressReport)
                        .GroupBy(r => r.NonDietarySurvey)
                        .ToDictionary(r => r.Key, r => r.ToList());
                }
            }
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(INonDietaryExposuresActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new NonDietaryExposuresSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }

        public ICollection<NonDietaryExposureSet> ResampleNondietaryExposureUncertainSets(ActionData data, IRandom random) {
            var codes = data.NonDietaryExposureSets
                .Where(c => c.IsUncertaintySet())
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
                    var randomGenerator = new McraRandomGenerator(RandomUtils.CreateSeed(seed, g.Key.Code));
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
