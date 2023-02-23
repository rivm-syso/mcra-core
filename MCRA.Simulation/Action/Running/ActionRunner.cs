using log4net;
using MCRA.Data.Management;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Actions;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;
using System.Diagnostics;

namespace MCRA.Simulation.Action {

    public sealed class ActionRunner {

        private readonly ILog _log;
        private Stopwatch _stopwatch;

        private readonly ProjectDto _project;
        private readonly ActionCalculatorProvider _actionCalculatorProvider;

        private static readonly int _actionSettingsSectionOrder = 3;
        private static readonly int _actionDataSectionOrder = 4;
        private static readonly int _actionOutputSectionOrder = 5;

        public ActionRunner(ProjectDto project) {
            _log = LogManager.GetLogger(GetType());
            _project = project;
            _actionCalculatorProvider = new ActionCalculatorProvider();
        }

        public void SummarizeSettings(ActionMapping actionMapping, SectionHeader header) {
            var summarizer = new ActionSettingsSummarizer();
            var section = summarizer.Summarize(_project, actionMapping);
            int order = _actionSettingsSectionOrder;
            section.WriteToOutputSummaryRecursive(header, ref order, OutputConstants.ActionSettingsSectionGuid, false, false);
        }

        public void SummarizeDataSources(
            ActionMapping actionMapping,
            IEnumerable<IRawDataSourceVersion> dataSourceVersions,
            SectionHeader header
        ) {
            var summarizer = new ActionSettingsSummarizer();
            var section = summarizer.SummarizeDataSources(dataSourceVersions, actionMapping);
            int order = -1;
            section.WriteToOutputSummaryRecursive(header, ref order, Guid.NewGuid(), false, false);
        }

        public void ValidateData(ActionMapping actionMapping, ICompiledLinkManager linkManager) {
            var moduleMappings = actionMapping.GetModuleMappings()
                .Where(r => r.IsVisible).ToList();
            foreach (var moduleMapping in moduleMappings) {
                if (moduleMapping.IsSpecified && !moduleMapping.IsCompute) {
                    var actionCalculator = moduleMapping.ActionCalculator;
                    linkManager.LoadScope(moduleMapping.TableGroup);
                    var readingReports = actionCalculator.GetDataReadingReport(linkManager);
                    moduleMapping.CompiledDataReadingReports = readingReports;
                    moduleMapping.IsDataValid = readingReports?.Values?.All(r => !r.IsError) ?? true;
                    moduleMapping.IsDataDependentSettingsValid = actionCalculator.CheckDataDependentSettings(linkManager);
                } else {
                    moduleMapping.IsDataValid = true;
                    moduleMapping.IsDataDependentSettingsValid = true;
                }

                var activeInputMappings = moduleMapping.InputRequirements
                    .Where(r => r.IsVisible && (r.IsRequired || actionMapping.ModuleMappingsDictionary[r.ActionType].IsSpecified))
                    .ToList();

                moduleMapping.IsSettingsValid = moduleMapping.IsSpecified
                    && (moduleMapping.Settings?.IsValid ?? true)
                    && activeInputMappings.All(r => actionMapping.ModuleMappingsDictionary[r.ActionType].IsSettingsValid);

                moduleMapping.IsValid = moduleMapping.IsSettingsValid
                    && moduleMapping.IsDataValid
                    && moduleMapping.IsDataDependentSettingsValid
                    && activeInputMappings.All(r => actionMapping.ModuleMappingsDictionary[r.ActionType].IsValid
                        && actionMapping.ModuleMappingsDictionary[r.ActionType].IsDataValid
                        && actionMapping.ModuleMappingsDictionary[r.ActionType].IsDataDependentSettingsValid
                    );
            }
        }

