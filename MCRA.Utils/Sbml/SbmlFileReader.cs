using System.Globalization;
using System.Xml;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.Utils.SBML {
    public class SbmlFileReader {

        private XmlNamespaceManager _xmlNamespaceManager;

        private static readonly HashSet<string> _secondTimeUnitAliases = new(StringComparer.OrdinalIgnoreCase) {
            "SEC",
            "SECONDS",
            "S"
        };

        private static readonly HashSet<string> _dayTimeUnitAliases = new(StringComparer.OrdinalIgnoreCase) {
            "DAY",
            "DAYS",
            "D"
        };

        private static readonly HashSet<string> _hourTimeUnitAliases = new(StringComparer.OrdinalIgnoreCase) {
            "HR",
            "HOUR",
            "HOURS",
            "H"
        };

        public SbmlModel LoadModel(string pathSbmlfile) {

            if (!File.Exists(pathSbmlfile)) {
                throw new FileNotFoundException(pathSbmlfile);
            }
            var doc = new XmlDocument();
            doc.Load(pathSbmlfile);

            var root = doc.DocumentElement;
            if (root.Name != "sbml") {
                throw new Exception($"Error parsing SBML file [{pathSbmlfile}]: unexpected root node name [{root.Name}].");
            }

            (var level, var version) = getVersion(root);
            var sbmlNamespaceString = getVersionNameSpace(level, version);

            _xmlNamespaceManager = new XmlNamespaceManager(doc.NameTable);
            _xmlNamespaceManager.AddNamespace("ls", sbmlNamespaceString);
            _xmlNamespaceManager.AddNamespace("rdf", @"http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            _xmlNamespaceManager.AddNamespace("bqbiol", @"http://biomodels.net/biology-qualifiers/");
            _xmlNamespaceManager.AddNamespace("bqmodel", @"http://biomodels.net/model-qualifiers/");

            var modelNode = root.SelectSingleNode("descendant::ls:model", _xmlNamespaceManager);

            var id = parseId(modelNode);
            var name = parseName(modelNode);

            var unitDefinitions = parseUnitDefinitions(modelNode).ToDictionary(r => r.Id);
            var timeUnits = modelNode.Attributes["timeUnits"]?.Value;
            var substancesUnits = modelNode.Attributes["substancesUnits"]?.Value;
            var volumeUnits = modelNode.Attributes["volumeUnits"]?.Value;

            var assignmentRules = parseAssignmentRules(modelNode);
            var reactions = parseReactions(modelNode);
            var species = parseSpecies(modelNode);
            var parameters = parseParameters(modelNode);
            var compartments = parseCompartments(modelNode);

            var model = new SbmlModel() {
                Id = id,
                Name = name,
                TimeUnits = timeUnits,
                SubstancesUnits = substancesUnits,
                VolumeUnits = volumeUnits,
                Compartments = compartments,
                Parameters = parameters,
                Species = species,
                UnitDefinitions = unitDefinitions,
                AssignmentRules = assignmentRules,
                Reactions = reactions
            };

            return model;
        }

        private string getVersionNameSpace(int level, int version) {
            string sbmlNamespaceString;
            if (level == 3) {
                if (version == 1) {
                    sbmlNamespaceString = @"http://www.sbml.org/sbml/level3/version1/core";
                } else if (version == 2) {
                    sbmlNamespaceString = @"http://www.sbml.org/sbml/level3/version2/core";
                } else {
                    throw new Exception($"Error parsing SBML file: version [{version}] of level [{level}] not supported");
                }
            } else {
                throw new Exception($"Error parsing SBML file: level [{level}] not supported.");
            }

            return sbmlNamespaceString;
        }

        private (int level, int version) getVersion(XmlElement root) {
            var levelAttribute = root.GetAttribute("level");
            if (string.IsNullOrEmpty(levelAttribute)) {
                throw new Exception($"Error parsing the SBML file: cannot determine level.");
            }
            var level = int.Parse(levelAttribute);
            var versionAttribute = root.GetAttribute("version");
            if (string.IsNullOrEmpty(versionAttribute)) {
                throw new Exception($"Error parsing the SBML file: cannot determine level.");
            }
            var version = int.Parse(versionAttribute);
            return (level, version);
        }

        private string parseId(XmlNode modelNode) {
            return modelNode.Attributes["id"]?.Value;
        }

        private string parseName(XmlNode modelNode) {
            return modelNode.Attributes["name"]?.Value;
        }

        private Dictionary<string, SbmlModelCompartment> parseCompartments(XmlNode modelNode) {
            var result = new List<SbmlModelCompartment>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfCompartments/ls:compartment", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var node = enumerator.Current as XmlNode;
                if (node != null) {
                    var record = new SbmlModelCompartment();
                    var id = node.Attributes["id"]?.InnerText;
                    var name = node.Attributes["name"]?.InnerText;
                    var metaId = node.Attributes["metaid"]?.InnerText;
                    var units = node.Attributes["units"]?.InnerText;
                    record.Id = id;
                    record.Name = name;
                    record.Units = units;
                    record.MetaId = metaId;
                    record.BqbIsResources = parseElementAnnotation(node, metaId, "bqbiol:is");
                    record.BqmIsResources = parseElementAnnotation(node, metaId, "bqmodel:is");
                    result.Add(record);
                }
            }
            return result.ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
        }

        private Dictionary<string, SbmlModelSpecies> parseSpecies(XmlNode modelNode) {
            var result = new List<SbmlModelSpecies>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfSpecies/ls:species", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var node = enumerator.Current as XmlNode;
                if (node != null) {
                    var record = new SbmlModelSpecies();
                    var id = node.Attributes["id"]?.InnerText;
                    var name = node.Attributes["name"]?.InnerText;
                    var metaId = node.Attributes["metaid"]?.InnerText;
                    var compartment = node.Attributes["compartment"]?.InnerText;
                    var substanceUnit = node.Attributes["substanceUnits"]?.InnerText;
                    var units = node.Attributes["units"]?.InnerText;
                    record.Id = id;
                    record.Name = name;
                    record.Units = units;
                    record.Compartment = compartment;
                    record.SubstanceUnits = substanceUnit;
                    record.MetaId = metaId;
                    record.BqbIsResources = parseElementAnnotation(node, metaId, "bqbiol:is");
                    record.BqmIsResources = parseElementAnnotation(node, metaId, "bqmodel:is");
                    result.Add(record);
                }
            }
            return result.ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
        }

        private Dictionary<string, SbmlModelParameter> parseParameters(XmlNode modelNode) {
            var result = new List<SbmlModelParameter>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfParameters/ls:parameter", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var node = enumerator.Current as XmlNode;
                if (node != null) {
                    var record = new SbmlModelParameter();
                    var id = node.Attributes["id"]?.InnerText;
                    var name = node.Attributes["name"]?.InnerText;
                    var metaId = node.Attributes["metaid"]?.InnerText;
                    var units = node.Attributes["units"]?.InnerText;
                    var valueString = node.Attributes["value"]?.InnerText;
                    var constantString = node.Attributes["constant"]?.InnerText;
                    var isConstant = constantString == "true";
                    record.Id = id;
                    record.Name = name;
                    record.Units = units;
                    record.MetaId = metaId;
                    record.BqbIsResources = parseElementAnnotation(node, metaId, "bqbiol:is");
                    record.BqmIsResources = parseElementAnnotation(node, metaId, "bqmodel:is");
                    record.DefaultValue = double.TryParse(valueString, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var value) ? value : double.NaN;
                    record.IsConstant = isConstant;
                    result.Add(record);
                }
            }
            return result.ToDictionary(r => r.Id, StringComparer.OrdinalIgnoreCase);
        }

        private List<SbmlReaction> parseReactions(XmlNode modelNode) {
            var result = new List<SbmlReaction>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfReactions/ls:reaction", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var reactionNode = enumerator.Current as XmlNode;
                if (reactionNode != null) {
                    var reactants = reactionNode
                        .SelectNodes($"descendant::ls:listOfReactants/ls:speciesReference", _xmlNamespaceManager)
                        .Cast<XmlNode>()
                        .Select(r => r.Attributes["species"].InnerText)
                        .ToList();
                    var products = reactionNode
                        .SelectNodes($"descendant::ls:listOfProducts/ls:speciesReference", _xmlNamespaceManager)
                        .Cast<XmlNode>()
                        .Select(r => r.Attributes["species"].InnerText)
                        .ToList();
                    var record = new SbmlReaction {
                        Id = reactionNode.Attributes["id"].InnerText,
                        Products = products,
                        Reactants = reactants
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        private List<string> parseElementAnnotation(XmlNode node, string metaId, string qualifier) {
            var annotationNode = node.SelectSingleNode("descendant::ls:annotation", _xmlNamespaceManager);
            if (annotationNode != null) {
                var rdfDescriptionNode = annotationNode.SelectSingleNode($@"descendant::rdf:RDF/rdf:Description[@rdf:about='#{metaId}']", _xmlNamespaceManager);
                if (rdfDescriptionNode != null) {
                    var bqbIsNodes = rdfDescriptionNode.SelectNodes($"descendant::{qualifier}/rdf:Bag/rdf:li", _xmlNamespaceManager);
                    var resources = bqbIsNodes
                        .Cast<XmlNode>()
                        .Select(r => r.Attributes["rdf:resource"].InnerText)
                        .ToList();
                    return resources;
                }
            }
            return null;
        }

        private List<SbmlModelAssignmentRule> parseAssignmentRules(XmlNode modelNode) {
            var result = new List<SbmlModelAssignmentRule>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfRules/ls:assignmentRule", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var node = enumerator.Current as XmlNode;
                if (node != null) {
                    var record = new SbmlModelAssignmentRule {
                        Variable = node.Attributes["variable"]?.InnerText
                    };
                    result.Add(record);
                }
            }
            return result;
        }

        private List<SbmlUnitDefinition> parseUnitDefinitions(XmlNode modelNode) {
            var result = new List<SbmlUnitDefinition>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfUnitDefinitions/ls:unitDefinition", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var node = enumerator.Current as XmlNode;
                if (node != null) {
                    var record = new SbmlUnitDefinition();
                    var id = node.Attributes["id"]?.InnerText;
                    var name = node.Attributes["name"]?.InnerText;
                    record.Id = id;
                    record.Name = name;
                    record.Units = parseUnits(node);
                    result.Add(record);
                }
            }
            return result;
        }

        private List<SbmlUnit> parseUnits(XmlNode modelNode) {
            var result = new List<SbmlUnit>();
            var nodeList = modelNode.SelectNodes("descendant::ls:listOfUnits/ls:unit", _xmlNamespaceManager);
            var enumerator = nodeList.GetEnumerator();
            while (enumerator.MoveNext()) {
                var node = enumerator.Current as XmlNode;
                if (node != null) {
                    var record = new SbmlUnit {
                        Kind = SbmlUnitKindConverter.Parse(node.Attributes["kind"]?.InnerText),
                        Exponent = decimal.Parse(node.Attributes["exponent"]?.InnerText, CultureInfo.InvariantCulture),
                        Scale = decimal.Parse(node.Attributes["scale"]?.InnerText, CultureInfo.InvariantCulture),
                        Multiplier = decimal.Parse(node.Attributes["multiplier"]?.InnerText, CultureInfo.InvariantCulture)
                    };
                    result.Add(record);
                    var annotationNode = node.SelectSingleNode("annotation");
                    if (annotationNode != null) {
                        // TODO: parse RDF annotation
                    }
                }
            }
            return result;
        }
    }
}
