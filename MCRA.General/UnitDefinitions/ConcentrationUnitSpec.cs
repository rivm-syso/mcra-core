using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {

    public abstract record ConcentrationUnitSpec;

    public sealed record StandardConcentrationUnitSpec(ConcentrationUnit Unit) : ConcentrationUnitSpec;

    public sealed record AirConcentrationUnitSpec(AirConcentrationUnit Unit) : ConcentrationUnitSpec;


    public static class ConcentrationUnitSpecExtensions {
        public static string GetShortDisplayName(this ConcentrationUnitSpec spec)
            => spec switch {
                StandardConcentrationUnitSpec u => u.Unit.GetShortDisplayName(),
                AirConcentrationUnitSpec u => u.Unit.GetShortDisplayName(),
                _ => ""
            };
    }
}
