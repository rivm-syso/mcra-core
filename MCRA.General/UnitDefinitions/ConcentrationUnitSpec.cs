using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {

    /// <summary>
    /// A discriminated union of concentration unit specifications, using pattern-matching records, where each case 
    /// can hold its own specific data. This facilitates support for shared code and use of switch expression logic.
    /// </summary>
    public abstract record ConcentrationUnitSpec;

    public sealed record StandardConcentrationUnitSpec(ConcentrationUnit Unit) : ConcentrationUnitSpec;
    public sealed record AirConcentrationUnitSpec(AirConcentrationUnit Unit) : ConcentrationUnitSpec;
    public sealed record ConsumerProductConcentrationUnitSpec(ConsumerProductConcentrationUnit Unit) : ConcentrationUnitSpec;


    public static class ConcentrationUnitSpecExtensions {
        public static string GetShortDisplayName(this ConcentrationUnitSpec spec)
            => spec switch {
                StandardConcentrationUnitSpec u => u.Unit.GetShortDisplayName(),
                AirConcentrationUnitSpec u => u.Unit.GetShortDisplayName(),
                ConsumerProductConcentrationUnitSpec u => u.Unit.GetShortDisplayName(),

                _ => ""
            };

        public static string ToSanitizedString(this ConcentrationUnitSpec spec)
           => spec switch {
               StandardConcentrationUnitSpec u => u.Unit.ToString(),
               AirConcentrationUnitSpec u => u.Unit.ToString(),
               ConsumerProductConcentrationUnitSpec u => u.Unit.ToString(),
               _ => ""
           };

        public static bool IsPerUnit(this ConcentrationUnitSpec spec)
            => spec switch {
                StandardConcentrationUnitSpec u => false,
                AirConcentrationUnitSpec u => false,
                ConsumerProductConcentrationUnitSpec u => u.Unit.IsPerUnit(),
                _ => false
            };

        public static double GetConcentrationAlignmentFactor(
            this ConcentrationUnitSpec specFrom,
            ConcentrationUnitSpec specTo,
            double molecularMass
        ) => (specFrom, specTo) switch {
            (StandardConcentrationUnitSpec f, StandardConcentrationUnitSpec t)
                => f.Unit.GetConcentrationAlignmentFactor(t.Unit, molecularMass),
            (AirConcentrationUnitSpec f, AirConcentrationUnitSpec t)
                => f.Unit.GetConcentrationAlignmentFactor(t.Unit, molecularMass),
            (ConsumerProductConcentrationUnitSpec f, ConsumerProductConcentrationUnitSpec t)
                => f.Unit.GetConcentrationAlignmentFactor(t.Unit, molecularMass),
            _ => throw new InvalidOperationException(
                $"Cannot compute alignment factor between {specFrom.GetType().Name} and {specTo.GetType().Name}.")
        };
    }
    
}
