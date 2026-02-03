using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public enum KineticConversionFactorCovariateType {
        Age,
        Sex
    }

    public interface IKineticConversionFactorModel {

        Compound SubstanceTo { get; }

        Compound SubstanceFrom { get; }

        ExposureUnitTriple UnitTo { get; }

        ExposureUnitTriple UnitFrom { get; }

        ExposureTarget TargetFrom { get; }

        ExposureTarget TargetTo { get; }

        void CalculateParameters();

        void ResampleModelParameters(IRandom random);

        double GetConversionFactor(double? age = null, GenderType gender = GenderType.Undefined);

        bool HasCovariate(KineticConversionFactorCovariateType covariateType);

    }
}
