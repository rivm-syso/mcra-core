using MCRA.Data.Raw.Objects.ActiveSubstance;
using MCRA.Data.Raw.Objects.DoseResponseModels;
using MCRA.Data.Raw.Objects.HazardCharacterisations;
using MCRA.Data.Raw.Objects.RelativePotencyFactors;
using MCRA.General;

namespace MCRA.Data.Raw.Objects {
    public static class RawTableGroupDataFactory {
        public static IRawTableGroupData Create(SourceTableGroup tableGroup) {
            switch (tableGroup) {
                case SourceTableGroup.AssessmentGroupMemberships:
                    return new RawActiveSubstancesData();
                case SourceTableGroup.DoseResponseModels:
                    return new RawDoseResponseModelData();
                case SourceTableGroup.HazardCharacterisations:
                    return new RawHazardCharacterisationsData();
                case SourceTableGroup.RelativePotencyFactors:
                    return new RawRelativePotencyFactorsData();
                default:
                    return null;
            }
        }
    }
}
