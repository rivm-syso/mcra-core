namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Section containing chart metadata and file locations of (temporary) chart files.
    /// summarizer
    /// </summary>
    public class ChartSummarySection : SummarySection {

        public ChartSummarySection(
            string chartName,
            string chartFileName,
            string titlePath,
            string caption,
            string sectionLabel = null
        ) {
            ChartName = chartName;
            TempFileName = chartFileName;
            TitlePath = titlePath;
            ChartFileExtension = Path.GetExtension(chartFileName).Substring(1);
            Caption = caption;
            SectionLabel = sectionLabel;
        }

        /// <summary>
        /// File name of the chart file that is saved in the temp folder.
        /// </summary>
        public string TempFileName { get; }

        /// <summary>
        /// Name of the chart.
        /// </summary>
        public string ChartName { get; }

        /// <summary>
        /// Caption of the chart.
        /// </summary>
        public string Caption { get; }

        /// <summary>
        /// Name of the table.
        /// </summary>
        public string ChartFileExtension { get; }

        /// <summary>
        /// Path in the output table of contents tree to the section containing the figure.
        /// </summary>
        public string TitlePath { get; }
    }
}
