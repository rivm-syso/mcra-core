namespace MCRA.General {

    public abstract record ConcentrationUnitSpec;

    public sealed record StandardConcentrationUnit(ConcentrationUnit Unit) : ConcentrationUnitSpec;

    public sealed record AirConcentrationUnitSpec(AirConcentrationUnit Unit) : ConcentrationUnitSpec;
}