        public void Run(
            ActionMapping actionMapping,
            SubsetManager subsetManager,
            SectionHeader header,
            IRawDataWriter outputRawDataWriter,
            CompositeProgressState progressReport
        ) {
            var doUncertainty = _project.UncertaintyAnalysisSettings.DoUncertaintyAnalysis
                && actionMapping.ModuleDefinition.HasUncertaintyAnalysis;
            var uncertaintyCycles = _project.UncertaintyAnalysisSettings.NumberOfResampleCycles;
            var analysisRunCycles = doUncertainty ? uncertaintyCycles + 1 : 1;

            var subProgressRunNominal = 100D / analysisRunCycles;

            // Nominal run
            _actionCalculatorProvider.Reset();
            var data = runNominal(
                actionMapping,
                subsetManager,
                header,
                outputRawDataWriter,
                progressReport.NewCompositeState(subProgressRunNominal)
            );

            // Uncertainty runs
            if (doUncertainty) {

                // Store original seed and create main random seed
                var nominalSeed = _project.MonteCarloSettings.RandomSeed;
                var masterSeed = _project.MonteCarloSettings.RandomSeed;
                if (_project.MonteCarloSettings.RandomSeed == 0) {
                    var generator = new McraRandomGenerator();
                    masterSeed = generator.Next();
                }

                // Create random generators for data bootstraps
                var randomSeedGenerator = new McraRandomGenerator(masterSeed, true);
                var uncertaintySourceGenerators = new Dictionary<UncertaintySource, IRandom>();
                ICollection<UncertaintySource> uncertaintySources;
                if (Simulation.IsBackwardCompatibilityMode) {
                    uncertaintySources = new List<UncertaintySource> {
                        UncertaintySource.Individuals,
                        UncertaintySource.Portions,
                        UncertaintySource.Concentrations,
                        UncertaintySource.Processing,
                        UncertaintySource.IntraSpecies,
                        UncertaintySource.InterSpecies,
                        UncertaintySource.RPFs,
                        UncertaintySource.NonDietaryExposures,
                        UncertaintySource.AssessmentGroupMemberships,
                        UncertaintySource.ImputeExposureDistributions,
                        UncertaintySource.KineticModelParameters
                    };
                    foreach (var source in uncertaintySources) {
                        uncertaintySourceGenerators[source] = new McraRandomGenerator(randomSeedGenerator.Next(), true);
                    }
                    uncertaintySourceGenerators[UncertaintySource.HazardCharacterisationsImputation] = uncertaintySourceGenerators[UncertaintySource.RPFs];
                    uncertaintySourceGenerators[UncertaintySource.HazardCharacterisationsSelection] = uncertaintySourceGenerators[UncertaintySource.RPFs];
                    uncertaintySourceGenerators[UncertaintySource.DoseResponseModels] = uncertaintySourceGenerators[UncertaintySource.RPFs];
                    uncertaintySourceGenerators[UncertaintySource.PointsOfDeparture] = uncertaintySourceGenerators[UncertaintySource.RPFs];
                    uncertaintySourceGenerators[UncertaintySource.ConcentrationModelling] = uncertaintySourceGenerators[UncertaintySource.Concentrations];
                } else {
                    var moduleMappings = actionMapping.GetModuleMappings();
                    uncertaintySources = moduleMappings
                        .SelectMany(r => r.ActionCalculator.GetRandomSources())
                        .Distinct()
                        .ToList();
                    foreach (var source in uncertaintySources) {
                        uncertaintySourceGenerators[source] = new McraRandomGenerator(
                            RandomUtils.CreateSeed(masterSeed, (int)source)
                        );
                    }
                }

                // Create uncertainty factorial design
                var doUncertaintyFactorial = _project.UncertaintyAnalysisSettings.DoUncertaintyFactorial;
                var factorialDesign = Simulation.IsBackwardCompatibilityMode
                    ? UncertaintyFactorialDesignGenerator.Create(_project.UncertaintyAnalysisSettings)
                    : UncertaintyFactorialDesignGenerator.Create(uncertaintySources);
                var factorialResults = new List<UncertaintyFactorialResultRecord>();

                // Total number of uncertainty cycles
                var totalBootstraps = doUncertaintyFactorial 
                    ? uncertaintyCycles * factorialDesign.Count : uncertaintyCycles;

                // Initialize progress counters
                var factorialSetCount = 0;
                var bootstrapCount = 0;

                // Loop over the uncertainty factorial designs
                foreach (var factorialSet in factorialDesign) {

                    // Reset random generators for each factorial set
                    randomSeedGenerator.Reset();

                    if (Simulation.IsBackwardCompatibilityMode) {
                        // Reset all generators
                        foreach (var generator in uncertaintySourceGenerators.Values) {
                            generator.Reset();
                        }
                    }

                    // If this is not the full factorial set and we do not want to run
                    // the uncertainty factorial, then skip this cycle.
                    // TODO: refactor. If we do not want to run the uncertainty factorial
                    // then we should not create a factorial design containing all factorial
                    // sets in the first place.
                    if (!factorialSet.IsFullSet && !doUncertaintyFactorial) {
                        continue;
                    }

                    // Increate factorial set counter
                    factorialSetCount++;

                    for (int i = 0; i < uncertaintyCycles; i++) {

                        // Increase total bootstrap counter
                        bootstrapCount++;

                        // Create progress state for bootstrap
                        var bootstrapProgress = progressReport.NewProgressState(100D / totalBootstraps);

                        // Reset action calculators
                        _actionCalculatorProvider.Reset();

                        // Update progress
                        if (_project.UncertaintyAnalysisSettings.DoUncertaintyFactorial) {
                            // If uncertainty factorial then also print factorial set
                            var msg = $"Uncertainty set {i + 1}/{uncertaintyCycles}, cycle {factorialSetCount}/{factorialDesign.Count}";
                            bootstrapProgress.Update(msg);
                            logMessage(msg);
                        } else {
                            // Simple uncertainty run
                            var msg = $"Uncertainty cycle {i + 1}/{uncertaintyCycles}";
                            bootstrapProgress.Update(msg);
                            logMessage(msg);
                        }

                        // Set random seed for simulation
                        _project.MonteCarloSettings.RandomSeed = Simulation.IsBackwardCompatibilityMode
                            ? randomSeedGenerator.Next(1, int.MaxValue)
                            : RandomUtils.CreateSeed(masterSeed, i + 1);

                        // Set uncertainty source generators based on master seed and source hashes
                        if (!Simulation.IsBackwardCompatibilityMode) {
                            foreach (var source in uncertaintySources) {
                                if (factorialSet.Contains(source)) {
                                    // Create a random generator for the uncertainty source
                                    uncertaintySourceGenerators[source] = new McraRandomGenerator(
                                        RandomUtils.CreateSeed(masterSeed, i + 1, (int)source)
                                    );
                                } else {
                                    // Use a fixed random generator based on the nominal run if the
                                    // uncertainty source is not part of the factorial set
                                    uncertaintySourceGenerators[source] = new McraRandomGenerator(nominalSeed);
                                }
                            }
                        }

                        // Deep copy simulation data of nominal run
                        var simulationData = data.Copy();

                        // Run uncertainty cycle
                        var hiddenProgress = new CompositeProgressState(bootstrapProgress.CancellationToken);
                        var result = runActionUncertain(
                            actionMapping,
                            _project,
                            simulationData,
                            factorialSet,
                            uncertaintySourceGenerators,
                            header,
                            outputRawDataWriter,
                            bootstrapCount,
                            hiddenProgress
                        );

                        // If we have an uncertainty factorial result, then add it to the factorial
                        // results collection.
                        var uncertaintyFactorialResult = result.FactorialResult;
                        if (uncertaintyFactorialResult != null) {
                            factorialResults.Add(new UncertaintyFactorialResultRecord(
                                factorialSet.UncertaintySources.ToList(),
                                uncertaintyFactorialResult
                            ));
                        }

                        bootstrapProgress.Update(100);
                    }
                }

                // If the run involved an uncertainty factorial, then summarize it.
                if (actionMapping.ModuleDefinition.HasUncertaintyFactorial
                    && doUncertaintyFactorial
                    && factorialResults.Any()
                ) {
                    var actionCalculator = _actionCalculatorProvider.Get(actionMapping.MainActionType, _project, false);
                    actionCalculator.SummarizeUncertaintyFactorial(factorialDesign, factorialResults, header);
                }
            }

            outputRawDataWriter?.Store();
        }

