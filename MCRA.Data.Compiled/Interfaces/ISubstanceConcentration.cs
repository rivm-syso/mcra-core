using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Interfaces {
    public interface ISubstanceConcentration {
        Compound Substance { get; }
        double Concentration { get; }
        string UnitTypeString { get;  }
    }
}
