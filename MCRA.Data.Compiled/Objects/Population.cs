using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public class Population : StrongEntity {
        public virtual string Location { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public virtual double NominalBodyWeight { get; set; }
        public virtual double Size { get; set; }

        public virtual PopulationSizeDistributionType SizeUncertaintyDistribution { get; set; } = PopulationSizeDistributionType.Constant;
        public virtual double? SizeUncertaintyLower { get; set; }
        public virtual double? SizeUncertaintyUpper { get; set; }

        public virtual BodyWeightUnit BodyWeightUnit => BodyWeightUnit.kg;

        public virtual Dictionary<string, PopulationIndividualPropertyValue> PopulationIndividualPropertyValues { get; set; }

        public virtual ICollection<PopulationCharacteristic> PopulationCharacteristics { get; set; } = [];

        public bool HasPopulationCharacteristics() {
            return PopulationCharacteristics != null && PopulationCharacteristics.Count > 0;
        }

        public override bool Equals(object obj) {
            if (obj is Population other) {
                return string.Equals(Code, other.Code, StringComparison.Ordinal);
            }
            return false;
        }

        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(Population left, Population right) {
            if (ReferenceEquals(left, right)) {
                return true;
            }
            if (left is null || right is null) {
                return false;
            }
            return string.Equals(left.Code, right.Code, StringComparison.Ordinal);
        }

        public static bool operator !=(Population left, Population right)
            => !(left == right);
    }
}
