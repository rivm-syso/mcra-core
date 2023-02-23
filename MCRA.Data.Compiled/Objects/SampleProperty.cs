using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SampleProperty {

        public SampleProperty() {
            SamplePropertyValues = new HashSet<SamplePropertyValue>();
        }

        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<SamplePropertyValue> SamplePropertyValues { get; set; }

        public PropertyType PropertyType {
            get {
                return SamplePropertyValues
                    .All(ipv => ipv.IsNumeric()) ? PropertyType.Covariable : PropertyType.Cofactor;
            }
        }
    }
}
