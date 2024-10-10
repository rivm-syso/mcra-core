using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class Response : StrongEntity {

        public TestSystem TestSystem { get; set; }

        public string ResponseTypeString { get; set; }

        public string ResponseUnit { get; set; }

        public string GuidelineMethod { get; set; }

        public ResponseType ResponseType => ResponseTypeConverter.FromString(ResponseTypeString);
    }
}
