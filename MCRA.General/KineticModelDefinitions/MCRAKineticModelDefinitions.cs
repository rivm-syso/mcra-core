using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General {

    public class MCRAKineticModelDefinitions {

        private static UnitDefinition _unitDefinition;
        private static IDictionary<string, KineticModelDefinition> _kineticModelDefinitions;
        private static IDictionary<string, KineticModelDefinition> _kineticModelDefinitionByAlias;

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
        /// Parses the string as an amount unit.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static KineticModelType FromString(string str) {
            if (!string.IsNullOrEmpty(str)) {
                return UnitDefinition.FromString<KineticModelType>(str);
            }
            return KineticModelType.Undefined;
        }

        /// <summary>
        /// Tries to get a kinetic model definition by alias; returns true if the model
        /// definition is found, false otherwise.
        /// </summary>
        /// <param name="str">States whether a model definition was found.</param>
        /// <param name="definition">The matched model definition.</param>
        /// <returns></returns>
        public static bool TryGetDefinitionByAlias(string str, out KineticModelDefinition definition) {
            if (_kineticModelDefinitionByAlias == null) {
                var definitions = Definitions.Values;
                _kineticModelDefinitionByAlias = definitions
                    .SelectMany(
                        r => r.Aliases,
                        (d, a) => (alias: a, definition: d)
                    )
                    .ToDictionary(r => r.alias, r => r.definition, StringComparer.OrdinalIgnoreCase);
            }
            var result = _kineticModelDefinitionByAlias.TryGetValue(str, out var model);
            definition = model;
            return result;
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
