using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class TestSystem : StrongEntity {

        private string _description;

        public override string Description {
            get => string.IsNullOrEmpty(_description) ? Code : _description;
            set => _description = value;
        }

        public TestSystemType TestSystemType { get; set; }

        public string Species { get; set; }

        public string Strain { get; set; }

        public ExposureRoute ExposureRoute { get; set; }

        public string Organ { get; set; }

        public string GuidelineStudy { get; set; }

        public string Reference { get; set; }

        public ExposureTarget GetTarget() {
            return new ExposureTarget() {
                BiologicalMatrix = BiologicalMatrixConverter.FromString(Organ),
                ExposureRoute = ExposureRoute
            };
        }
    }
}
