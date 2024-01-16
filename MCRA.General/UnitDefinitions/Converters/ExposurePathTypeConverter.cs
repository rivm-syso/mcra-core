namespace MCRA.General {
    public partial class ExposurePathTypeConverter : UnitConverterBase<ExposurePathType> {
        public static ExposurePathType FromString(string str) {
            return FromString(str, ExposurePathType.Undefined);
        }
    }
}
