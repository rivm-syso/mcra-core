using System.IO;
using System.Text;
using System.Web;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Utility methods for rendering a html report.
    /// </summary>
    public class HtmlReportBuilder {
        public static string Render(string body) {
            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.Append("<head>");
            var css = File.ReadAllText(@"Resources/ReportTemplate/print.css");
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
