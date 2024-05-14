using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using MCRA.General.Sbml;
using MCRA.Utils.SBML;

namespace MCRA.General {

    public class MCRAKineticModelDefinitions {

        private static UnitDefinition _unitDefinition;
        private static IDictionary<string, KineticModelDefinition> _kineticModelDefinitions;

        public static void AddSbmlModel(string id, string filename, List<string> aliases) {
            if (!File.Exists(filename)) {
                throw new FileNotFoundException();
            }
            var reader = new SbmlFileReader();
            var sbmlModel = reader.LoadModel(filename);
            var converter = new SbmlToPbkModelDefinitionConverter();
            var modelDefinition = converter.Convert(sbmlModel);
            modelDefinition.Id = id;
            modelDefinition.Aliases = aliases;
            modelDefinition.FileName = filename;
            Definitions.Add(modelDefinition.Id, modelDefinition);
        }

        /// <summary>
        /// Returns all module definitions.
        /// </summary>
        public static IDictionary<string, KineticModelDefinition> Definitions {
            get {
                if (_kineticModelDefinitions == null) {
                    var definitions = loadDefinitions();
                    _kineticModelDefinitions = definitions
                        .ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
                }
                return _kineticModelDefinitions;
            }
        }

        /// <summary>
        /// Gets the unit definitions of this unit.
        /// </summary>
        public static UnitDefinition UnitDefinition {
            get {
                if (_unitDefinition == null) {
                    loadDefinitions();
                }
                return _unitDefinition;
            }
        }

        /// <summary>
        /// Tries to get a kinetic model definition by alias; returns true if the model
        /// definition is found, false otherwise.
        /// </summary>
        /// <param name="str">States whether a model definition was found.</param>
        /// <param name="definition">The matched model definition.</param>
        /// <returns></returns>
        public static bool TryGetDefinitionByAlias(string str, out KineticModelDefinition definition) {
            if (Definitions.TryGetValue(str, out definition)) {
                return true;
            }
            definition = Definitions.Values
                .FirstOrDefault(r => r.Aliases?.Contains(str, StringComparer.OrdinalIgnoreCase) ?? false);
            return definition != null;
        }

        /// <summary>
        /// Returns all table group definitions (including the nested table definitions) as defined in
        /// the internal xml.
        /// </summary>
        private static Collection<KineticModelDefinition> loadDefinitions() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.KineticModelDefinitions.KineticModelDefinitions.Generated.xml")) {
                var xs = new XmlSerializer(typeof(KineticModelDefinitionCollection));
                var definitions = (KineticModelDefinitionCollection)xs.Deserialize(stream);
                _unitDefinition = new UnitDefinition() {
                    Name = "Kinetic model definitions",
                    Id = "KineticModels",
                    Units = definitions.Select(r => r as UnitValueDefinition).ToList(),
                };
                return definitions;
            }
        }
    }
}
