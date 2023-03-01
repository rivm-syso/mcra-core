using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.Helpers {
    [TestClass]
    public class ReportBuilderTests {

        /// <summary>
        /// Tests render output info section.
        /// </summary>
        [TestMethod]
        public void ReportBuilder_TestRenderOutputInfo() {
            var builder = new ReportBuilder(null);
            var outputInfo = createFakeOutputInfo();
            var html = builder.RenderDisplayReport(outputInfo, false, null);
            Assert.IsTrue(!string.IsNullOrEmpty(html));
        }

        /// <summary>
        /// Tests render output info section.
        /// </summary>
        [TestMethod]
        public void ReportBuilder_TestRenderPartialReport() {
            var builder = new ReportBuilder(null);
            var toc = createFakeSummaryToc();
            var outputInfo = createFakeOutputInfo();
            var html = builder.RenderPartialReport(toc, outputInfo, false, null);
            Assert.IsTrue(!string.IsNullOrEmpty(html));
        }

        /// <summary>
        /// Tests render output info section.
        /// </summary>
        [TestMethod]
        public void ReportBuilder_TestRenderSectionContent() {
            var builder = new ReportBuilder(null);
            var toc = createFakeSummaryToc();
            var html = builder.RenderSectionContent(toc);
            Assert.IsNotNull(html);
        }

        /// <summary>
        /// Test resolve recursive template.
        /// </summary>
        [TestMethod]
        public void ReportBuilder_TestResolveReportTemplate_RecursiveSectionOutletElement() {
            var fakeToc = createFakeSummaryToc();
            var builder = new ReportBuilder(fakeToc);
            Assert.IsNotNull(builder);
            var templateHtml = "<div section-path=\"Title1|*\" header-level=\"2\" section-title=\"New section title\" class=\"section-outlet\"></div>";
            var resolvedHtml = builder.ResolveReportTemplate(templateHtml);
            var sectionHash = fakeToc.SubSectionHeaders.First(r => r.Name == "Title1").SectionHash;
            Assert.IsTrue(resolvedHtml.Contains($"<h2 class=\"sectionHeader\" id=\"{sectionHash}\">New section title</h2>"));
            Assert.IsTrue(resolvedHtml.Contains("<div class=\"section-outlet\" render-recursive=\"true\" section-id=\"10000000-0000-0000-0000-000000000000\" header-level=\"2\"></div>"));
        }

        [TestMethod]
        public void ReportBuilder_TestResolveReportTemplate_SectionPathElement() {
            var fakeToc = createFakeSummaryToc();
            var builder = new ReportBuilder(fakeToc);
            Assert.IsNotNull(builder);
            var sectionPath = new[] { "title1", "TITLEa", "TitleD" };
            var pathValue = string.Join("|", sectionPath);
            var templateHtml = $"<div class='section-outlet' id='asdfsaf' section-path='{pathValue}'></div>";
            var resolvedHtml = builder.ResolveReportTemplate(templateHtml);
            Assert.IsTrue(resolvedHtml.Contains("<div class=\"section\" id=\"asdfsaf\" data-section-id=\"1ad00000-0000-0000-0000-000000000000\">"));
            Assert.IsTrue(resolvedHtml.Contains("<div class=\"section-outlet\" section-id=\"1ad00000-0000-0000-0000-000000000000\">"));
        }

        [TestMethod]
        public void ReportBuilder_TestResolveReportTemplate_SectionLabelElement() {
            var fakeToc = createFakeSummaryToc();
            var builder = new ReportBuilder(fakeToc);
            Assert.IsNotNull(builder);
            var sectionLabel = "sectionLabel1AD";
            var templateHtml = $"<div class='section-outlet' id='asdfsaf' section-label='{sectionLabel}'></div>";
            var resolvedHtml = builder.ResolveReportTemplate(templateHtml);
            Assert.IsTrue(resolvedHtml.Contains("<div class=\"section\" id=\"asdfsaf\" data-section-id=\"1ad00000-0000-0000-0000-000000000000\">"));
            Assert.IsTrue(resolvedHtml.Contains("<div class=\"section-outlet\" section-id=\"1ad00000-0000-0000-0000-000000000000\">"));
        }

        [TestMethod]
        public void ReportBuilder_TestResolveReportTemplate_ChartOutletElement() {
            var fakeToc = createFakeSummaryToc();
            var builder = new ReportBuilder(fakeToc);
            Assert.IsNotNull(builder);
            var sectionPath = new[] { "title1", "TITLEa", "TitleD" };
            var pathValue = string.Join("|", sectionPath);

            var sectionId = new Guid("1ad00000-0000-0000-0000-000000000000");
            var chartHeader = new ChartHeader { SectionId = sectionId, Name = "pieChart", FileExtension = "svg", TitlePath = pathValue };

            fakeToc.ChartHeaders.Add(chartHeader);
            var xhtml = $"<div class='chart-outlet' title-path='{pathValue}' chart-name='piechart'></div>";
            var resolvedHtml = builder.ResolveReportTemplate(xhtml);
            const string expected = "<div class=\"chart-outlet\">" +
                "<img class=\"chart-svg dummy\"" +
                " chart-id=\"1ad00000000000000000000000000000\"" +
                " chart-name=\"pieChart\" /></div>";
            Assert.AreEqual(expected, resolvedHtml);
        }

        [TestMethod]
        public void ReportBuilder_TestResolveReportTemplate_TableOutletElement() {
            var fakeToc = createFakeSummaryToc();
            var builder = new ReportBuilder(fakeToc);
            Assert.IsNotNull(builder);
            var sectionPath = new[] { "title1", "TITLEa", "TitleD" };
            var pathValue = string.Join("|", sectionPath);
            var sectionId = new Guid("1ad00000-0000-0000-0000-000000000000");
            var dataHeader = new CsvDataHeader { SectionId = sectionId, Name = "testTable", TitlePath = pathValue, SectionLabel = "testLabel" };
            fakeToc.DataHeaders.Add(dataHeader);
            var xhtml = $"<div class='table-outlet' title-path='{pathValue}' table-name='testTable'></div>";
            var resolvedHtml = builder.ResolveReportTemplate(xhtml);
            const string expected = "<div class=\"table-outlet\">" +
                "<table id=\"BAE15F4BF3CAB9EEB0F099DF6C2E9BD7\"" +
                " class=\"sortable csv-data-table\"" +
                " csv-download-id=\"1ad00000000000000000000000000000\"" +
                " csv-download-name=\"testTable\"></table>" +
                "</div>";
            Assert.AreEqual(expected, resolvedHtml);
        }

        [TestMethod]
        public void ReportBuilder_TestResolveReportTemplate_TableAllAttrElement() {
            var fakeToc = createFakeSummaryToc();
            var builder = new ReportBuilder(fakeToc);
            Assert.IsNotNull(builder);
            var sectionPath = new[] { "title1", "TITLEa", "TitleD" };
            var pathValue = string.Join("|", sectionPath);
            var sectionId = new Guid("1ad00000-0000-0000-0000-000000000000");
            var dataHeader = new CsvDataHeader { SectionId = sectionId, Name = "testTable", TitlePath = pathValue };
            fakeToc.DataHeaders.Add(dataHeader);
            var xhtml = "<div class='table-outlet'" +
                $" title-path='{pathValue}'" +
                " table-name='testTable'" +
                " table-caption='Test Table'" +
                " max-row-count='40'" +
                " column-order='0,2,4,5,1,3,10'></div>";
            var resolvedHtml = builder.ResolveReportTemplate(xhtml);
            const string expected = "<div class=\"table-outlet\">" +
                "<table id=\"BAE15F4BF3CAB9EEB0F099DF6C2E9BD7\"" +
                " class=\"sortable csv-data-table\"" +
                " csv-download-id=\"1ad00000000000000000000000000000\"" +
                " csv-download-name=\"testTable\"" +
                " csv-table-caption=\"Test Table\"" +
                " csv-max-records=\"40\"" +
                " csv-column-order=\"0,2,4,5,1,3,10\"></table>" +
                "</div>";
            Assert.AreEqual(expected, resolvedHtml);
        }

        [TestMethod]
        public void ReportBuilder_TestResolveSettingsStubs_WithHeader() {
            const string xhtml = "<div class='settings-outlet' header-level='4'></div>";
            var settings = new Dictionary<string, string> {
                {"Setting1", "Value1" },
                {"val1 < val2", "true" },
                {"SpecialChars", "<>?;&!:=+*%^~`" }
            };
            var resolvedHtml = ReportBuilder.ResolveSettingsStubs(xhtml, settings, "This is the header");
            const string expected = "<div class=\"settings-outlet\" header-level=\"4\">" +
                "<h4 class=\"sectionHeader\">This is the header</h4>" +
                "<table><thead>" +
                "<th>Setting name</th><th>Value</th>" +
                "</thead><tbody>" +
                "<tr><td>Setting1</td><td>Value1</td></tr>" +
                "<tr><td>val1 &lt; val2</td><td>true</td></tr>" +
                "<tr><td>SpecialChars</td><td>&lt;&gt;?;&amp;!:=+*%^~`</td></tr>" +
                "</tbody></table></div>";
            Assert.AreEqual(expected, resolvedHtml);
        }
        #region fakes

        private static OutputInfo createFakeOutputInfo() {
            return new OutputInfo() {
                BuildDate = DateTime.Now,
                DateCreated = DateTime.Now,
                BuildVersion = "version-xxx",
                Description = "description-xxx",
                ExecutionTime = "way too long",
                Title = "Project name"
            };
        }

        private SummaryToc createFakeSummaryToc() {
            var toc = new SummaryToc {
                Name = "Root",
                HasSectionData = false,
                SectionId = Guid.Empty,
                SectionTypeName = "TestRoot",
                SummarySectionName = "TestRoot",
            };
            for (var i = 1; i <= 15; i++) {
                var hdri = toc.AddEmptySubSectionHeader($"Title{i:X1}", i, sectionId: new Guid($"{i:X1}0000000-0000-0000-0000-000000000000"), sectionLabel: $"sectionLabel{i:X1}");
                for (var j = 1; j <= 15; j++) {
                    var hdrj = hdri.AddEmptySubSectionHeader($"Title{j:X1}", j, sectionId: new Guid($"{i:X1}{j:X1}000000-0000-0000-0000-000000000000"), sectionLabel: $"sectionLabel{i:X1}{j:X1}");
                    for (var h = 1; h <= 15; h++) {
                        _ = hdrj.AddEmptySubSectionHeader($"Title{h:X1}", h, sectionId: new Guid($"{i:X1}{j:X1}{h:X1}00000-0000-0000-0000-000000000000"), sectionLabel: $"sectionLabel{i:X1}{j:X1}{h:X1}");
                    }
                }
            }
            return toc;
        }

        #endregion
    }
}
