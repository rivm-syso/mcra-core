using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Utils;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Calculators.FoodConversionCalculation {
    public sealed class FoodConversionCalculator {

        private readonly FoodConversionsModuleConfig _settings;
        private readonly IDictionary<string, Food> _allFoods;
        private readonly IDictionary<string, ProcessingType> _processingTypes;
        private readonly IDictionary<Food, ICollection<Food>> _foodExtrapolations;
        private readonly ILookup<Food, FoodTranslation> _foodCompositions;
        private readonly IDictionary<Food, Food> _tdsFoodSampleCompositions;
        private readonly ICollection<MarketShare> _marketShares;
        private readonly HashSet<Food> _modelledFoods;
        private readonly HashSet<(Food, Compound)> _modelledFoodSubstanceInfos;
        private readonly ILookup<Food, ProcessingFactor> _genericProcessingFactors;
        private readonly ILookup<(Food, Compound), ProcessingFactor> _substanceSpecificProcessingFactors;

        public FoodConversionCalculator(
            FoodConversionsModuleConfig settings,
            IDictionary<string, Food> allFoods,
            IDictionary<(Food, Compound), ModelledFoodInfo> foodAsMeasuredInfos,
            ICollection<Food> modelledFoods,
            IDictionary<Food, ICollection<Food>> foodExtrapolations = null,
            ILookup<Food, FoodTranslation> foodCompositions = null,
            IDictionary<Food, Food> tdsFoodSampleCompositionDictionary = null,
            ICollection<ProcessingType> processingTypes = null,
            ICollection<MarketShare> marketShares = null,
            ICollection<ProcessingFactor> processingFactors = null
        ) {
            _settings = settings;
            _allFoods = allFoods;
            _modelledFoodSubstanceInfos = foodAsMeasuredInfos?.Keys.ToHashSet();
            _modelledFoods = modelledFoods?.ToHashSet();
            _foodExtrapolations = settings.UseReadAcrossFoodTranslations ? foodExtrapolations : null;
            _processingTypes = settings.UseDefaultProcessingFactor ? processingTypes?.ToDictionary(r => r.Code, StringComparer.OrdinalIgnoreCase) : null;
            _foodCompositions = settings.UseComposition ? foodCompositions : null;
            _tdsFoodSampleCompositions = settings.TotalDietStudy ? tdsFoodSampleCompositionDictionary : null;
            _marketShares = settings.UseMarketShares ? marketShares : null;
            if (settings.UseProcessing) {
                _genericProcessingFactors = processingFactors?
                    .Where(r => r.FoodProcessed != null && r.Compound == null)
                    .ToLookup(r => r.FoodProcessed);
                _substanceSpecificProcessingFactors = processingFactors?
                    .Where(r => r.FoodProcessed != null && r.Compound != null)
                    .ToLookup(r => (r.FoodProcessed, r.Compound));
            }
        }

        public List<FoodConversionResult> CalculateFoodConversions(
            ICollection<Food> foodsAsEaten,
            ICollection<Compound> substances
        ) {
            return Calculate(foodsAsEaten, substances, new CompositeProgressState());
        }

        public List<FoodConversionResult> Calculate(
            ICollection<Food> foodsAsEaten,
            ICollection<Compound> substances,
            CompositeProgressState progressState
        ) {
            var localProgress = progressState.NewProgressState(100);
            var cancelToken = progressState?.CancellationToken ?? new();
            var conversionResults = new List<FoodConversionResult>();
            var i = 0;

            if (_settings.SubstanceIndependent) {
                conversionResults = foodsAsEaten
                   .AsParallel()
                   .WithCancellation(cancelToken)
                   .SelectMany(food => Convert(food, null))
                   .ToList();
            } else {
                foreach (var substance in substances) {
                    localProgress.Update($"Converting substance {substance.Code}", i++ * (100D / substances.Count));
                    var foodConversionResults = foodsAsEaten
                        .AsParallel()
                        .WithCancellation(cancelToken)
                        .SelectMany(food => Convert(food, substance));
                    conversionResults.AddRange(foodConversionResults);
                }
            }

            // Collect the conversion results in a list and order by conversion steps
            var conversionResultsList = conversionResults
                .OrderBy(fcr => fcr.AllStepsToMeasuredString, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!conversionResults.Any(c => c.ConversionStepResults.Last().Finished)) {
                throw new Exception("No link between the consumed foods and foods with concentrations.");
            }

            localProgress.Update("Conversion finished", 100D);

            return conversionResultsList;
        }

        /// <summary>
        /// Runs a food conversion for the specified food and substance.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="compound"></param>
        /// <returns></returns>
        public List<FoodConversionResult> Convert(Food food, Compound compound) {
            var conversionResults = new List<FoodConversionResult>();
            processSteps(food, food.Code, new FoodConversionResult() {
                FoodAsEaten = food,
                Compound = compound,
                FoodTrace = [food],
            }, conversionResults);
            return conversionResults;
        }

        /// <summary>
        /// Main loop of the food conversion algorithm. Steers the steps of the conversion algorithm.
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="result"></param>
        /// <param name="conversionResults"></param>
        private void processSteps(Food food, string searchCode, FoodConversionResult result, List<FoodConversionResult> conversionResults) {
            if (!DetectCircularLoop(food, searchCode, result, conversionResults)) {
                bool found;
                if (_settings.SubstanceIndependent) {
                    found = IdenticalCodeConcentrationSubstanceIndependent(food, searchCode, result, conversionResults);
                } else {
                    found = IdenticalCodeConcentration(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseProcessing) {
                    found = ProcessingLink(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseComposition) {
                    found = FoodCompositionLink(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.TotalDietStudy) {
                    found = TdsFoodCompositionLink(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseReadAcrossFoodTranslations) {
                    found = FoodExtrapolationLink(food, searchCode, result, conversionResults);
                }

                if (!found) {
                    found = SubTypeLink(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseSuperTypes) {
                    found = SuperTypeLink(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseSuperTypes) {
                    found = HierarchySuperTypeLink(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseDefaultProcessingFactor) {
                    found = DefaultProcessingFactor(food, searchCode, result, conversionResults);
                }

                if (!found && _settings.UseDefaultProcessingFactor) {
                    found = CompositeFacetProcessingFactor(food, searchCode, result, conversionResults);
                }

                if (!found) {
                    found = RemoveFacets(food, searchCode, result, conversionResults);
                }

                if (!found) {
                    // If we haven't found any successful processing step, then add the "no match" record
                    result.ConversionStepResults.Add(
                        new FoodConversionResultStep() {
                            Step = FoodConversionStepType.NoMatch,
                            FoodCodeFrom = searchCode,
                            FoodCodeTo = "no match",
                            Finished = false,
                        }
                    );
                    conversionResults.Add(new FoodConversionResult(result));
                    result.Initialize();
                }
            }
        }

        /// <summary>
        /// Checks if the processSteps recursive algorithm is not stuck in an infinite loop
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <returns></returns>
        private bool DetectCircularLoop(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            bool circularLoopDetected = fcr.FoodTrace
                .Take(fcr.FoodTrace.Count - 1)
                .Contains(food);
            if (circularLoopDetected) {
                fcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                    Step = FoodConversionStepType.CircularLoop,
                    FoodCodeFrom = searchCode,
                    FoodCodeTo = food?.Code ?? string.Empty,
                    Finished = false,
                });
                conversionResults.Add(new FoodConversionResult(fcr));
                fcr.Initialize();
            }
            return circularLoopDetected;
        }

        /// <summary>
        /// STEP 1: Identical Code, check if code is in concentrationvalues
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool IdenticalCodeConcentration(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null) {
                return false;
            }
            var found = false;
            if (_modelledFoodSubstanceInfos?.Contains((food, fcr.Compound)) ?? false) {
                fcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                    Step = FoodConversionStepType.Concentration,
                    FoodCodeFrom = searchCode,
                    FoodCodeTo = food.Code,
                    Finished = true,
                });
                found = true;
                if (fcr.FoodTrace.Any()) {
                    fcr.FoodTrace.RemoveAt(fcr.FoodTrace.Count - 1);
                }
                conversionResults.Add(new FoodConversionResult(fcr) { FoodAsMeasured = food });
                fcr.Initialize();
            }
            return found;
        }


        /// <summary>
        /// STEP 1: Identical Code, check if code is in concentrationvalues
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool IdenticalCodeConcentrationSubstanceIndependent(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null) {
                return false;
            }
            var found = false;
            if (_modelledFoods?.Contains(food) ?? false) {
                fcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                    Step = FoodConversionStepType.Concentration,
                    FoodCodeFrom = searchCode,
                    FoodCodeTo = food.Code,
                    Finished = true,
                });
                found = true;
                if (fcr.FoodTrace.Any()) {
                    fcr.FoodTrace.RemoveAt(fcr.FoodTrace.Count - 1);
                }
                conversionResults.Add(new FoodConversionResult(fcr) { FoodAsMeasured = food });
                fcr.Initialize();
            }
            return found;
        }

        /// <summary>
        /// STEP 2a: Processing link, check if code is in processing
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool ProcessingLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null || _genericProcessingFactors == null || _substanceSpecificProcessingFactors == null) {
                return false;
            }
            var found = false;

            ProcessingFactor processingFactor = null;
            if (_substanceSpecificProcessingFactors.Contains((food, fcr.Compound))) {
                processingFactor = _substanceSpecificProcessingFactors[(food, fcr.Compound)].First();
            } else if (_genericProcessingFactors.Contains(food)) {
                processingFactor = _genericProcessingFactors[food].First();
            }
            if (processingFactor != null) {
                found = true;
                var foodUnProcessed = processingFactor.FoodUnprocessed;

                // Check if there is an additional weight reduction/increase
                var translationRecord = (_foodCompositions?.Contains(food) ?? false)
                    ? _foodCompositions[food]?.FirstOrDefault(ci => ci.FoodFrom.Code.Equals(searchCode, StringComparison.OrdinalIgnoreCase) && ci.FoodTo.Code.Equals(foodUnProcessed.Code, StringComparison.OrdinalIgnoreCase))
                    : null;
                var proportion = (translationRecord?.Proportion ?? 100D) / 100;
                var processingSteps = new List<ProcessingType>() { processingFactor.ProcessingType };
                var proportionProcessing = proportion;
                var conversionStep = new FoodConversionResultStep() {
                    Step = FoodConversionStepType.ProcessingExact,
                    FoodCodeFrom = searchCode,
                    FoodCodeTo = foodUnProcessed.Code,
                    Proportion = proportion,
                    ProportionProcessed = proportionProcessing,
                    ProcessingTypes = processingSteps.ToList(),
                    Finished = false,
                };
                var newFcr = new FoodConversionResult(fcr);
                newFcr.Proportion *= proportion;
                newFcr.ConversionStepResults.Add(conversionStep);
                newFcr.FoodTrace.Add(food);
                processSteps(foodUnProcessed, foodUnProcessed.Code, newFcr, conversionResults);
            }
            return found;
        }

        /// <summary>
        /// STEP 3a: Food Composition Link, check if code is in foodtranslation
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool FoodCompositionLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            var found = false;
            if (food != null && (_foodCompositions?.Contains(food) ?? false)) {
                found = true;
                foreach (var ingredient in _foodCompositions[food]) {
                    var toFood = ingredient.FoodTo;
                    var proportion = ingredient.Proportion / 100;
                    var processingSteps = (_settings.UseDefaultProcessingFactor || !_settings.UseProcessing)
                        ? food.ProcessingTypes.Except(toFood.ProcessingTypes).ToList()
                        : [];
                    if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(searchCode)
                        && _processingTypes != null
                        && _processingTypes.TryGetValue(searchCode.Replace(toFood.Code, "").Replace("#", ""), out var processingType)
                    ) {
                        // If the search code is a FoodEx2 code with factets and the to code
                        // is a substring of the search code, then check if the stripped string
                        // matches a processing type. E.g., for "AR4WS#F20.AO7QF$F28.BO7QG" and
                        // "AR4WS", the difference "#F20.AO7QF$F28.BO7QG" could be matched to a
                        // processing type with code "F20.AO7QF$F28.BO7QG". Notice that the '#'
                        // should be ignored.
                        processingSteps.Add(processingType);
                    }
                    var isProcessingStep = processingSteps.Any();
                    var proportionProcessing = isProcessingStep ? proportion : 1D;
                    var conversionStep = new FoodConversionResultStep() {
                        Step = isProcessingStep
                            ? FoodConversionStepType.ProcessingTranslation
                            : FoodConversionStepType.CompositionExact,
                        FoodCodeFrom = searchCode,
                        FoodCodeTo = toFood.Code,
                        Proportion = proportion,
                        ProportionProcessed = proportionProcessing,
                        ProcessingTypes = processingSteps.ToList(),
                        Finished = false,
                    };
                    var newFcr = new FoodConversionResult(fcr);
                    newFcr.Proportion *= proportion;
                    newFcr.ConversionStepResults.Add(conversionStep);
                    newFcr.FoodTrace.Add(food);
                    processSteps(toFood, toFood.Code, newFcr, conversionResults);
                }
            }
            return found;
        }

        /// <summary>
        /// STEP 3b: TDS Food Composition Link, check if code is in TdsFoodComposition
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool TdsFoodCompositionLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null) {
                return false;
            }
            var found = false;
            if (_tdsFoodSampleCompositions?.Count > 0) {
                if (_tdsFoodSampleCompositions.TryGetValue(food, out var foodTo)) {
                    found = true;
                    var newFcr = new FoodConversionResult(fcr);
                    newFcr.Proportion *= 1;
                    newFcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                        Step = FoodConversionStepType.TDSCompositionExact,
                        FoodCodeFrom = searchCode,
                        FoodCodeTo = foodTo.Code,
                        Finished = false,
                    });
                    newFcr.FoodTrace.Add(food);
                    processSteps(foodTo, foodTo.Code, newFcr, conversionResults);
                }
            }
            return found;
        }

        /// <summary>
        /// STEP 3c: Read Across Food Translation Link, check if code is in ReadAcrossFoodTranslation
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool FoodExtrapolationLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null) {
                return false;
            }
            var found = false;
            if (_foodExtrapolations != null
                && _foodExtrapolations.TryGetValue(food, out var readAcrossFoods)) {
                found = true;
                var newFcr = new FoodConversionResult(fcr);
                var foodTo = readAcrossFoods.First();
                newFcr.Proportion *= 1;
                newFcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                    Step = FoodConversionStepType.ReadAcross,
                    FoodCodeFrom = searchCode,
                    FoodCodeTo = foodTo.Code,
                    Finished = false,
                });
                newFcr.FoodTrace.Add(food);
                processSteps(foodTo, foodTo.Code, newFcr, conversionResults);
            }
            return found;
        }

        /// <summary>
        /// STEP 4: (SubType Link)
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool SubTypeLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null) {
                return false;
            }
            bool found = false;
            if (_marketShares?.Count > 0) {
                var marketShares = _marketShares
                    .Where(ms => ms.Food.Code.Length > searchCode.Length && ms.Food.Code.StartsWith(searchCode))
                    .Where(m => m.Food.Code.Substring(searchCode.Length + 1).Contains("$") == false || m.Food.Code.Substring(searchCode.Length + 1).Contains("."))
                    .ToList();
                var sumMarketShares = marketShares.Sum(m => m.Percentage);
                if (marketShares.Any() && (_settings.UseSubTypes || Math.Abs(100 - sumMarketShares) <= 1e-10)) {
                    foreach (var marketShare in marketShares) {
                        var newFcr = new FoodConversionResult(fcr);
                        found = true;
                        food = marketShare.Food;
                        newFcr.MarketShare *= marketShare.Percentage / sumMarketShares;
                        newFcr.ConversionStepResults.Add(
                            new FoodConversionResultStep() {
                                Step = FoodConversionStepType.Subtype,
                                FoodCodeFrom = marketShare.Food.Code,
                                FoodCodeTo = food?.Code ?? string.Empty,
                                Finished = false,
                            });
                        newFcr.FoodTrace.Add(food);
                        processSteps(food, marketShare.Food.Code, newFcr, conversionResults);
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// STEP 5: (SuperType Link)
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool SuperTypeLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null) {
                return false;
            }
            var found = false;

            var separationCharacter = "$";
            if (searchCode.Contains(".")) {
                separationCharacter = ".";
            }
            if (searchCode.Contains(separationCharacter)) {
                var newSearchCode = searchCode;
                while (newSearchCode.LastIndexOf(separationCharacter) > 0) {
                    newSearchCode = searchCode.Substring(0, searchCode.LastIndexOf(separationCharacter));
                    if (searchCode.Contains("-")) {
                        newSearchCode = newSearchCode + searchCode.Substring(searchCode.IndexOf("-"));
                    }
                    food = null;
                    if (_allFoods.TryGetValue(newSearchCode, out food)) {
                        fcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                            Step = FoodConversionStepType.Supertype,
                            FoodCodeFrom = searchCode,
                            FoodCodeTo = food.Code,
                            Finished = false,
                        });
                        fcr.FoodTrace.Add(food);
                        found = true;
                        var newFcr = new FoodConversionResult(fcr);
                        processSteps(food, newSearchCode, newFcr, conversionResults);
                        break;
                    }
                    searchCode = newSearchCode;
                }
            }
            return found;
        }

        /// <summary>
        /// STEP 5b: (Hierarchy SuperType Link)
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool HierarchySuperTypeLink(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            if (food == null || (food.FoodFacets?.Count > 0) || food.Parent == null) {
                return false;
            }
            var parent = food.Parent;
            var newFcr = new FoodConversionResult(fcr);
            newFcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                Step = FoodConversionStepType.HierarchySupertype,
                FoodCodeFrom = searchCode,
                FoodCodeTo = parent.Code,
                Finished = false,
            });
            newFcr.FoodTrace.Add(food);
            processSteps(parent, parent.Code, newFcr, conversionResults);
            return true;
        }

        /// <summary>
        /// STEP 6: (Default Processing Factor)
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool DefaultProcessingFactor(
            Food food,
            string searchCode,
            FoodConversionResult fcr,
            List<FoodConversionResult> conversionResults
        ) {
            if (food == null) {
                return false;
            }
            bool found = false;
            var defaultCode = string.Empty;
            if (searchCode.Contains("-")) {
                var processingTypeCode = searchCode.Substring(searchCode.IndexOf("-") + 1);
                ProcessingType processingType = null;
                _ = _processingTypes?.TryGetValue(processingTypeCode, out processingType) ?? false;
                defaultCode = searchCode.Remove(searchCode.IndexOf("-"));
                _allFoods.TryGetValue(defaultCode, out food);
                fcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                    Step = FoodConversionStepType.DefaultProcessing,
                    FoodCodeFrom = searchCode,
                    FoodCodeTo = food?.Code ?? string.Empty,
                    Finished = false,
                    ProcessingTypes = processingType != null ? new List<ProcessingType>() { processingType } : null
                });
                fcr.FoodTrace.Add(food);
                var newFcr = new FoodConversionResult(fcr);
                processSteps(food, defaultCode, newFcr, conversionResults);
                found = true;
            }
            return found;
        }

        /// <summary>
        /// STEP 6b: (Composite Facet Processing Factor)
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool CompositeFacetProcessingFactor(
            Food food,
            string searchCode,
            FoodConversionResult fcr,
            List<FoodConversionResult> conversionResults
        ) {
            if (food == null) {
                return false;
            }
            bool found = false;
            if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(searchCode)) {
                var processingTypeCode = searchCode.Substring(searchCode.IndexOf("#") + 1);
                ProcessingType processingType = null;
                var matchesProcessingType = _processingTypes?.TryGetValue(processingTypeCode, out processingType) ?? false;
                if (matchesProcessingType) {
                    var baseCode = FoodCodeUtilities.GetFoodEx2BaseCode(searchCode);
                    _allFoods.TryGetValue(baseCode, out var baseFood);
                    fcr.ConversionStepResults.Add(
                        new FoodConversionResultStep() {
                            Step = FoodConversionStepType.CompositeFacetProcessing,
                            FoodCodeFrom = searchCode,
                            FoodCodeTo = baseCode,
                            Finished = false,
                            ProcessingTypes = matchesProcessingType ? new List<ProcessingType>() { processingType } : null
                        }
                    );
                    fcr.FoodTrace.Add(baseFood);
                    processSteps(
                        baseFood,
                        baseCode,
                        new FoodConversionResult(fcr),
                        conversionResults
                    );
                    found = true;
                }
            }
            return found;
        }

        /// <summary>
        /// STEP 6c: Strip all FoodEx 2 facets from the search code
        /// </summary>
        /// <param name="food"></param>
        /// <param name="searchCode"></param>
        /// <param name="fcr"></param>
        /// <param name="conversionResults"></param>
        /// <returns></returns>
        private bool RemoveFacets(Food food, string searchCode, FoodConversionResult fcr, List<FoodConversionResult> conversionResults) {
            var found = false;
            if (FoodCodeUtilities.IsCodeWithFoodEx2Facets(searchCode)) {
                var newSearchCode = FoodCodeUtilities.GetFoodEx2BaseCode(searchCode);
                if (!string.Equals(searchCode, newSearchCode, StringComparison.OrdinalIgnoreCase)) {
                    // Try obtain new food
                    _allFoods.TryGetValue(newSearchCode, out food);
                    fcr.FoodTrace.Add(food);
                    fcr.ConversionStepResults.Add(new FoodConversionResultStep() {
                        Step = FoodConversionStepType.RemoveFacets,
                        FoodCodeFrom = searchCode,
                        FoodCodeTo = newSearchCode,
                        Finished = false,
                    });
                    //Iets toevoegen van facets aan ProcessingTypes
                    var newFcr = new FoodConversionResult(fcr);
                    processSteps(food, newSearchCode, newFcr, conversionResults);
                    found = true;
                }
            }
            return found;
        }
    }
}
