using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RelativePotencyFactors;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawRelativePotencyFactorDataConverter : RawTableGroupDataConverterBase<RawRelativePotencyFactorsData> {

        public override RawRelativePotencyFactorsData FromCompiledData(CompiledData data) {
            return ToRaw(data.AllRelativePotencyFactors?.Values?.SelectMany(r => r));
        }

        public RawRelativePotencyFactorsData ToRaw(IEnumerable<RelativePotencyFactor> records) {
            var result = new RawRelativePotencyFactorsData();
            if (records?.Any() ?? false) {
                foreach (var record in records) {
                    var rawRecord = new RawRelativePotencyFactorRecord() {
                        idCompound = record.Compound?.Code,
                        idEffect = record.Effect?.Code,
                        RPF = record.RPF ?? double.NaN
                    };
                    result.RelativePotencyFactors.Add(rawRecord);
                    if (record?.RelativePotencyFactorsUncertains?.Any() ?? false) {
                        foreach (var uncertainRecord in record.RelativePotencyFactorsUncertains) {
                            var rawUncertainRecord = new RawRelativePotencyFactorUncertainRecord() {
                                idCompound = record.Compound.Code,
                                idEffect = record.Effect.Code,
                                idUncertaintySet = uncertainRecord.idUncertaintySet,
                                RPF = record.RPF ?? double.NaN
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
                var rawRecord = new RawRelativePotencyFactorRecord() {
                    idCompound = record.Key.Code,
                    idEffect = effect.Code,
                    RPF = record.Value
                };
                result.RelativePotencyFactors.Add(rawRecord);
            }
            return result;
        }
    }
}
