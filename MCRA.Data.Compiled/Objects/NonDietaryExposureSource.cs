namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietaryExposureSource : StrongEntity {

        public NonDietaryExposureSource() {
        }

        public NonDietaryExposureSource(string code) : this() {
            Code = code;
        }
    }
}
