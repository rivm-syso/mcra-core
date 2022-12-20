using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Base clas for transforming a set of positive daily exposures to
    /// normality (e.g. using the power or log).
    /// </summary>
    public abstract class IntakeTransformer {
        public abstract TransformType TransformType { get; }
        public abstract double Transform(double x);
        public abstract double InverseTransform(double x);
        public abstract double BiasCorrectedInverseTransform(double x, double varianceWithin);
    }
}
