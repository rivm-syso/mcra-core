using System.Text;
using System.Xml;
using MCRA.General.Action.Serialization;
using MCRA.General.Action.Settings;
using MCRA.General.Test.Helpers;
using MCRA.Utils.Xml;
using ModuleSettingsType = (string moduleId, (string key, string value)[])[];

namespace MCRA.General.Test.UnitTests.Action.Serialization {

    public class ProjectSettingsSerializerTestsBase {

        protected static readonly string _outputPath =
            Path.Combine(TestResourceUtilities.OutputResourcesPath, "Serialization");

        protected static readonly string _xmlResourcesPath = @"Resources\Xml\";


        #region Helpers

        protected static ProjectDto testImportSettingsXml(
            string filename,
            bool isOldStyle,
            DataSourceConfiguration dataSourceConfiguration,
            bool writeOutput = true
        ) {
            var settings = ProjectSettingsSerializer.ImportFromXmlFile($"{_xmlResourcesPath}{filename}", dataSourceConfiguration, isOldStyle, out var modified);
            if (writeOutput) {
                writeProjectSettingsXml(filename, settings);
            }
            return settings;
        }

        protected static void writeProjectSettingsXml(string filename, ProjectDto project) {
            if (!Directory.Exists(_outputPath)) {
                Directory.CreateDirectory(_outputPath);
            }
            var outputFile = Path.Combine(_outputPath, $"Transformed{filename}");
            ProjectSettingsSerializer.ExportToXmlFile(project, outputFile, true);
        }

        protected static string createMockSettingsXmlFromFile(string xmlResource, Version version = null) {
            var resourceFile = Path.Combine(_xmlResourcesPath, xmlResource);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(resourceFile);
            var xml = xmlDoc.DocumentElement.InnerXml;
            return createMockSettingsXml(xml, version);
        }

        protected static string createMockSettingsXml(string settingsXml = null, Version version = null) {
            var sb = new StringBuilder();
            sb.Append("<Project xmlns:xsd = \"http://www.w3.org/2001/XMLSchema\" xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\">");
            if (version != null) {
                sb.Append(
                    $"<McraVersion>" +
                    $"<Major>{version.Major}</Major>" +
                    $"<Minor>{version.Minor}</Minor>" +
                    $"<Build>{version.Build}</Build>" +
                    $"<Revision>{version.Revision}</Revision>" +
                    $"</McraVersion>");
            }
            if (!string.IsNullOrEmpty(settingsXml)) {
                sb.Append(settingsXml);
            }
            sb.Append("</Project>");
            return sb.ToString();
        }

        protected static string createMockSettingsXml(ModuleSettingsType modules, Version version = null) {
            if (modules == null || modules.Length == 0) {
                return string.Empty;
            }
            var sb = new StringBuilder();
            //check version, new format XML ModuleConfigurations/Settings structure or plain old XML elements
            var isNewFormat = version != null && version.Major >= 10 && version.Minor >= 1;

            if (isNewFormat) {
                sb.Append("<ModuleConfigurations>");
                foreach (var (moduleId, moduleSettings) in modules) {
                    sb.Append($"<ModuleConfiguration module='{moduleId}'><Settings>");
                    foreach (var (key, value) in moduleSettings) {
                        sb.Append($"<Setting id='{key}'>{value}</Setting>");
                    }
                    sb.Append("</Settings></ModuleConfiguration>");
                }
                sb.Append("</ModuleConfigurations>");
            } else {
                //old format, module settings are elements
                foreach (var (moduleId, moduleSettings) in modules) {
                    sb.Append($"<{moduleId}>");
                    foreach (var (key, value) in moduleSettings) {
                        sb.Append($"<{key}>{value}</{key}>");
                    }
                    sb.Append($"</{moduleId}>");
                }
            }

            return createMockSettingsXml(sb.ToString(), version);
        }

        protected static string applyPatch(string xml, string patchFileName) {
            string xslTransform = string.Empty;
            var fileName = $"MCRA.General.Action.Serialization.Transforms.{patchFileName}";
            using (var stream = typeof(ProjectSettingsSerializer).Assembly.GetManifestResourceStream(fileName)) {
                using (var sr = new StreamReader(stream)) {
                    xslTransform = sr.ReadToEnd();
                }
            }
            var xmlString = XmlSerialization.TransformXmlStringWithXslString(xml, xslTransform);
            return xmlString;
        }

        #endregion
    }
}
