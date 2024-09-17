using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public interface IKineticConversionFactorModel {

        bool UseSubgroups { get; set; }

        KineticConversionFactor ConversionRule { get; }

        void CalculateParameters();

        void ResampleModelParameters(IRandom random);

        double GetConversionFactor(double? age, GenderType gender);

        bool MatchesFromSubstance(Compound substance);

        bool IsSubstanceFromSpecific();
    }
}
