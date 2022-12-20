using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DataSourceSummaryRecord {
        [Display(AutoGenerateField = false)]
        public int IdDataSourceVersion { get; set; }

        [DisplayName("Data type")]
        public string DataType { get; set; }

        [DisplayName("Data source")]
        [DisplayFormat(DataFormatString = "RawHTML")]
        public string FullPath {
            get {
                var fullPath = Path != null
                    ? System.IO.Path.Combine(Path, Name)
                    : Name;
                return $"<span class='data-source-link' data-source-version-id='{IdDataSourceVersion}'>{HttpUtility.HtmlEncode(fullPath)}</span>";
            }
        }

        [DisplayName("Data source name")]
        [Display(AutoGenerateField = false)]
        public string Name { get; set; }

        [DisplayName("Data source path")]
        [Display(AutoGenerateField = false)]
        public string Path { get; set; }

        [DisplayName("Version")]
        public string Version { get; set; }

        [DisplayName("Version date")]
        public string VersionDate { get; set; }

        [DisplayName("Checksum")]
        [Display(AutoGenerateField = false)]
        public string Checksum { get; set; }

    }
}
