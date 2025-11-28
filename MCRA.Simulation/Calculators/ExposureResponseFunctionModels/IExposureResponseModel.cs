using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctions {

    public interface IExposureResponseModel {

        string Code { get; }

        string Name { get; }

        TargetUnit TargetUnit { get; }

        Compound Substance { get; }

        Effect Effect { get; }

        ExposureResponseFunction ExposureResponseFunction { get; }

        EffectMetric EffectMetric { get; }

        ExposureResponseType ExposureResponseType { get; }

        PopulationCharacteristicType PopulationCharacteristic { get; }

        double? EffectThresholdLower { get; }

        double? EffectThresholdUpper { get; }

        double GetCounterFactualValue();

        double Compute(double exposure, bool useErfBins);

        bool HasErfSubGroups { get; }

        List<double> SubGroupLevels { get; }

        void ResampleExposureResponseFunction(IRandom random);

        void ResampleCounterFactualValue(IRandom random);
    }
}
