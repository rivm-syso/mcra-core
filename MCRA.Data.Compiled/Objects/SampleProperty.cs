using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SampleProperty {

        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<SamplePropertyValue> SamplePropertyValues { get; set; } = new HashSet<SamplePropertyValue>();

        public PropertyType PropertyType => SamplePropertyValues
            .All(ipv => ipv.IsNumeric()) ? PropertyType.Covariable : PropertyType.Cofactor;

        public override string ToString() {
            return $"{Name} ({GetHashCode():X8}";
        }
    }
}
