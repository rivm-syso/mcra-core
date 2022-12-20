namespace MCRA.Data.Raw.Converters {
    public sealed class EntityCodeConversionTuple {

        public EntityCodeConversionTuple() { }

        public EntityCodeConversionTuple(string originalCode, string toCode) {
            OriginalCode = originalCode;
            ToCode = toCode;
        }

        public string OriginalCode { get; set; }
        public string ToCode { get; set; }
    }
}
