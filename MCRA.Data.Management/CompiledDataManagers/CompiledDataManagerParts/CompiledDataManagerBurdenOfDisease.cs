using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Returns all burdens of disease of the compiled datasource.
        /// </summary>
        public IList<BurdenOfDisease> GetAllBurdensOfDisease() {
            if (_data.AllBurdensOfDisease == null) {
                LoadScope(SourceTableGroup.BurdensOfDisease);
                GetAllEffects();
                GetAllPopulations();
                var allBurdensOfDisease = new List<BurdenOfDisease>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.BurdensOfDisease);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawBurdensOfDisease>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idEffect = r.GetString(RawBurdensOfDisease.IdEffect, fieldMap);
                                    var idPopulation = r.GetString(RawBurdensOfDisease.Population, fieldMap);
                                    var record = new BurdenOfDisease() {
                                        Population = _data.GetOrAddPopulation(idPopulation),
                                        Effect = _data.GetOrAddEffect(idEffect),
                                        BodIndicator = r.GetEnum<BodIndicator>(RawBurdensOfDisease.BodIndicator, fieldMap),
                                        Value = r.GetDouble(RawBurdensOfDisease.Value, fieldMap)
                                    };
                                    allBurdensOfDisease.Add(record);
                                }
                            }
                        }
                    }
                }
                _data.AllBurdensOfDisease = allBurdensOfDisease;
            }
            return _data.AllBurdensOfDisease;
        }

        /// <summary>
        /// Returns all burdens of disease of the compiled datasource.
        /// </summary>
        public IList<BodIndicatorConversion> GetAllBodIndicatorConversions() {
            if (_data.AllBodIndicatorConversions == null) {
                LoadScope(SourceTableGroup.BurdensOfDisease);
                var bodIndicatorConversions = new List<BodIndicatorConversion>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.BurdensOfDisease);
                if (rawDataSourceIds?.Count > 0) {
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawBodIndicatorConversions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var bodIndicator = r.GetEnum<BodIndicator>(RawBodIndicatorConversions.FromIndicator, fieldMap);
                                    var record = new BodIndicatorConversion() {
                                        FromIndicator = bodIndicator,
                                        FromUnit = r.GetStringOrNull(RawBodIndicatorConversions.FromUnit, fieldMap),
                                        ToIndicator = r.GetEnum<BodIndicator>(RawBodIndicatorConversions.ToIndicator, fieldMap),
                                        ToUnit = r.GetStringOrNull(RawBodIndicatorConversions.ToUnit, fieldMap),
                                        Value = r.GetDouble(RawBodIndicatorConversions.Value, fieldMap)
                                    };
                                    bodIndicatorConversions.Add(record);
                                }
                            }
                        }
                    }
                }

                //foreach (var bod in allBurdensOfDisease) {
                //    var conversions = new List<BodIndicatorConversion>();
                //    var allBodIndicatorConversions = getBodIndicatorConversion(bod.BodIndicator, bodIndicatorConversions, conversions);
                //    if (allBodIndicatorConversions != null) {
                //        bod.BodIndicatorConversions.AddRange(allBodIndicatorConversions);
                //    }
                //}
                _data.AllBodIndicatorConversions = bodIndicatorConversions;
            }
            return _data.AllBodIndicatorConversions;
        }

        private List<BodIndicatorConversion> getBodIndicatorConversion(
            BodIndicator bodIndicator,
            Dictionary<BodIndicator, BodIndicatorConversion> bodIndicatorConversions,
            List<BodIndicatorConversion> conversions
        ) {
            if (bodIndicatorConversions.TryGetValue(bodIndicator, out var conversion)) {
                conversions.Add(conversion);
                getBodIndicatorConversion(conversion.ToIndicator, bodIndicatorConversions, conversions);
                return conversions;
            }
            return null;
        }

        private static void writeBurdensOfDiseaseDataToCsv(string tempFolder, IEnumerable<BurdenOfDisease> burdensOfDisease) {
            if (!burdensOfDisease?.Any() ?? true) {
                return;
            }

            var tdBurdensOfDisease = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.BurdensOfDisease);
            var dtABurdensOfDisease = tdBurdensOfDisease.CreateDataTable();
            var ccr = new int[Enum.GetNames(typeof(RawBurdensOfDisease)).Length];
            foreach (var bod in burdensOfDisease) {
                var r = dtABurdensOfDisease.NewRow();
                r.WriteNonEmptyString(RawBurdensOfDisease.Population, bod.Population.ToString(), ccr);
                r.WriteNonEmptyString(RawBurdensOfDisease.IdEffect, bod.Effect?.Code, ccr);
                r.WriteNonEmptyString(RawBurdensOfDisease.BodIndicator, bod.BodIndicator.ToString(), ccr);
                r.WriteNonNullDouble(RawBurdensOfDisease.Value, bod.Value, ccr);
                dtABurdensOfDisease.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdBurdensOfDisease, dtABurdensOfDisease);
        }
        private static void writeBodIndicatorConversionDataToCsv(string tempFolder, IEnumerable<BodIndicatorConversion> bodIndicatorConversion) {
            if (!bodIndicatorConversion?.Any() ?? true) {
                return;
            }

            var tdbodIndicatorConversion = McraTableDefinitions.Instance.GetTableDefinition(RawDataSourceTableID.BodIndicatorConversions);
            var dtbodIndicatorConversion = tdbodIndicatorConversion.CreateDataTable();
            foreach (var conversion in bodIndicatorConversion) {
                var r = dtbodIndicatorConversion.NewRow();
                r.WriteNonEmptyString(RawBodIndicatorConversions.FromIndicator, conversion.FromIndicator.ToString());
                r.WriteNonEmptyString(RawBodIndicatorConversions.FromUnit, conversion.FromUnit);
                r.WriteNonEmptyString(RawBodIndicatorConversions.ToIndicator, conversion.ToIndicator.ToString());
                r.WriteNonEmptyString(RawBodIndicatorConversions.ToUnit, conversion.ToUnit);
                r.WriteNonNullDouble(RawBodIndicatorConversions.Value, conversion.Value);
                dtbodIndicatorConversion.Rows.Add(r);
            }
            writeToCsv(tempFolder, tdbodIndicatorConversion, dtbodIndicatorConversion);
        }

    }
}
