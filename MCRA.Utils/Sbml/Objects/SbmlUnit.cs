namespace MCRA.Utils.Sbml.Objects {
    public class SbmlUnit {

        private static readonly Dictionary<int, string> _siPrefixes = new() {
            { 3, "kilo" },
            { 2, "hecto" },
            { 1, "deca" },
            { 0, "" },
            { -1, "deci" },
            { -2, "centi" },
            { -3, "milli" },
            { -6, "micro" },
            { -9, "nano" },
            { -12, "pico" }
        };

        private static readonly Dictionary<int, string> _siShortPrefixes = new() {
            { 3, "k" },
            { 2, "h" },
            { 1, "da" },
            { 0, "" },
            { -1, "d" },
            { -2, "c" },
            { -3, "m" },
            { -6, "u" },
            { -9, "n" },
            { -12, "p" }
        };

        private static readonly Dictionary<SbmlUnitKind, string> _siShortBaseUnitStrings = new() {
            { SbmlUnitKind.Dimensionless, "" },
            { SbmlUnitKind.Second, "s" },
            { SbmlUnitKind.Gram, "g" },
            { SbmlUnitKind.Litre, "L" },
            { SbmlUnitKind.Metre, "m" },
            { SbmlUnitKind.Mole, "mol" },
        };

        private static readonly Dictionary<int, string> _shortTimeUnitMultipliers = new() {
            { 1, "s" },
            { 60, "min" },
            { 3600, "h" },
            { 86400, "d" },
            { 31557600, "y" },
        };

        public SbmlUnitKind Kind { get; set; }
        public decimal Exponent { get; set; }
        public decimal Scale { get; set; }
        public decimal Multiplier { get; set; }

        public string GetUnitString() {
            var multiplier = Multiplier;
            var scale = Scale;

            string baseUnitString;
            if (Kind == SbmlUnitKind.Dimensionless) {
                return string.Empty;
            } else if (Kind == SbmlUnitKind.Second) {
                var normalizedMultiplier = Math.Pow(10, (double)scale) * (double)multiplier;
                if (Math.Abs(normalizedMultiplier - Math.Round(normalizedMultiplier)) < 1e-5
                    && _shortTimeUnitMultipliers.TryGetValue((int)normalizedMultiplier, out var str)
                ) {
                    baseUnitString = str;
                    multiplier = 1;
                    scale = 0;
                } else {
                    baseUnitString = _shortTimeUnitMultipliers[1];
                }
            } else {
                baseUnitString = _siShortBaseUnitStrings[Kind];
            }

            string scaleString;
            if (Math.Abs((double)scale - Math.Round((double)scale)) < 1e-5
                && _siShortPrefixes.TryGetValue((int)scale, out var scaleStr)
            ) {
                scaleString = scaleStr;
            } else {
                scaleString = $"10^{scale} ";
            }
            var operatorString = (Exponent >= 0) ? "." : "/";
            var exponentString = Math.Abs(Exponent) != 1 ? $"^{Math.Abs(Exponent):g}" : string.Empty;
            var multiplierString = multiplier != 1 ? $"{multiplier}." : "";
            var result = $"{operatorString}{multiplierString}{scaleString}{baseUnitString}{exponentString}";
            return result;
        }
    }
}
