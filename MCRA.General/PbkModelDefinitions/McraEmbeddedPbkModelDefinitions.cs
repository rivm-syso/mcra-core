using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve;

namespace MCRA.General {

    public class McraEmbeddedPbkModelDefinitions {

        private static IDictionary<string, DeSolvePbkModelSpecification> _pbkModelDefinitions;

        /// <summary>
        /// Returns all module definitions.
        /// </summary>
        public static IDictionary<string, DeSolvePbkModelSpecification> Definitions {
            get {
                if (_pbkModelDefinitions == null) {
                    var definitions = loadDefinitions();
                    _pbkModelDefinitions = definitions
                        .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
                }
                return _pbkModelDefinitions;
            }
        }

        /// <summary>
        /// Tries to get a model by alias; returns true if the model
        /// definition is found, false otherwise.
        /// </summary>
        /// <param name="str">States whether a model definition was found.</param>
        /// <param name="definition">The matched model definition.</param>
        /// <returns></returns>
        public static bool TryGetDefinitionByAlias(string str, out DeSolvePbkModelSpecification definition) {
            if (Definitions.TryGetValue(str, out definition)) {
                return true;
            }
            definition = Definitions.Values
                .FirstOrDefault(r => r.Aliases?.Contains(str, StringComparer.OrdinalIgnoreCase) ?? false);
            return definition != null;
        }

        /// <summary>
        /// Loads all definitions defined in the xml.
        /// </summary>
        private static Collection<DeSolvePbkModelSpecification> loadDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.PbkModelDefinitions.EmbeddedPbkModelSpecifications.Generated.xml")) {
                var xs = new XmlSerializer(typeof(EmbeddedPbkModelSpecificationCollection));
                var definitions = (EmbeddedPbkModelSpecificationCollection)xs.Deserialize(stream);
                return definitions;
            }
        }
    }
}
