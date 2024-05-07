using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.RiskCalculation;
using MCRA.Simulation.Calculators.RiskPercentilesCalculation;
using MCRA.Simulation.Calculators.SingleValueRisksCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.SingleValueRisks {

    [ActionType(ActionType.SingleValueRisks)]
    public sealed class SingleValueRisksActionCalculator : ActionCalculatorBase<SingleValueRisksActionResult> {

        public SingleValueRisksActionCalculator(ProjectDto project) : base(project) {
            var isComputeFromIndividualRisks = project != null
                ? project.RisksSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks
                : false;
            _actionInputRequirements[ActionType.Risks].IsVisible = isComputeFromIndividualRisks;
            _actionInputRequirements[ActionType.Risks].IsRequired = isComputeFromIndividualRisks;
            _actionInputRequirements[ActionType.SingleValueDietaryExposures].IsVisible = !isComputeFromIndividualRisks;
            _actionInputRequirements[ActionType.SingleValueDietaryExposures].IsRequired = !isComputeFromIndividualRisks;
            _actionInputRequirements[ActionType.HazardCharacterisations].IsVisible = !isComputeFromIndividualRisks;
            _actionInputRequirements[ActionType.HazardCharacterisations].IsRequired = !isComputeFromIndividualRisks;
        }

        public override IActionSettingsManager GetSettingsManager() {
            return new SingleValueRisksSettingsManager();
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.RisksSettings.UseAdjustmentFactors) {
                result.Add(UncertaintySource.SingleValueRiskAdjustmentFactors);
            }
            return result;
        }

        protected override void verify() {

        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new SingleValueRisksSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override SingleValueRisksActionResult run(
            ActionData data, 
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new SingleValueRisksActionResult();

            if (_project.RisksSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromSingleValues) {
                var calculator = new SingleValueRisksCalculator();
                var hazardCharacterisationsCollection = data.HazardCharacterisationModelsCollections.First();
                var hazardCharacterisations = hazardCharacterisationsCollection.HazardCharacterisationModels;
                var hazardCharacterisationUnit = hazardCharacterisationsCollection.TargetUnit;
                result.SingleValueRiskEstimates = calculator
                    .Compute(
                        data.SingleValueDietaryExposureResults,
                        hazardCharacterisations,
                        data.SingleValueDietaryExposureUnit,
                        hazardCharacterisationUnit
                    );
            } else {
                result = getSingleValueIndividualRisks(data.CumulativeIndividualEffects);
                //Hit summarizer settings
                _ = _project.AssessmentSettings.FocalCommodity;
                _ = _project.RisksSettings.UseBackgroundAdjustmentFactor;
                if (_project.RisksSettings.UseAdjustmentFactors
                    && _project.RisksSettings.UseBackgroundAdjustmentFactor
                    && _project.AssessmentSettings.FocalCommodity
                    && _project.ConcentrationModelSettings.IsFocalCommodityMeasurementReplacement
                ) {
                    result.FocalCommodityContribution = getFocalCommodityContribution(
                        data.DietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.FocalCommodityCombinations,
                        data.SubstanceConversions,
                        data.DeterministicSubstanceConversionFactors,
                        _project.AssessmentSettings.ExposureType,
                        _project.RisksSettings.RiskMetricType,
                        _project.RisksSettings.Percentage,
                        _project.SubsetSettings.IsPerPerson
                    );
                } else {
                    result.FocalCommodityContribution = 0;
                }
            }
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, SingleValueRisksActionResult result) {
            data.SingleValueRiskCalculationResults = result.SingleValueRiskEstimates;
        }

        protected override void summarizeActionResult(
            SingleValueRisksActionResult result, 
            ActionData data, 
            SectionHeader header, 
            int order, 
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new SingleValueRisksSummarizer();
            summarizer.Summarize(_project, result, data, header, order);
            localProgress.Update(100);
        }

        protected override SingleValueRisksActionResult runUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);
            SingleValueRisksActionResult result = null;
            if (_project.RisksSettings.SingleValueRiskCalculationMethod == SingleValueRiskCalculationMethod.FromIndividualRisks) {
                result = getSingleValueIndividualRisks(
                    data.CumulativeIndividualEffects,
                    true,
                    factorialSet.Contains(UncertaintySource.SingleValueRiskAdjustmentFactors)
                        ? uncertaintySourceGenerators[UncertaintySource.SingleValueRiskAdjustmentFactors]
                        : null
                );
                if (_project.RisksSettings.UseAdjustmentFactors
                    && _project.RisksSettings.UseBackgroundAdjustmentFactor
                    && _project.AssessmentSettings.FocalCommodity
                    && _project.ConcentrationModelSettings.IsFocalCommodityMeasurementReplacement
                ) {
                    result.FocalCommodityContribution = getFocalCommodityContribution(
                        data.DietaryIndividualDayIntakes,
                        data.CorrectedRelativePotencyFactors,
                        data.MembershipProbabilities,
                        data.FocalCommodityCombinations,
                        data.SubstanceConversions,
                        data.DeterministicSubstanceConversionFactors,
                        _project.AssessmentSettings.ExposureType,
                        _project.RisksSettings.RiskMetricType,
                        _project.RisksSettings.Percentage,
                        _project.SubsetSettings.IsPerPerson
                    );
                } else {
                    result.FocalCommodityContribution = 0;
                }
            }
            localProgress.Update(100);
            return result;
        }

        protected override void summarizeActionResultUncertain(
                UncertaintyFactorialSet factorialSet,
                SingleValueRisksActionResult actionResult,
                ActionData data,
                SectionHeader header,
                CompositeProgressState progressReport
            ) {
            var localProgress = progressReport.NewProgressState(100);
            if (actionResult != null) {
                var summarizer = new SingleValueRisksSummarizer();
                summarizer.SummarizeUncertain(_project, actionResult, data, header);
            }
            localProgress.Update(100);
        }

        private SingleValueRisksActionResult getSingleValueIndividualRisks(
            ICollection<IndividualEffect> individualEffects,
            bool uncertainty = false,
            IRandom adjustmentFactorsRandomGenerator = null
        ) {
            var result = new SingleValueRisksActionResult();
            var settings = new IndividualSingleValueRisksCalculatorSettings(_project.RisksSettings);
            var calculator = new RiskDistributionPercentilesCalculator(settings);
            result.SingleValueRiskEstimates = calculator
                .Compute(individualEffects)
                .Select(r => new SingleValueRiskCalculationResult() {
                    Exposure = r.Exposure,
                    HazardCharacterisation = r.HazardCharacterisation,
                    ExposureHazardRatio = r.HazardQuotient,
                    HazardExposureRatio = r.HazardExposureRatio,
                })
                .ToList();

            if (_project.RisksSettings.UseAdjustmentFactors) {
                var exposureSettings = new AdjustmentFactorModelFactorySettings(_project.RisksSettings, isExposure: true);
                var exposureModel = new AdjustmentFactorModelFactory(exposureSettings);
                var exposureAdjustmentFactorModel = exposureModel.Create();

                var hazardSettings = new AdjustmentFactorModelFactorySettings(_project.RisksSettings, isExposure: false);
                var hazardModel = new AdjustmentFactorModelFactory(hazardSettings);
                var hazardAdjustmentFactorModel = hazardModel.Create();

                if (uncertainty) {
                    result.AdjustmentFactorExposure = exposureAdjustmentFactorModel.DrawFromDistribution(adjustmentFactorsRandomGenerator);
                    result.AdjustmentFactorHazard = hazardAdjustmentFactorModel.DrawFromDistribution(adjustmentFactorsRandomGenerator);
                } else {
                    result.AdjustmentFactorExposure = exposureAdjustmentFactorModel.GetNominal();
                    result.AdjustmentFactorHazard = hazardAdjustmentFactorModel.GetNominal();
                }
            } else {
                result.AdjustmentFactorExposure = 1;
                result.AdjustmentFactorHazard = 1;
            }
            return result;
        }

        private double getFocalCommodityContribution(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<(Food Food, Compound Substance)> focalCommodityCombinations,
            ICollection<SubstanceConversion> substanceConversions,
            ICollection<DeterministicSubstanceConversionFactor> deterministicSubstanceConversions,
            ExposureType exposureType,
            RiskMetricType riskMetricType,
            double percentage,
            bool isPerPerson
        ) {
            if (riskMetricType == RiskMetricType.HazardExposureRatio) {
                percentage = 100 - percentage;
            }

            var calculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperDietaryDayIntakes = calculator.GetUpperIntakes(
                dietaryIndividualDayIntakes,
                relativePotencyFactors,
                membershipProbabilities,
                percentage,
                isPerPerson
            );

            if (!upperDietaryDayIntakes.Any()) {
                return 0;
            }

            var totalExposure = double.NaN;
            var focalCombinationExposure = 0d;
            if (exposureType == ExposureType.Acute) {
                totalExposure = upperDietaryDayIntakes.Sum(r => r.IndividualSamplingWeight * r.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson));
                foreach (var focalCommodityCombination in focalCommodityCombinations) {
                    var food = focalCommodityCombination.Food;
                    var substance = focalCommodityCombination.Substance;
                    var activeSubstances = getActiveSubstancesOfFocalSubstance(relativePotencyFactors, substanceConversions, deterministicSubstanceConversions, substance);
                    foreach (var activeSubstance in activeSubstances) {
                        foreach (var day in upperDietaryDayIntakes) {
                            focalCombinationExposure += day.IntakesPerFood
                                .AsParallel()
                                .Where(c => c.FoodAsMeasured == food)
                                .SelectMany(c => c.IntakesPerCompound)
                                .Where(c => c.Compound == activeSubstance)
                                .Select(c => c.Amount * relativePotencyFactors[activeSubstance] * membershipProbabilities[activeSubstance] / (isPerPerson ? 1 : day.Individual.BodyWeight))
                                .Sum();
                        }
                    }
                }
            } else {
                var upperDietaryDayIntakesGrouped = upperDietaryDayIntakes
                    .GroupBy(c => c.SimulatedIndividualId)
                    .ToList();

                totalExposure = upperDietaryDayIntakesGrouped
                    .Select(c => c.Sum(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson) * i.IndividualSamplingWeight) / c.Count())
                    .Sum();

                foreach (var focalCommodityCombination in focalCommodityCombinations) {
                    var food = focalCommodityCombination.Food;
                    var substance = focalCommodityCombination.Substance;
                    var activeSubstances = getActiveSubstancesOfFocalSubstance(relativePotencyFactors, substanceConversions, deterministicSubstanceConversions, substance);
                    foreach (var activeSubstance in activeSubstances) {
                        foreach (var ind in upperDietaryDayIntakesGrouped) {
                            focalCombinationExposure += ind
                                .AsParallel()
                                .SelectMany(c => c.IntakesPerFood)
                                .Where(c => c.FoodAsMeasured == food)
                                .SelectMany(c => c.IntakesPerCompound)
                                .Where(c => c.Compound == activeSubstance)
                                .Select(c => c.Amount * relativePotencyFactors[activeSubstance] * membershipProbabilities[activeSubstance] / (isPerPerson ? 1 : ind.First().Individual.BodyWeight))
                                .Sum() / ind.Count();
                        }
                    }
                }
            }

            return focalCombinationExposure / totalExposure;
        }

        private static HashSet<Compound> getActiveSubstancesOfFocalSubstance(
            IDictionary<Compound, double> relativePotencyFactors,
            ICollection<SubstanceConversion> substanceConversions,
            ICollection<DeterministicSubstanceConversionFactor> deterministicSubstanceConversions,
            Compound substance
        ) {
            var activeSubstances = new HashSet<Compound>();
            if (relativePotencyFactors.ContainsKey(substance)) {
                activeSubstances.Add(substance);
            }

            var deterministicConversionSubstances = deterministicSubstanceConversions?
                .Where(r => r.MeasuredSubstance == substance && relativePotencyFactors.ContainsKey(r.ActiveSubstance))
                .Select(r => r.ActiveSubstance)
                .ToList();
            if (deterministicConversionSubstances?.Any() ?? false) {
                activeSubstances.UnionWith(deterministicConversionSubstances);
            }
            var conversionSubstances = substanceConversions?
                .Where(r => r.MeasuredSubstance == substance && relativePotencyFactors.ContainsKey(r.ActiveSubstance))
                .Select(r => r.ActiveSubstance)
                .ToList();
            if (conversionSubstances?.Any() ?? false) {
                activeSubstances.UnionWith(conversionSubstances);
            }
            return activeSubstances;
        }
    }
}
