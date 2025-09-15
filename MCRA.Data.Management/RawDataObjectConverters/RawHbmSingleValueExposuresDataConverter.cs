using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawHbmSingleValueExposuresDataConverter : RawTableGroupDataConverterBase<RawHbmSingleValueExposuresData> {

        public override RawHbmSingleValueExposuresData FromCompiledData(CompiledData data) {
            throw new NotImplementedException();
        }

        public RawHbmSingleValueExposuresData ToRaw(IEnumerable<HbmSingleValueExposureSet> hbmSingleValueSets) {
            var result = new RawHbmSingleValueExposuresData();
            foreach (var model in hbmSingleValueSets) {
                var modelRecord = new RawHbmSingleValueExposureSet {
                    id = model.Code,
                    idSubstance = model.Substance.Code,
                    BiologicalMatrix = model.BiologicalMatrix.GetShortDisplayName(),
                    ExpressionType = model.ExpressionType.GetShortDisplayName(),
                    DoseUnit = model.DoseUnit.GetShortDisplayName()
                };

                foreach (var perc in model.HbmSingleValueExposures) {
                    result.HbmSingleValueExposureRecords.Add(
                        new RawHbmSingleValueExposure {
                            idExposureSet = model.Code,
                            Percentage = perc.Percentage,
                            Value = perc.Value
                        });
                }
                result.HbmSingleValueExposureSetRecords.Add(modelRecord);
            }
            return result;
        }
    }
}
