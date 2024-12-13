using MCRA.Data.Raw.Objects.RawTableGroups;
using MCRA.General;

namespace MCRA.Data.Raw.Objects {
    public static class RawTableGroupDataFactory {
        public static IRawTableGroupData Create(SourceTableGroup tableGroup) {
            return tableGroup switch {
                SourceTableGroup.AssessmentGroupMemberships => new RawActiveSubstancesData(),
                SourceTableGroup.DoseResponseModels => new RawDoseResponseModelData(),
                SourceTableGroup.HazardCharacterisations => new RawHazardCharacterisationsData(),
                SourceTableGroup.RelativePotencyFactors => new RawRelativePotencyFactorsData(),
                _ => null,
            };
        }
    }
}
