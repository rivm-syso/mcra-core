using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Gets all kinetic models.
        /// </summary>
        /// <returns></returns>
        public IList<KineticModelInstance> GetAllPbkModels(string dataFilePath = null) {

            if (_data.AllKineticModelInstances == null) {
                LoadScope(SourceTableGroup.PbkModels);
                GetAllCompounds();
                GetAllPbkModelDefinitions(dataFilePath);
                var allPbkModelInstances = new Dictionary<string, KineticModelInstance>(StringComparer.OrdinalIgnoreCase);
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.PbkModels);
                using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                    if (rawDataSourceIds?.Count > 0) {
                        // Read kinetic model instances
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticModelInstances>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idModelDefinition = r.GetString(RawKineticModelInstances.IdModelDefinition, fieldMap);
                                    var idModelInstance = r.GetString(RawKineticModelInstances.IdModelInstance, fieldMap);
                                    var substanceCodes = r.GetString(RawKineticModelInstances.Substances, fieldMap)
                                        .Split(',')
                                        .Select(c => c.Trim())
                                        .ToArray();

                                    var valid = CheckLinkSelected(ScopingType.KineticModelDefinitions, idModelDefinition)
                                        & CheckLinkSelected(ScopingType.Compounds, substanceCodes)
                                        & IsCodeSelected(ScopingType.KineticModelDefinitions, idModelDefinition)
                                        & IsCodeSelected(ScopingType.KineticModelInstances, idModelInstance);


                                    if (valid && (_data.AllPbkModelDefinitions?.TryGetValue(idModelDefinition, out var pbkModelDefinition) ?? false)) {
                                        var modelDefinition = pbkModelDefinition.KineticModelDefinition;
                                        var modelSubstances = (modelDefinition?.GetModelSubstances()?.Count > 0)
                                            ? modelDefinition.GetModelSubstances().Count : 1;
                                        var substances = substanceCodes.Select(code => _data.GetOrAddSubstance(code));
                                        if (substanceCodes.Length != modelSubstances) {
                                            // Number of substances specified in instance does not match the number
                                            // of substances of the definition. This is only allowed when the number
                                            // of substances matches the number of input substances.
                                            var modelInputSubstances = (modelDefinition.GetModelSubstances()?.Count > 0)
                                                ? modelDefinition.GetModelSubstances().Count(r => r.IsInput)
                                                : 1;
                                            if (substanceCodes.Length != modelInputSubstances) {
                                                var msg = $"Error in model instance {idModelInstance}: " +
                                                    $"{substanceCodes.Length} substances were provided where the " +
                                                    $"referenced kinetic model {idModelDefinition} expects " +
                                                    $"{modelDefinition.GetModelSubstances().Count} substances.";
                                                throw new Exception(msg);
                                            }
                                        }
                                        List<PbkModelSubstance> kineticModelSubstances = null;
                                        if (modelDefinition?.GetModelSubstances()?.Count > 0) {
                                            kineticModelSubstances = [.. modelDefinition.GetModelSubstances()
                                                .Zip(substanceCodes)
                                                .Select(r => new PbkModelSubstance {
                                                    Substance = _data.GetOrAddSubstance(r.Second),
                                                    SubstanceDefinition = r.First
                                                })];
                                        } else {
                                            kineticModelSubstances = [.. substances
                                                .Select(r => new PbkModelSubstance() {
                                                    Substance = r,
                                                    SubstanceDefinition = null
                                                })];
                                        }
                                        var instance = new KineticModelInstance {
                                            IdModelInstance = idModelInstance,
                                            IdModelDefinition = pbkModelDefinition.IdModelDefinition,
                                            IdTestSystem = r.GetString(RawKineticModelInstances.IdTestSystem, fieldMap),
                                            ModelSubstances = kineticModelSubstances,
                                            Reference = r.GetStringOrNull(RawKineticModelInstances.Reference, fieldMap),
                                            Name = r.GetStringOrNull(RawKineticModelInstances.Name, fieldMap),
                                            Description = r.GetStringOrNull(RawKineticModelInstances.Description, fieldMap),
                                            KineticModelDefinition = modelDefinition,
                                            KineticModelInstanceParameters = new Dictionary<string, KineticModelInstanceParameter>(StringComparer.OrdinalIgnoreCase)
                                        };
                                        allPbkModelInstances.Add(idModelInstance, instance);
                                    }
                                }
                                ;
                            }
                        }

                        // Read kinetic model parameters
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawKineticModelInstanceParameters>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var variability = r.GetDoubleOrNull(RawKineticModelInstanceParameters.CvVariability, fieldMap);
                                    var uncertainty = r.GetDoubleOrNull(RawKineticModelInstanceParameters.CvUncertainty, fieldMap);
                                    var idModelInstance = r.GetString(RawKineticModelInstanceParameters.IdModelInstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.KineticModelInstances, false, idModelInstance)
                                        & allPbkModelInstances.TryGetValue(idModelInstance, out var instance);
                                    if (valid) {
                                        var kmp = new KineticModelInstanceParameter {
                                            IdModelInstance = idModelInstance,
                                            Parameter = r.GetString(RawKineticModelInstanceParameters.Parameter, fieldMap),
                                            Description = r.GetStringOrNull(RawKineticModelInstanceParameters.Description, fieldMap),
                                            Value = r.GetDouble(RawKineticModelInstanceParameters.Value, fieldMap),
                                            DistributionType = r.GetEnum<PbkModelParameterDistributionType>(RawKineticModelInstanceParameters.DistributionType, fieldMap),
                                            CvVariability = variability,
                                            CvUncertainty = uncertainty,
                                        };
                                        instance.KineticModelInstanceParameters[kmp.Parameter] = kmp;
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllKineticModelInstances = [.. allPbkModelInstances.Values];
            }
            return _data.AllKineticModelInstances;
        }
    }
}
