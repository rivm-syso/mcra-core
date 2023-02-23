using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {

    public class HazardExposureSectionView : SectionView<HazardExposureSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            var lowerPercentage = (100 - Model.ConfidenceInterval) / 2;
            var pLower = $"p{lowerPercentage:F1}";
            var pUpper = $"p{(100 - (100 - Model.ConfidenceInterval) / 2):F1}";

            var hasHcVariability = Model.HazardExposureRecords?.Any(r => r.LowerHc != r.UpperHc) ?? false;
            var hasUncertainty = Model.HasUncertainty;

            if (!hasUncertainty) {
                hiddenProperties.Add("LowerRisk_UncLower");
                hiddenProperties.Add("UpperRisk_UncUpper");
                hiddenProperties.Add("LowerHc_UncLower");
                hiddenProperties.Add("UpperHc_UncUpper");
                hiddenProperties.Add("LowerExposure_UncLower");
                hiddenProperties.Add("UpperExposure_UncUpper");
                hiddenProperties.Add("MedianAllRisk_UncMedian");
                hiddenProperties.Add("LowerAllRisk_UncMedian");
                hiddenProperties.Add("UpperAllRisk_UncMedian");
            } else {
                hiddenProperties.Add("MedianAllRisk");
                hiddenProperties.Add("LowerAllRisk");
                hiddenProperties.Add("UpperAllRisk");
            }
            if (!hasHcVariability) {
                hiddenProperties.Add("LowerHc");
                hiddenProperties.Add("UpperHc");
                hiddenProperties.Add("MedianHc");
                hiddenProperties.Add("LowerHc_UncLower");
                hiddenProperties.Add("UpperHc_UncUpper");
            }
            hiddenProperties.Add("MeanExposure");
            hiddenProperties.Add("StDevExposure");
            hiddenProperties.Add("MeanHc");
            hiddenProperties.Add("StDevHc");
            hiddenProperties.Add("LowerRisk");
            hiddenProperties.Add("MedianRisk");
            hiddenProperties.Add("UpperRisk");

            //Render HTML
            if (Model.HazardExposureRecords.Any(c => c.PercentagePositives < lowerPercentage)) {
                var numberAbovePercentage = Model.HazardExposureRecords.Where(c => c.PercentagePositives >= lowerPercentage).Count();
                sb.AppendParagraph($"Only substances with > {lowerPercentage} % positive residues are shown ({numberAbovePercentage} out of {Model.HazardExposureRecords.Count}).");
            }

            var panelBuilder = new HtmlTabPanelBuilder();

            panelBuilder.AddPanel(
                id: "A",
                title: "0",
                hoverText: "Traditional approach",
                content: ChartHelpers.Chart(
                    "RiskMatrixTraditionalChart",
                    Model,
                    ViewBag,
                    new HazardExposure_TraditionalChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                    ChartFileType.Svg,
                    true,
                    $"Traditional approach: {pUpper} exposure plotted at hazard characterisation level HC."
                )
            );

            panelBuilder.AddPanel(
                id: "B",
                title: "1a",
                hoverText: "Exposure distribution at hazard level HC/100",
                content: ChartHelpers.Chart(
                    "RiskMatrixExposureDistributionAtFixedHCChart",
                    Model,
                    ViewBag,
                    new HazardExposure_ExpvsCed100ExpChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                    ChartFileType.Svg,
                    true,
                    $"Exposure distribution shown as {pLower} - {pUpper} plotted at hazard level HC."
                )
            );

            if (hasHcVariability) {
                panelBuilder.AddPanel(
                    id: "C",
                    title: "1b",
                    hoverText: $"Exposure distribution at {pLower} of hazard distribution",
                    content: ChartHelpers.Chart(
                        "RiskMatrixExposureDistributionAtLowerHCChart",
                        Model,
                        ViewBag,
                        new HazardExposure_ExpvsLowerCedChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                        ChartFileType.Svg,
                        true,
                        $"Exposure distribution shown as {pLower} - {pUpper} plotted at hazard level {pLower}(HC)."
                    )
                );
            }

            if (hasHcVariability) {
                panelBuilder.AddPanel(
                    id: "D",
                    title: "2",
                    hoverText: $"Hazard characterisation distribution (HC) plotted at exposure level {pUpper}(Exp)",
                    content: ChartHelpers.Chart(
                        "RiskMatrixHazardDistributionAtUpperExpChart",
                        Model,
                        ViewBag,
                        new HazardExposure_CedvsUpperExpChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                        ChartFileType.Svg,
                        true,
                        $"Hazard characterisation distribution shown as {pLower} - {pUpper} plotted at exposure level {pUpper}(Exp)."
                    )
                );
            }

            if (hasHcVariability) {
                panelBuilder.AddPanel(
                    id: "E",
                    title: "3a",
                    hoverText: $"Hazard characterisation distribution at exposure level {pUpper}(Exp) and exposure distribution at hazard level HC",
                    content: ChartHelpers.Chart(
                        "RiskMatrixExposureAndHazardDistributionAtUpperChart",
                        Model,
                        ViewBag,
                        new HazardExposure_CedExpvsCed100ChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                        ChartFileType.Svg,
                        true,
                        $"Hazard characterisation distribution at exposure level {pUpper}(Exp) shown as {pLower} - {pUpper}" +
                        $" and exposure distribution at hazard level HC/100 shown as {pLower} - {pUpper}."
                    )
                );
            }

            if (hasHcVariability) {
                panelBuilder.AddPanel(
                    id: "F",
                    title: "3b",
                    hoverText: $"Hazard characterisation distribution at exposure level {pLower}(Exp)" +
                        $" and exposure distribution at hazard level {pLower}(HC)",
                    content: ChartHelpers.Chart(
                        "RiskMatrixExposureAndHazardDistributionAtLowerChart",
                        Model,
                        ViewBag,
                        new HazardExposure_CedExpvsLowerCedChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                        ChartFileType.Svg,
                        true,
                        $"Hazard characterisation distribution at exposure level {pLower}(Exp) shown as {pLower} - {pUpper}" +
                        $" and exposure distribution at hazard level {pLower}(HC) shown as {pLower} - {pUpper}."
                    )
                );
            }

            panelBuilder.AddPanel(
                id: "G",
                title: "4",
                hoverText: $"Risk distribution plotted through line ({pUpper}Exp, {pLower}HC)",
                content: ChartHelpers.Chart(
                    "RiskMatrixRiskDistributionChart",
                    Model,
                    ViewBag,
                    new HazardExposure_MOEvsUpperExpLowerCedChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                    ChartFileType.Svg,
                    true,
                    $"Risk distribution shown as {pLower} - {pUpper} plotted on line through ({pUpper} Exp, {pLower} HC)."
                )
            );

            panelBuilder.AddPanel(
                id: "H",
                title: "5a",
                hoverText: $"Risk distribution, hazard characterisation distribution (HC), and exposure distribution (Exp) plotted through line ({pUpper} Exp, HC)",
                content: ChartHelpers.Chart(
                    "RiskMatrixHazardExposureRiskDistributionAtUpperChart",
                    Model,
                    ViewBag,
                    new HazardExposure_MOEExpCedvsUpperExpCed100ChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                    ChartFileType.Svg,
                    true,
                    $"Risk distribution shown as  {pLower} - {pUpper}, " +
                    $"hazard characterisation distribution shown as  {pLower} - {pUpper}, and " +
                    $"exposure distribution (Exp) shown as  {pLower} - {pUpper} " +
                    $" plotted on line through ({pUpper} Exp, HC)."
                )
            );

            if (hasHcVariability) {
                panelBuilder.AddPanel(
                    id: "I",
                    title: "5b",
                    hoverText: $"Risk distribution, hazard distribution (HC), and " +
                        $"exposure distribution (Exp) plotted through line ({pUpper} Exp, {pLower} HC)",
                    content: ChartHelpers.Chart(
                        "RiskMatrixHazardExposureRiskDistributionAtUpperExpLowerHcChart",
                        Model,
                        ViewBag,
                        new HazardExposure_MOEExpCedvsUpperExpLowerCedChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                        ChartFileType.Svg,
                        true,
                        $"Risk distribution shown as {pLower} - {pUpper}, " +
                        $"hazard characterisation distribution shown as {pLower} - {pUpper}, and " +
                        $"exposure distribution shown as {pLower} - {pUpper}" +
                        $" plotted on line through ({pUpper} Exp, {pLower} HC)."
                    )
                );
                panelBuilder.AddPanel(
                    id: "J",
                    title: "5c",
                    hoverText: $"Risk distribution, hazard characterisation distribution (HC), and " +
                        $"exposure distribution (Exp) plotted through line (p50 Exp, p50 HC)",
                    content: ChartHelpers.Chart(
                        "RiskMatrixHazardExposureEllipsoidsAtP50Chart",
                        Model,
                        ViewBag,
                        new HazardExposure_EllipsChartCreator(Model, ViewBag.GetUnit("IntakeUnit")),
                        ChartFileType.Svg,
                        true,
                        $"95% bivariate confidence area for hazard characterisation distribution and " +
                        $"exposure distribution shown as ellipsoids plotted on line through (p50(Exp), p50(HC))."
                    )
                );

                panelBuilder.AddPanel(
                    id: "K",
                    title: "5d",
                    hoverText: $"95% bivariate confidence area for hazard characterisation distribution and " +
                        $"exposure distribution shown as ellipsoids, risk distribution, hazard distribution, and " +
                        $"exposure distribution plotted through line (p50(Exp), p50(HC))",
                    content: ChartHelpers.Chart(
                        "RiskMatrixHazardExposureRiskDistributionAndEllipsoidsAtP50Chart",
                        Model,
                        ViewBag,
                        new HazardExposure_EllipsChartCreator(Model, ViewBag.GetUnit("IntakeUnit"), true),
                        ChartFileType.Svg,
                        true,
                        $"95% bivariate confidence area for hazard characterisation distribution and exposure distribution shown as ellipsoids, " +
                        $"risk distribution shown as {pLower} - {pUpper}, " +
                        $"hazard characterisation distribution shown as {pLower} - {pUpper}, " +
                        $"and exposure distribution shown as {pLower} - {pUpper} " +
                        $"plotted on line through (p50(Exp), p50(HC))."
                    )
                );
            }

            panelBuilder.RenderPanel(sb);
            sb.AppendTable(
                Model,
                Model.HazardExposureRecords,
                "CEDExposurePerSubstanceTable",
                ViewBag,
                caption: "Exposure, hazard characterisation and risk statistics by substance.",
                saveCsv: true,
                header: true,
                hiddenProperties: hiddenProperties
            );
        }
    }
}
