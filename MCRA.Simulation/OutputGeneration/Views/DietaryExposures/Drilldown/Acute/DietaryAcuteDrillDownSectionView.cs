using System.Text;
using MCRA.Simulation.OutputGeneration.Helpers;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DietaryAcuteDrillDownSectionView : SectionView<DietaryAcuteDrillDownSection> {
        public override void RenderSectionHtml(StringBuilder sb) {

            var hiddenPropertiesOverall = new List<string>();

            var processingCalculation = string.Empty;
            var processingCorrectionFactor = string.Empty;
            var descriptionDetailed = string.Empty;

            if (Model.IsProcessing) {
                processingCalculation = "* Processing factor / Processing correction factor";
                processingCorrectionFactor = " .The processing correction factor e.g for a dried food with a consumption " +
                    "of 100 gram which is translated to 300 gram raw agricultural commodity, is 3.";
            }
            if (Model.IsUnitVariability) {
                descriptionDetailed = $"Exposure (= Portion amount * Concentration in portion {processingCalculation} / body weight){processingCorrectionFactor}";
            } else {
                descriptionDetailed = $"Exposure (= Portion amount * Concentration in portion {processingCalculation} / body weight)";
            }
            var equivalents = Model.IsCumulative ? " equivalents" : "";

            //Render HTML
            var description = $"Drilldown of {Model.OverallIndividualDayDrillDownRecords.Count} individual days " +
                $"around {Model.VariabilityDrilldownPercentage} % ({Model.PercentileValue:G3} {ViewBag.GetUnit("IntakeUnit").ToHtml()}) " +
                $"of exposure distribution ({Model.ReferenceCompoundName.ToHtml()}{equivalents})";

            //DietaryAcuteDrillDownIndividualsSection
            sb.AppendTable(
                Model,
                Model.OverallIndividualDayDrillDownRecords,
                "DietaryAcuteDrillDownIndividualsSectionTable",
                ViewBag,
                caption: description,
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenPropertiesOverall
            );


            //DietaryAcuteDrillDownDetailSection
            for (int i = 0; i < Model.OverallIndividualDayDrillDownRecords.Count; i++) {
                var hiddenPropertiesDetailed = new List<string>();
                var hiddenPropertiesSubstances = new List<string>();
                var hiddenPropertiesFoods = new List<string>();
                var item = Model.OverallIndividualDayDrillDownRecords[i];
                if (item.DietaryExposure > 0) {
                    sb.Append($"<h3>Drilldown {i + 1}</h3>");
                    var descriptionIndividual = $"Individual {item.IndividualId.ToHtml()}, day {item.Day.ToHtml()}, " +
                        $"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}, " +
                        $"sampling weight: {item.SamplingWeight:F2}";
                    ;
                    //DietaryAcuteDrillDownDetailSection
                    if (Model.DetailedIndividualDayDrillDownRecords.TryGetValue(item.SimulatedIndividualDayId, out var detailedRecords)) {
                        var showRpf = detailedRecords.Any(c => !double.IsNaN(c.Rpf) && c.Rpf != 1D);
                        sb.Append("<h4>Per consumed portion per substance</h4>");
                        if (Model.IsProcessing) {
                            sb.AppendParagraph($"Exposure = portion amount * concentration in portion * processing factor / processing correction factor / {item.BodyWeight} (= body weight)");
                        } else {
                            sb.AppendParagraph($"Exposure = portion amount * concentration in portion  / {item.BodyWeight} (= body weight)");
                        }
                        if (showRpf) {
                            sb.AppendParagraph($"{Model.ReferenceCompoundName.ToHtml()} equivalents exposure = RPF * exposure");
                        }
                        //if (item.OthersIntakePerMassUnit > 0) {
                        //    sb.AppendParagraph("The summary below refers to the exposure due to riskdrivers");
                        //}

                        if (!Model.IsUnitVariability) {
                            hiddenPropertiesDetailed.Add("UnitWeight");
                            hiddenPropertiesDetailed.Add("UnitsInCompositeSample");
                            hiddenPropertiesDetailed.Add("ConcentrationInSample");
                            hiddenPropertiesDetailed.Add("VariabilityFactor");
                            hiddenPropertiesDetailed.Add("StochasticVf");
                        }
                        if (!Model.IsProcessing) {
                            hiddenPropertiesDetailed.Add("ProcessingFactor");
                            hiddenPropertiesDetailed.Add("ProcessingCorrectionFactor");
                        }
                        if (!Model.IsUnitVariability && !Model.IsProcessing) {
                            hiddenPropertiesDetailed.Add("ProcessingTypeDescription");
                        }
                        if (!showRpf) {
                            hiddenPropertiesDetailed.Add("Rpf");
                            hiddenPropertiesDetailed.Add("EquivalentExposure");
                            hiddenPropertiesDetailed.Add("Percentage");
                        }
                        sb.AppendTable(
                            Model,
                            detailedRecords,
                            $"DietaryAcuteDrillDownDetailSectionTable-{item.SimulatedIndividualDayId}",
                            ViewBag,
                            caption: descriptionIndividual,
                            saveCsv: true,
                            header: true,
                            displayLimit: 20,
                            hiddenProperties: hiddenPropertiesDetailed
                        );
                    }

                    //DietaryAcuteDrillDownCompoundSection
                    if (Model.IndividualSubstanceDrillDownRecords.TryGetValue(item.SimulatedIndividualDayId, out var substanceRecords)) {
                        hiddenPropertiesSubstances.Add("Day");
                        if (!Model.IsCumulative) {
                            hiddenPropertiesSubstances.Add("Rpf");
                            hiddenPropertiesSubstances.Add("EquivalentExposure");
                        }
                        sb.Append("<h4>Per substance</h4>");
                        var descriptionSubstance = $"Exposure {Model.ReferenceCompoundName}{equivalents} = exposure * relative potency factor" +
                            $"body weight: {item.BodyWeight} {ViewBag.GetUnit("BodyWeightUnit").ToHtml()}";
                        if (substanceRecords.Count > 1 && substanceRecords.Count(c => c.ExposurePerDay > 0) > 1) {
                            var chartCreator = new DietaryAcuteCompoundPieChartCreator(substanceRecords, item.SimulatedIndividualDayId);
                            sb.AppendChart(
                                $"DietaryAcuteSubstancePieChart-{item.SimulatedIndividualDayId}",
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
                            $"DietaryAcuteSubstanceSectionTable-{item.SimulatedIndividualDayId}",
                            ViewBag,
                            caption: descriptionSubstance,
                            saveCsv: true,
                            header: true,
                            displayLimit: 20,
                            hiddenProperties: hiddenPropertiesSubstances
                        );
                    }

                    //DietaryAcuteDrillDownFoodAsMeasuredSection
                    if (Model.IndividualModelledFoodDrillDownRecords.TryGetValue(item.SimulatedIndividualDayId, out var modelledFoodRecords)) {
                        sb.Append("<h4>Per modelled food</h4>");
                        hiddenPropertiesFoods.Add("Day");
                        var descriptionModelledFood = $"Intake = consumption modelled food * {Model.ReferenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight)";
                        hiddenPropertiesFoods.Add("Day");
                        if (modelledFoodRecords != null) {
                            if (modelledFoodRecords.Count(c => c.Exposure > 0) > 1) {
                                var chartCreator = new DietaryAcuteFoodAsMeasuredPieChartCreator(modelledFoodRecords, item.SimulatedIndividualDayId);
                                sb.AppendChart(
                                    $"DietaryAcuteModelledFoodPieChart-{item.SimulatedIndividualDayId}",
                                    chartCreator,
                                    ChartFileType.Svg,
                                    Model,
                                    ViewBag,
                                    "Total exposure per body weight/day for modelled foods",
                                    true
                                );
                            }
                            sb.AppendTable(
                                Model,
                                modelledFoodRecords,
                                $"DietaryAcuteModelledFoodSectionTable-{item.SimulatedIndividualDayId}",
                                ViewBag,
                                caption: descriptionModelledFood,
                                saveCsv: true,
                                header: true,
                                displayLimit: 20,
                                hiddenProperties: hiddenPropertiesFoods
                            );
                        }
                    }

                    //DietaryAcuteDrillDownFoodAsEatenSection
                    if (Model.IndividualFoodAsEatenDrillDownRecords.TryGetValue(item.SimulatedIndividualDayId, out var foodRecords)) {
                        sb.Append("<h4>Per food as eaten</h4>");
                        var descriptionFoodAsEaten = $"Intake = consumption food as eaten * {Model.ReferenceCompoundName}{equivalents} / {item.BodyWeight} (= body weight)";
                        if (foodRecords != null) {
                            if (foodRecords.Count(c => c.Exposure > 0) > 1) {
                                var chartCreator = new DietaryAcuteFoodAsMeasuredPieChartCreator(foodRecords, item.SimulatedIndividualDayId);
                                //var chartCreator = new DietaryAcuteFoodAsEatenPieChartCreator(foodRecords, index);
                                sb.AppendChart(
                                    $"DietaryAcuteFoodAsEatenPieChart-{item.SimulatedIndividualDayId}",
                                    chartCreator,
                                    ChartFileType.Svg,
                                    Model,
                                    ViewBag,
                                    "Total exposure per body weight/day for foods as eaten",
                                    true
                                );
                            }
                            sb.AppendTable(
                                Model,
                                foodRecords,
                                $"DietaryAcuteFoodAsEatenSectionTable-{item.SimulatedIndividualDayId}",
                                ViewBag,
                                caption: descriptionFoodAsEaten,
                                saveCsv: true,
                                header: true,
                                displayLimit: 20,
                                hiddenProperties: hiddenPropertiesFoods

                            );
                        }
                    }
                } else {
                    sb.AppendParagraph($"For individual: {item.IndividualId}, day {item.Day} no exposures available");
                }
            }
        }
    }


}
