using MCRA.Data.Compiled;
using MCRA.Data.Compiled.Objects;
using MCRA.General.TableDefinitions.RawTableObjects;
using MCRA.Data.Raw.Objects.RawTableGroups;

namespace MCRA.Data.Management.RawDataObjectConverters {
    public sealed class RawActiveSubstancesDataConverter : RawTableGroupDataConverterBase<RawActiveSubstancesData> {

        public override RawActiveSubstancesData FromCompiledData(CompiledData data) {
            return ToRaw(data.AllActiveSubstanceModels?.Values);
        }

        public RawActiveSubstancesData ToRaw(IEnumerable<ActiveSubstanceModel> records) {
            if (!records?.Any() ?? true) {
                return null;
            }
            var result = new RawActiveSubstancesData();
            foreach (var record in records) {
                var rawRecord = new RawAssessmentGroupMembershipModel() {
                    id = record.Code,
                    Name = record.Name,
                    Description = record.Description,
                    idEffect = record.Effect?.Code,
                    idIndexSubstance = record.IndexSubstance?.Code,
                    Accuracy = record.Accuracy,
                    Sensitivity = record.Sensitivity,
                    Specificity = record.Specificity,
                    Reference = record.Reference
                };
                result.ActiveSubstanceModels.Add(rawRecord);
                if (record?.MembershipProbabilities?.Values.Any() ?? false) {
                    foreach (var membershipRecord in record?.MembershipProbabilities) {
                        var rawMembershipRecord = new RawAssessmentGroupMembership() {
                            idCompound = membershipRecord.Key.Code,
                            idGroupMembershipModel = record.Code,
                            MembershipProbability = membershipRecord.Value,
                        };
                        result.ActiveSubstances.Add(rawMembershipRecord);
                    }
                }
            }
            return result;
        }

        public RawActiveSubstancesData ToRaw(
            string code,
            string name,
            string description,
            string reference,
            Effect effect,
            Compound indexSubstance,
            IDictionary<Compound, double> records
        ) {
            var result = new RawActiveSubstancesData();
            var rawModelRecord = new RawAssessmentGroupMembershipModel() {
                id = code,
                idEffect = effect?.Code,
                Name = name,
                Description = description,
                Reference = reference,
                idIndexSubstance = indexSubstance?.Code
            };
            result.ActiveSubstanceModels.Add(rawModelRecord);
            foreach (var record in records) {
                var rawMembershipRecord = new RawAssessmentGroupMembership() {
                    idCompound = record.Key.Code,
                    idGroupMembershipModel = code,
                    MembershipProbability = record.Value
                };
                result.ActiveSubstances.Add(rawMembershipRecord);
            }
            return result;
        }
    }
}
