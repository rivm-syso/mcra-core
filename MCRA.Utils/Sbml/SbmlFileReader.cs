using System.Globalization;
using System.Xml;
using MCRA.Utils.Sbml.Objects;

namespace MCRA.Utils.SBML {
    public class SbmlFileReader {

        private XmlNamespaceManager _xmlNamespaceManager;

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

            var model = new SbmlModel() {
                Id = parseId(modelNode),
                Name = parseName(modelNode),
                TimeUnit = parseTimeUnit(modelNode),
                Compartments = parseCompartments(modelNode),
                Parameters = parseParameters(modelNode),
                Species = parseSpecies(modelNode),
                UnitDefinitions = parseUnitDefinitions(modelNode).ToDictionary(r => r.Id),
                AssignmentRules = parseAssignmentRules(modelNode),
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

        private static HashSet<string> _secondTimeUnitAliases = new(StringComparer.OrdinalIgnoreCase) {
            "SEC",
            "SECONDS",
            "S"
        };
        private static HashSet<string> _dayTimeUnitAliases = new(StringComparer.OrdinalIgnoreCase) {
            "DAY",
            "DAYS",
            "D"
        };
        private static HashSet<string> _hourTimeUnitAliases = new(StringComparer.OrdinalIgnoreCase) {
            "HR",
            "HOUR",
            "HOURS",
            "H"
        };

        private SbmlTimeUnit parseTimeUnit(XmlNode modelNode) {
            // TODO PBK SBML: get time unit from SBML model
            var timeUnit = modelNode.Attributes["timeUnits"]?.Value;
            if (_dayTimeUnitAliases.Contains(timeUnit)) {
                return SbmlTimeUnit.Days;
            } else if (_hourTimeUnitAliases.Contains(timeUnit)) {
                return SbmlTimeUnit.Hours;
            } else if (_secondTimeUnitAliases.Contains(timeUnit)) {
                return SbmlTimeUnit.Seconds;
            }
            // TODO PBK SBML: default?
            return SbmlTimeUnit.Hours;
        }

        private List<SbmlModelCompartment> parseCompartments(XmlNode modelNode) {
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
                    record.BqbIsResources = parseElementAnnotation(node, metaId);
                    result.Add(record);
                }
            }
            return result;
        }

        private List<SbmlModelSpecies> parseSpecies(XmlNode modelNode) {
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
                    record.BqbIsResources = parseElementAnnotation(node, metaId);
                    result.Add(record);
                }
            }
            return result;
        }

        private List<SbmlModelParameter> parseParameters(XmlNode modelNode) {
            var results = new List<SbmlModelParameter>();
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
                    record.Id = id;
                    record.Name = name;
                    record.Units = units;
                    record.MetaId = metaId;
                    record.BqbIsResources = parseElementAnnotation(node, metaId);
                    record.DefaultValue = double.TryParse(valueString, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out var value) ? value : double.NaN;
                    results.Add(record);
                }
            }
            return results;
        }

        private List<string> parseElementAnnotation(XmlNode node, string metaId) {
            var annotationNode = node.SelectSingleNode("descendant::ls:annotation", _xmlNamespaceManager);
            if (annotationNode != null) {
                var rdfDescriptionNode = annotationNode.SelectSingleNode($@"descendant::rdf:RDF/rdf:Description[@rdf:about='#{metaId}']", _xmlNamespaceManager);
                if (rdfDescriptionNode != null) {
                    var bqbIsNodes = rdfDescriptionNode.SelectNodes("descendant::bqbiol:is/rdf:Bag/rdf:li", _xmlNamespaceManager);
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
