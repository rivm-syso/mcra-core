using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ActiveSubstancesSummarySectionView : SectionView<ActiveSubstancesSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hiddenProperties = new List<string>();
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Code))) {
                hiddenProperties.Add("Code");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Name))) {
                hiddenProperties.Add("Name");
            }
            if (Model.Records.All(r => string.IsNullOrEmpty(r.Reference))) {
                hiddenProperties.Add("Reference");
            }
            if (Model.Records.All(r => !r.IsProbabilistic)) {
                hiddenProperties.Add("MembershipScoresMean");
                hiddenProperties.Add("MembershipScoresMedian");
                hiddenProperties.Add("ProbableMembershipsCount");
                hiddenProperties.Add("FractionProbableMemberships");
            }

            //Render HTML
            if (Model.Records.Count == 1) {
                var memberships = Model.Records.First().MembershipProbabilities;
                var membershipsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "AssessmentGroupMemberships", Model, memberships,
                    ViewBag, true, new List<string>()
                );
                var membershipRecords = Model.Records.First().MembershipProbabilities;
                IReportChartCreator membershipsChartCreator = (membershipRecords.Count > 20)
                    ? new ActiveSubstancesDistributionChartCreator(Model)
                    : new ActiveSubstancesChartCreator(Model);

                sb.AppendChart(
                    name: "AssessmentGroupMembershipsChart",
                    chartCreator: membershipsChartCreator,
                    fileType: ChartFileType.Svg,
                    section: Model,
                    viewBag: ViewBag,
                    caption: "Assessment group memberships.",
                    saveChartFile: true,
                    chartData: membershipsDataSection
                );

                sb.AppendTable(
                    Model,
                    Model.Records,
                    "ComputedAssessmentGroupMembershipModelTable",
                    ViewBag,
                    caption: "Computed assessment group memberships.",
                    saveCsv: false,
                    header: true,
                    hiddenProperties: hiddenProperties
                );
            } else if (Model.Records.Count > 1) {
                sb.AppendTable(
                    Model,
                    Model.Records,
                    "AssessmentGroupMembershipModelRecordsTable",
                    ViewBag,
                    caption: "Assessment group memberships.",
                    saveCsv: true,
                    header: true,
                    hiddenProperties: hiddenProperties
                );

                var availableMembershipsDataSection = DataSectionHelper.CreateCsvDataSection(
                    "AvailableMembershipsTable",
                    Model,
                    ViewBag,
                    (r) => Model.WriteCsv(r)
                );
                sb.Append(ChartHelpers.RenderSvg(new ActiveSubstanceModelsHeatmapChartCreator(Model), availableMembershipsDataSection));
            }
        }
    }
}
