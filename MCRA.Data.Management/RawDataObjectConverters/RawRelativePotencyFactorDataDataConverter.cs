using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General.TableDefinitions.RawTableObjects;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawRelativePotencyFactorDataConverter : RawTableGroupDataConverterBase<RawRelativePotencyFactorsData> {

        public override RawRelativePotencyFactorsData FromCompiledData(CompiledData data) {
            return ToRaw(data.AllRelativePotencyFactors?.Values?.SelectMany(r => r));
        }

        public RawRelativePotencyFactorsData ToRaw(IEnumerable<RelativePotencyFactor> records) {
            var result = new RawRelativePotencyFactorsData();
            if (records?.Any() ?? false) {
                foreach (var record in records) {
                    var rawRecord = new RawRelativePotencyFactor() {
                        idCompound = record.Compound?.Code,
                        idEffect = record.Effect?.Code,
                        RPF = record.RPF,
                        PublicationAuthors = record.PublicationAuthors,
                        PublicationTitle = record.PublicationTitle,
                        PublicationUri = record.PublicationUri,
                        PublicationYear = record.PublicationYear,
                        Description = record.Description
                    };
                    result.RelativePotencyFactors.Add(rawRecord);
                    if (record?.RelativePotencyFactorsUncertains?.Count > 0) {
                        foreach (var uncertainRecord in record.RelativePotencyFactorsUncertains) {
                            var rawUncertainRecord = new RawRelativePotencyFactorUncertain() {
                                idCompound = record.Compound.Code,
                                idEffect = record.Effect.Code,
                                idUncertaintySet = uncertainRecord.idUncertaintySet,
                                RPF = record.RPF
                            };
                            result.RelativePotencyFactorsUncertain.Add(rawUncertainRecord);
                        }
                    }
                }
            }
            return result;
        }

        public RawRelativePotencyFactorsData ToRaw(
            Effect effect,
            IDictionary<Compound, double> records
        ) {
            var result = new RawRelativePotencyFactorsData();
            foreach (var record in records) {
                var rawRecord = new RawRelativePotencyFactor() {
                    idCompound = record.Key.Code,
                    idEffect = effect?.Code,
                    RPF = record.Value
                };
                result.RelativePotencyFactors.Add(rawRecord);
            }
            return result;
        }
    }
}
