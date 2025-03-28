using NCalc;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ErfSubgroup {
        public string idModel { get; set; }
        public string idSubgroup { get; set; }
        public double? ExposureUpper { get; set; }
        public Expression ExposureResponseSpecification { get; set; }
        public Expression ExposureResponseSpecificationLower { get; set; }
        public Expression ExposureResponseSpecificationUpper { get; set; }
    }
}
