using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders;
using MCRA.Utils.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Xml;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {

    /// <summary>
    /// Base class for summary section tests.
    /// </summary>
    public abstract class SectionTestBase {

        private static readonly string _sectionOutputPath =
            Path.Combine(TestUtilities.TestOutputPath, "SummarySections");

        /// <summary>
        /// Creates the summary section tests output folder that is used when rendering views
        /// to the test output folder.
        /// </summary>
        /// <param name="_"></param>
        [AssemblyInitialize]
        public static void MyTestInitialize(TestContext _) {
            if (!Directory.Exists(_sectionOutputPath)) {
                Directory.CreateDirectory(_sectionOutputPath);
            }
        }

        /// <summary>
        /// Renders the section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        protected string RenderViewHtml(SummarySection section, string viewName = null) {
            var view = SectionViewBuilder.CreateView(section, viewName);
            var sb = new StringBuilder();
            view.RenderSectionHtml(sb);
            var sectionHtml = sb.ToString().Trim();
            var reportHtml = HtmlReportBuilder.Render(sectionHtml);
            return reportHtml;
        }

        /// <summary>
        /// Asserts whether there is a valid view for the provided section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="viewName"></param>
        /// <param name="filename"></param>
        protected void RenderView(SummarySection section, string viewName = null, string filename = null) {
            var html = RenderViewHtml(section, viewName);
            if (!string.IsNullOrEmpty(filename)) {
                var outputPath = Path.Combine(_sectionOutputPath, GetType().Name);
                if (!Directory.Exists(outputPath)) {
                    Directory.CreateDirectory(outputPath);
                }
                File.WriteAllText(Path.Combine(outputPath, filename), html);
            }
        }

        /// <summary>
        /// Asserts whether there is a valid view for the provided section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="viewName"></param>
        protected void AssertIsValidView(SummarySection section, string viewName = null) {
            var html = RenderViewHtml(section, viewName);
            Assert.IsTrue(IsValidXml(html));
        }

        /// <summary>
        /// Checks whether the html string is valid xml.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        protected bool IsValidXml(string html) {
            var xmlDoc = new XmlDocument();
            //the HTML needs to be wrapped in a div tag, XML needs a root node
            var xHtml = $"<div>{html}</div>";
            try {
                xmlDoc.LoadXml(xHtml);
                return true;
            } catch {
                return false;
            }
        }
    }
}
