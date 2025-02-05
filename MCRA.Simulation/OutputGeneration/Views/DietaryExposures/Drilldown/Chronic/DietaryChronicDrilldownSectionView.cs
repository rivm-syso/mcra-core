using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryChronicDrillDownSectionView : SectionView<DietaryChronicDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenPropertiesOverall = new List<string>();

            string label = Model.IsOIM ? " the Observed individual mean " : " the model assisted ";
            var equivalents = Model.IsCumulative ? " equivalents" : " ";
            //Render HTML
            var description = $"Drilldown {Model.OverallIndividualDrillDownRecords.Count} individual days around " +
                $"{Model.VariabilityDrilldownPercentage} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                $"of {label} exposure distribution.";

            //DietaryChronicDrillDownIndividualsSection
            if (Model.IsOIM) {
                hiddenPropertiesOverall.Add("FrequencyPrediction");
                hiddenPropertiesOverall.Add("AmountPrediction");
                hiddenPropertiesOverall.Add("MeanTransformedIntake");
                hiddenPropertiesOverall.Add("ShrinkageFactor");
                hiddenPropertiesOverall.Add("ModelAssistedExposure");
            }
            if (Model.OverallIndividualDrillDownRecords.All(c => c.Cofactor == string.Empty)) {
                hiddenPropertiesOverall.Add("Cofactor");
            }
            if (Model.OverallIndividualDrillDownRecords.All(c => c.Covariable == 0)) {
                hiddenPropertiesOverall.Add("Covariable");
            }
            sb.AppendTable(
                Model,
                Model.OverallIndividualDrillDownRecords,
                "DietaryChronicDrillDownIndividualsSectionTable",
                ViewBag,
                caption: description,
                header: true,
                saveCsv: true,
                hiddenProperties: hiddenPropertiesOverall
            );

            for (int i = 0; i < Model.OverallIndividualDrillDownRecords.Count; i++) {
                var hiddenPropertiesDetailed = new List<string>();
                var hiddenPropertiesSubstances = new List<string>();
                var item = Model.OverallIndividualDrillDownRecords[i];
                if (item.TotalIntake > 0) {
                    sb.Append($"<h4>Drilldown {i + 1}</h4>");
                    var processingCalculation = Model.IsProcessing ? "* Processing factor / Processing correction factor" : "";
                    sb.Append($"Exposure per day = portion amount * concentration {processingCalculation.ToLower()} / {item.BodyWeight} (= body weight)");
                    var descriptionIndividual = $"Individual {item.IndividualId.ToHtml()}, body weight: {item.BodyWeight} " +
                        $"{ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, sampling weight: {item.SamplingWeight:F2}" +
                        $" observed individual mean: {item.ObservedIndividualMean:G3} {ViewBag.GetUnit("IntakeUnit")}";

                    var showRpf = Model.DetailedIndividualDrillDownRecords
                        .SelectMany(c => c.Value)
                        .Any(r => !double.IsNaN(r.Rpf) && r.Rpf != 1d);

                    //DietaryChronicDrillDownDetailSection
                    if (Model.DetailedIndividualDrillDownRecords.TryGetValue(item.SimulatedIndividualId, out var detailedRecords)) {
                        if (!Model.IsOIM!) {
                            //sb.AppendParagraph($"model assisted exposure: {individualDrillDown.ModelAssistedIntake:G3} {ViewBag.GetUnit("IntakeUnit")}");
                        }
                        if (!showRpf) {
                            hiddenPropertiesDetailed.Add("Rpf");
                            hiddenPropertiesDetailed.Add("EquivalentExposure");
                            hiddenPropertiesDetailed.Add("Percentage");
                        }
                        if (item.TotalIntake > 0) {
                            if (!Model.IsProcessing) {
                                hiddenPropertiesDetailed.Add("ProcessingFactor");
                                hiddenPropertiesDetailed.Add("ProcessingCorrectionFactor");
                                hiddenPropertiesDetailed.Add("ProcessingTypeDescription");
                            }
                            sb.AppendTable(
                                Model,
                                detailedRecords,
                                $"DietaryIndividualIntakeDrillDownTable{item.SimulatedIndividualId}",
                                ViewBag,
                                caption: descriptionIndividual,
                                saveCsv: true,
                                header: true,
                                displayLimit: 20,
                                hiddenProperties: hiddenPropertiesDetailed
                            );
                        }
                    }

                    //DietaryChronicDrillDownCompoundSection
                    if (Model.IndividualSubstanceDrillDownRecords.TryGetValue(item.SimulatedIndividualId, out var substanceRecords)) {
                        sb.Append("<h4>Per substance</h4>");
                        var descriptionSubstance = $"Exposure {Model.ReferenceCompoundName}{equivalents} = exposure * relative potency factor" +
                            $"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}";
                        var uniqueSubstanceNameCount = substanceRecords.Select(dd => dd.SubstanceName)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count();
                        if (!showRpf) {
                            hiddenPropertiesSubstances.Add("Rpf");
                            hiddenPropertiesSubstances.Add("EquivalentExposure");
                        }
                        if (uniqueSubstanceNameCount > 1) {
                            var chartCreator = new DietaryChronicCompoundPieChartCreator(substanceRecords, item.SimulatedIndividualId);
                            sb.AppendChart(
                                $"DietaryChronicSubstancePieChart{item.SimulatedIndividualId}",
                                chartCreator,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                chartCreator.Title,
                                true
                            );
                        }
                        sb.AppendTable(
                            Model,
                            substanceRecords,
                            $"DietaryAcuteSubstanceSectionTable{item.SimulatedIndividualId}",
                            ViewBag,
                            caption: descriptionSubstance,
                            saveCsv: true,
                            header: true,
                            displayLimit: 20,
                            hiddenProperties: hiddenPropertiesSubstances
                        );
                    }

                    //DietaryChronicDrillDownFoodAsMeasuredSection
                    if (Model.IndividualModelledFoodDrillDownRecords.TryGetValue(item.SimulatedIndividualId, out var modelledFoodRecords)) {
                        sb.Append("<h3>Per modelled food</h3>");
                        var descriptionModelledFoods = $"Exposure per day= consumption modelled food * {Model.ReferenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight).";
                        var uniqueModelledFoodNameCount = modelledFoodRecords.Select(dd => dd.FoodName)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count();

                        if (uniqueModelledFoodNameCount > 1) {
                            var chartCreator = new DietaryChronicModelledFoodPieChartCreator(modelledFoodRecords, item.SimulatedIndividualId);
                            sb.AppendChart(
                                $"DietaryChronicModelledFoodPieChart{item.SimulatedIndividualId}",
                                chartCreator,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                chartCreator.Title,
                                true
                            );
                        }
                        sb.AppendTable(
                            Model,
                            modelledFoodRecords,
                            $"DietaryChronicModelledFoodTable{item.SimulatedIndividualId}",
                            ViewBag,
                            caption: descriptionModelledFoods,
                            saveCsv: true,
                            header: true,
                            displayLimit: 20,
                            hiddenProperties: null
                        );
                    }

                    //DietaryChronicDrillDownFoodAsEatenSection
                    if (Model.IndividualFoodAsEatenDrillDownRecords.TryGetValue(item.SimulatedIndividualId, out var foodRecords)) {
                        sb.Append("<h3>Per food as eaten</h3>");
                        var descriptionFoods = $"Exposure per day= consumption food as eaten * {Model.ReferenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight).";
                        var uniqueFoodNameCount = foodRecords.Select(dd => dd.FoodName)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .Count();

                        if (uniqueFoodNameCount > 1) {
                            var chartCreator = new DietaryChronicFoodAsEatenPieChartCreator(foodRecords, item.SimulatedIndividualId);
                            sb.AppendChart(
                                $"DietaryChronicFoodAsEatenPieChart{item.SimulatedIndividualId}",
                                chartCreator,
                                ChartFileType.Svg,
                                Model,
                                ViewBag,
                                chartCreator.Title,
                                true
                            );
                        }
                        sb.AppendTable(
                            Model,
                            foodRecords,
                            $"DietaryChronicFoodAsEatenTable{item.SimulatedIndividualId}",
                            ViewBag,
                            caption: descriptionFoods,
                            saveCsv: true,
                            header: true,
                            displayLimit: 20,
                            hiddenProperties: null
                        );
                    }

                } else {
                    sb.AppendParagraph($"For individual: {item.IndividualId} no exposures available");
                }
            }
        }
    }
}