        private ActionData runNominal(
            ActionMapping actionMapping,
            SubsetManager subsetManager,
            SectionHeader header,
            IRawDataWriter outputRawDataWriter,
            CompositeProgressState progressReport
        ) {
            var data = new ActionData();

            var moduleMappings = actionMapping.GetModuleMappings();
            var subProgress = 100D / moduleMappings.Where(r => r.IsSpecified).Count();

            SectionHeader inputHeader = null;
            Func<SectionHeader> getInputHeader = () => {
                if (inputHeader == null) {
                    var inputDataSummary = new SimulationInputSummary();
                    inputHeader = header.AddSubSectionHeaderFor(inputDataSummary, "Sub-action results", _actionDataSectionOrder);
                    inputHeader.SaveSummarySection(inputDataSummary);
                }
                return inputHeader;
            };

            int order = 0;
            foreach (var moduleMapping in moduleMappings) {
                IActionResult result = null;
                var actionCalculator = moduleMapping.ActionCalculator;
                if (moduleMapping.IsSpecified) {
                    var actionDisplayName = moduleMapping.ActionType.GetDisplayName(true);
                    var actionProgress = progressReport.NewCompositeState(subProgress);
                    if (!moduleMapping.IsCompute) {
                        // Data action
                        logTimerStart($"Loading {actionDisplayName}");
                        moduleMapping.ActionCalculator.LoadData(data, subsetManager, actionProgress.NewCompositeState(header == null ? 100D : 80));
                        logTimerStop($"Finished loading {actionDisplayName}");
                    } else if (actionCalculator.ShouldCompute) {
                        // Calculation action
                        logTimerStart($"Computing {actionDisplayName}");
                        result = actionCalculator.Run(data, actionProgress.NewCompositeState(header == null ? 100D : 80));
                        logTimerStop($"Finished computing {actionDisplayName}");
                        actionCalculator.UpdateSimulationData(data, result);
                    }
                    if (header != null) {
                        // Summarize
                        var summarizerProgress = actionProgress.NewCompositeState(20D);
                        var summaryHeader = moduleMapping.IsMainModule ? header : getInputHeader();
                        var summaryOrder = moduleMapping.IsMainModule ? _actionOutputSectionOrder : order++;
                        logTimerStart($"Summarizing {actionDisplayName}");
                        actionCalculator.SummarizeActionResult(result, data, summaryHeader, summaryOrder, summarizerProgress);
                        logTimerStop($"Finished summarizing {actionDisplayName}");
                    }
                    if (outputRawDataWriter != null && moduleMapping.IsMainModule) {
                        actionCalculator.WriteOutputData(outputRawDataWriter, data, result);
                    }
                    actionProgress.MarkCompleted();
                } else {
                    actionCalculator.LoadDefaultData(data);
                }
            }
            progressReport.MarkCompleted();

            return data;
        }

