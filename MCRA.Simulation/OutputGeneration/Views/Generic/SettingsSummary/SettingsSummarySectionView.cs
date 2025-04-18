﻿using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class SettingsSummarySectionView : SectionView<SettingsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var hasDataSourceSummaryRecords = Model.DataSourceSummaryRecords?.Count > 0;
            if (hasDataSourceSummaryRecords) {
                sb.AppendTable(
                    section: Model,
                    items: Model.DataSourceSummaryRecords,
                    name: Model.SectionId,
                    viewBag: ViewBag,
                    sortable: false
                );
            }
            var settingsSummaryRecords = Model.SummaryRecords?.Count > 0;
            if (settingsSummaryRecords) {
                sb.AppendTable(
                    section: Model,
                    items: Model.SummaryRecords,
                    name: Model.SectionId,
                    viewBag: ViewBag,
                    header: false,
                    sortable: false,
                    tableClasses: ["settings-table"]
                );
            }
        }
    }
}
