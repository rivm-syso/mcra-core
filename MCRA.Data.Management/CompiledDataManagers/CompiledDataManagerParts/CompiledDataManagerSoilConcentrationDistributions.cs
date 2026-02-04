using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all soil concentration distributions, within the scope.
        /// </summary>
        public IList<SoilConcentrationDistribution> GetAllSoilConcentrationDistributions() {
            if (_data.AllSoilConcentrationDistributions == null) {
                LoadScope(SourceTableGroup.SoilConcentrationDistributions);
                var allSoilConcentrationDistributions = new List<SoilConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.SoilConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawSoilConcentrationDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawSoilConcentrationDistributions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawSoilConcentrationDistributions.Unit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerg);
                                        var SoilConcentrationDistribution = new SoilConcentrationDistribution {
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Unit = unit,
                                            Mean = r.GetDouble(RawSoilConcentrationDistributions.Mean, fieldMap),
                                            DistributionType = r.GetEnum(RawSoilConcentrationDistributions.DistributionType, fieldMap, SoilConcentrationDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawSoilConcentrationDistributions.CvVariability, fieldMap),
                                            OccurrencePercentage = r.GetDoubleOrNull(RawSoilConcentrationDistributions.OccurrencePercentage, fieldMap),
                                        };
                                        allSoilConcentrationDistributions.Add(SoilConcentrationDistribution);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllSoilConcentrationDistributions = allSoilConcentrationDistributions;
            }
            return _data.AllSoilConcentrationDistributions;
        }
    }
}
