namespace MCRA.Utils.Sbml.Objects {
    public enum SbmlUnitKind {
        Undefined = -1,
        Litre,
        Metre,
        Gram,
        Second,
        Mole
    }

    public static class SbmlUnitKindConverter {
        public static SbmlUnitKind Parse(string str) {
            if (str.Equals("litre", StringComparison.OrdinalIgnoreCase)) {
                return SbmlUnitKind.Litre;
            } else if (str.Equals("metre", StringComparison.OrdinalIgnoreCase)) {
                return SbmlUnitKind.Metre;
            } else if (str.Equals("gram", StringComparison.OrdinalIgnoreCase)) {
                return SbmlUnitKind.Gram;
            } else if (str.Equals("second", StringComparison.OrdinalIgnoreCase)) {
                return SbmlUnitKind.Second;
            } else if (str.Equals("mole", StringComparison.OrdinalIgnoreCase)) {
                return SbmlUnitKind.Mole;
            }
            return SbmlUnitKind.Undefined;
        }
    }
}
