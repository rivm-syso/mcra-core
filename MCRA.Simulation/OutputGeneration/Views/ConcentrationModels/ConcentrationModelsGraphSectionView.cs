using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ConcentrationModelsGraphSectionView : SectionView<ConcentrationModelsGraphSection> {
        //special case of concentration model image which occurs most redundantly in
        //this graph section view. This image needs only to be created once and cached once
        //in practice this gives a huge performance improvement
        private static string _noPositives100PctCensoredImage = null;

        public override void RenderSectionHtml(StringBuilder sb) {
            var foodsAsMeasured = Model.ConcentrationModelRecords.Select(c => c.FoodName).Distinct().OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToList();
            var substances = Model.ConcentrationModelRecords.Select(c => c.CompoundName).Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
            string highLight = string.Empty;
            int take = 6;
            int loopCount = (int)Math.Ceiling(1.0 * substances.Count / take);

            //Render HTML
            var description = "Concentration model graphs by food and substance."
                + "Note that this section only shows the models for the food-substance combinations with measurements.";
            sb.AppendDescriptionParagraph(description);
            for (int i = 0; i < loopCount; i++) {
                sb.Append($@"<table><thead><tr><th></th>");
                foreach (var substance in substances.Skip(i * take).Take(take)) {
                    highLight = substance.StartsWith("_") ? "class='highlight'" : string.Empty;
                    sb.Append($"<th {highLight}>{substance.ToHtml()}</th>");
                }
                sb.Append("</tr></thead><tbody>");

                foreach (var food in foodsAsMeasured) {
                    sb.Append($"<tr><td>{food.ToHtml()}</td>");
                    foreach (var substance in substances.Skip(i * take).Take(take)) {
                        highLight = substance.StartsWith("_") ? "class='highlight'" : string.Empty;
                        sb.Append($"<td {highLight}>");
                        var concModelRecord = Model.ConcentrationModelRecords
                            .FirstOrDefault(c => c.FoodName == food && c.CompoundName == substance);
                        if (concModelRecord != null && concModelRecord.HasMeasurements) {
                            //special case: (create and) use cached image
                            if (concModelRecord.IsEmpiricalWithNoPositives100PctCensored) {
                                //create the image or use the cached value
                                if (string.IsNullOrEmpty(_noPositives100PctCensoredImage)) {
                                    var sbCensoredValues = new StringBuilder();
                                    sbCensoredValues.AppendBase64Image(new ConcentrationModelChartCreator(concModelRecord, 120, 160, true));
                                    _noPositives100PctCensoredImage = sbCensoredValues.ToString();
                                    sbCensoredValues = null;
                                }
                                sb.Append(_noPositives100PctCensoredImage);
                            } else {
                                //always create a new image in this case
                                sb.AppendBase64Image(new ConcentrationModelChartCreator(concModelRecord, 120, 160, true));
                            }
                        } else {
                            sb.Append("<div class='no_measurements'>No measurements</div>");
                        }
                        sb.Append("</td>");
                    }
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
            }
        }
    }
}