        private IActionResult runActionUncertain(
            ActionMapping actionMapping,
            ProjectDto project,
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            SectionHeader header,
            IRawDataWriter outputRawDataWriter,
            int bootstrap,
            CompositeProgressState progressReport
        ) {
            var moduleMappings = actionMapping.GetModuleMappings();
            var subProgress = 100D / moduleMappings.Count;
            IActionResult result = null;
            foreach (var moduleMapping in moduleMappings) {
                var localProgress = progressReport.NewProgressState(subProgress);
                var actionDisplayName = moduleMapping.ActionType.GetDisplayName(true);
                var actionCalculator = _actionCalculatorProvider.Get(moduleMapping.ActionType, project, moduleMapping.IsVisible);
                IActionResult subActionResult = null;
                if (moduleMapping.IsSpecified) {
                    if (!moduleMapping.IsCompute) {
                        localProgress.Update($"Loading uncertain {actionDisplayName}");
                        logTimerStart($"Loading uncertain {actionDisplayName}");
                        actionCalculator.LoadDataUncertain(data, factorialSet, uncertaintySourceGenerators, progressReport.NewCompositeState(subProgress));
                        logTimerStop($"Finished loading uncertain {actionDisplayName}");
                        localProgress.Update($"Finished loading uncertain {actionDisplayName}", 100);
                    } else if (actionCalculator.ShouldCompute) {
                        localProgress.Update($"Computing uncertain {actionDisplayName}");
                        logTimerStart($"Computing uncertain {actionDisplayName}");
                        subActionResult = actionCalculator.RunUncertain(data, factorialSet, uncertaintySourceGenerators, header, progressReport.NewCompositeState(.8 * subProgress));
                        logTimerStop($"Finished computing uncertain {actionDisplayName}");
                        actionCalculator.UpdateSimulationDataUncertain(data, subActionResult);
                    }
                    if (header != null && factorialSet.IsFullSet) {
                        var summaryHeader = moduleMapping.IsMainModule ? header : header.GetSubSectionHeader<SimulationInputSummary>();
                        logTimerStart($"Summarizing results {actionDisplayName}");
                        actionCalculator.SummarizeActionResultUncertain(factorialSet, subActionResult, data, summaryHeader, progressReport.NewCompositeState(.2 * subProgress));
                        logTimerStop($"Finished summarizing results {actionDisplayName}");
                    }
                    localProgress.Update($"Finished computing uncertain {actionDisplayName}", 100);
                }
                if (moduleMapping.IsMainModule) {
                    result = subActionResult;
                }
                if (outputRawDataWriter != null && moduleMapping.IsMainModule && factorialSet.IsFullSet) {
                    actionCalculator.WriteOutputDataUncertain(outputRawDataWriter, data, subActionResult, bootstrap);
                }
            }
            progressReport.MarkCompleted();
            return result;
        }

        #region Logging

        private void logMessage(string message) {
            _log.Info(message);
        }

        private void logTimerStart(string message) {
            if (_stopwatch == null) {
                _stopwatch = new Stopwatch();
            }
            _log.Info(message);
            _stopwatch.Start();
        }

        private void logTimerStop(string message) {
            _stopwatch.Stop();
            _log.InfoFormat(message, _stopwatch.Elapsed);
            _stopwatch.Reset();
        }

        #endregion
    }
}
