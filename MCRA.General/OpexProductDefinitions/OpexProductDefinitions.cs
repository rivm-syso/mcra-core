using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.OpexProductDefinitions {
    public static class OpexProductDefinitions {

        private static IDictionary<string, OpexProductDefinition> _opexProductDefinitions;

        /// <summary>
        /// Returns all module definitions.
        /// </summary>
        public static IDictionary<string, OpexProductDefinition> Definitions {
            get {
                if (_opexProductDefinitions == null) {
                    var definitions = loadDefinitions();
                    _opexProductDefinitions = definitions
                        .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
                }
                return _opexProductDefinitions;
            }
        }

        private static Collection<OpexProductDefinition> loadDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.OpexProductDefinitions.OpexProductDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(OpexProductDefinitionCollection));
                var definitions = (OpexProductDefinitionCollection)xs.Deserialize(stream);
                return definitions;
            }
        }
    }
}
