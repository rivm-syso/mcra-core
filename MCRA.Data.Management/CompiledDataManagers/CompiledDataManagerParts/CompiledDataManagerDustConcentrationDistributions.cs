using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Extensions;
using MCRA.General.TableDefinitions.RawTableFieldEnums;
using MCRA.Utils.DataFileReading;

namespace MCRA.Data.Management.CompiledDataManagers {
    public partial class CompiledDataManager {

        /// <summary>
        /// Read all dust concentration distributions, within the scope.
        /// </summary>
        public IList<DustConcentrationDistribution> GetAllDustConcentrationDistributions() {
            if (_data.AllDustConcentrationDistributions == null) {
                LoadScope(SourceTableGroup.DustConcentrationDistributions);
                var allDustConcentrationDistributions = new List<DustConcentrationDistribution>();
                var rawDataSourceIds = _rawDataProvider.GetRawDatasourceIds(SourceTableGroup.DustConcentrationDistributions);
                if (rawDataSourceIds?.Count > 0) {
                    GetAllCompounds();
                    using (var rdm = _rawDataProvider.CreateRawDataManager()) {
                        foreach (var rawDataSourceId in rawDataSourceIds) {
                            using (var r = rdm.OpenDataReader<RawDustConcentrationDistributions>(rawDataSourceId, out int[] fieldMap)) {
                                while (r?.Read() ?? false) {
                                    var idSubstance = r.GetString(RawDustConcentrationDistributions.IdSubstance, fieldMap);
                                    var valid = CheckLinkSelected(ScopingType.Compounds, idSubstance);
                                    if (valid) {
                                        var unitString = r.GetStringOrNull(RawDustConcentrationDistributions.Unit, fieldMap);
                                        var unit = ConcentrationUnitConverter.FromString(unitString, ConcentrationUnit.ugPerg);
                                        var dustConcentrationDistribution = new DustConcentrationDistribution {
                                            Substance = _data.GetOrAddSubstance(idSubstance),
                                            Unit = unit,
                                            Mean = r.GetDouble(RawDustConcentrationDistributions.Mean, fieldMap),
                                            DistributionType = r.GetEnum(RawDustConcentrationDistributions.DistributionType, fieldMap, DustConcentrationDistributionType.Constant),
                                            CvVariability = r.GetDoubleOrNull(RawDustConcentrationDistributions.CvVariability, fieldMap),
                                            OccurrencePercentage = r.GetDoubleOrNull(RawDustConcentrationDistributions.OccurrencePercentage, fieldMap),
                                        };
                                        allDustConcentrationDistributions.Add(dustConcentrationDistribution);
                                    }
                                }
                            }
                        }
                    }
                }
                _data.AllDustConcentrationDistributions = allDustConcentrationDistributions;
            }
            return _data.AllDustConcentrationDistributions;
        }
    }
}
