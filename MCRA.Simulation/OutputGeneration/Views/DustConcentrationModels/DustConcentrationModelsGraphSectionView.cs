using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class DustConcentrationModelsGraphSectionView : SectionView<DustConcentrationModelsGraphSection> {
        //special case of concentration model image which occurs most redundantly in
        //this graph section view. This image needs only to be created once and cached once
        //in practice this gives a huge performance improvement
        private static string _noPositives100PctCensoredImage = null;

        public override void RenderSectionHtml(StringBuilder sb) {
            var samples = new List<string> { "Dust" };
            var substances = Model.Records
                .Select(c => c.SubstanceName)
                .Distinct()
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            string highLight = string.Empty;
            int take = 6;
            int loopCount = (int)Math.Ceiling(1.0 * substances.Count / take);

            //Render HTML
            var description = "Concentration model graphs by substance.";
            sb.AppendDescriptionParagraph(description);
            for (int i = 0; i < loopCount; i++) {
                sb.Append($@"<table><thead><tr><th></th>");
                foreach (var substance in substances.Skip(i * take).Take(take)) {
                    highLight = substance.StartsWith("_") ? "class='highlight'" : string.Empty;
                    sb.Append($"<th {highLight}>{substance.ToHtml()}</th>");
                }
                sb.Append("</tr></thead><tbody>");

                foreach (var sample in samples) {
                    sb.Append($"<tr><td>{sample.ToHtml()}</td>");
                    foreach (var substance in substances.Skip(i * take).Take(take)) {
                        highLight = substance.StartsWith("_") ? "class='highlight'" : string.Empty;
                        sb.Append($"<td {highLight}>");
                        var concModelRecord = Model.Records
                            .FirstOrDefault(c => c.SubstanceName == substance);
                        if (concModelRecord != null && concModelRecord.HasMeasurements) {
                            //special case: (create and) use cached image
                            if (concModelRecord.IsEmpiricalWithNoPositives100PctCensored) {
                                //create the image or use the cached value
                                if (string.IsNullOrEmpty(_noPositives100PctCensoredImage)) {
                                    var sbCensoredValues = new StringBuilder();
                                    sbCensoredValues.AppendBase64Image(new DustConcentrationModelChartCreator(concModelRecord, 120, 160, true));
                                    _noPositives100PctCensoredImage = sbCensoredValues.ToString();
                                    sbCensoredValues = null;
                                }
                                sb.Append(_noPositives100PctCensoredImage);
                            } else {
                                //always create a new image in this case
                                sb.AppendBase64Image(new DustConcentrationModelChartCreator(concModelRecord, 120, 160, true));
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
