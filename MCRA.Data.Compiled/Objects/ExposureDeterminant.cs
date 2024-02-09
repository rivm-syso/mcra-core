using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureDeterminant : IStrongEntity {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IndividualPropertyType PropertyType { get; set; }
    }
}
