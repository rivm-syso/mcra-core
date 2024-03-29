﻿using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Utility methods for rendering a html report.
    /// </summary>
    public class HtmlReportBuilder {
        public static string Render(string body) {
            var css = File.ReadAllText(@"Resources/ReportTemplate/css/print.css");
            return Render(body, css);
        }

        public static string Render(string body, string css) {
            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            sb.Append($"<style>{css}</style>");
            sb.Append("</head>");
            sb.Append("<body>");
            sb.Append(body);
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }
    }
}
