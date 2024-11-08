using MCRA.General;
using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class FocalCommodityConcentrationScenarioSectionView : SectionView<FocalCommodityConcentrationScenarioSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string> {
                "ConcentrationInputDataRecords",
                "TotalNumberOfSamples",
                "NumberOfDetects",
                "NumberOfCensoredValues",
                "NumberOfCompoundsWithConcentrations",
                "HasAuthorisations",
                "MeasuredCompoundCode",
                "MeasuredCompoundName",
                "Extrapolated",
                "FocalCombination",
                "Record"
            };


            var concentrationsAdjustmentString = string.Empty;
            if (Model.FocalCommodityConcentrationAdjustmentFactor != 1D && Model.FocalCommoditySubstanceOccurrencePercentage != 100) {
                concentrationsAdjustmentString = " (not adjusted with the scenario's occurrence percentage and concentration adjustment factor)";
            } else if (Model.FocalCommodityConcentrationAdjustmentFactor != 1D) {
                concentrationsAdjustmentString = " (not adjusted with the scenario's concentration adjustment factor)";
            } else if (Model.FocalCommoditySubstanceOccurrencePercentage != 100) {
                concentrationsAdjustmentString = " (not adjusted with the scenario's occurrence percentage)";
            }

            sb.AppendDescriptionTable(
                "FocalConcentrationsScenarioSettings",
                Model.SectionId,
                Model,
                ViewBag,
                caption: "Focal concentrations scenario settings.",
                header: false,
                showLegend: false,
                hiddenProperties: hiddenProperties);
            if (Model.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstanceConcentrationsByLimitValue) {
                sb.AppendDescriptionTable(
                    "FocalScenarioSampleStatistics",
                    Model.SectionId,
                    Model.Record,
                    ViewBag,
                    caption: $"Focal commodity sample statistics{concentrationsAdjustmentString}.",
                    header: false,
                    showLegend: false,
                    hiddenProperties: hiddenProperties);
            } else if (Model.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                && Model.ConcentrationInputDataRecords.Count == 1
                && Model.ConcentrationInputDataRecords.Single().TotalCount == 1
            ) {
                sb.AppendDescriptionTable(
                    "FocalScenarioSampleStatistics",
                    Model.SectionId,
                    Model.Record,
                    ViewBag,
                    caption: $"Focal commodity sample statistics{concentrationsAdjustmentString}.",
                    header: false,
                    showLegend: false,
                    hiddenProperties: hiddenProperties);
            } else if (Model.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                && Model.ConcentrationInputDataRecords.Count == 1
                && Model.ConcentrationInputDataRecords.Single().TotalCount > 1
            ) {
                sb.AppendDescriptionTable(
                    "FocalScenarioSampleStatistics",
                    Model.SectionId,
                    Model.ConcentrationInputDataRecords.Single(),
                    ViewBag,
                    caption: $"Focal commodity sample statistics{concentrationsAdjustmentString}.",
                    header: false,
                    showLegend: true,
                    hiddenProperties: hiddenProperties,
                    attributes: null);
            } else if (Model.FocalCommodityReplacementMethod == FocalCommodityReplacementMethod.ReplaceSubstances
                && Model.ConcentrationInputDataRecords.Count > 1
            ) {
                sb.AppendTable(
                         Model,
                         Model.ConcentrationInputDataRecords,
                         "FocalCommoditySamplesByFoodSubstanceTable",
                         ViewBag,
                         caption: $"Focal commodity sample statistics{concentrationsAdjustmentString}.",
                         saveCsv: true,
                         displayLimit: 20,
                         sortable: true,
                         hiddenProperties: hiddenProperties
                     );
            }
        }
    }
}
