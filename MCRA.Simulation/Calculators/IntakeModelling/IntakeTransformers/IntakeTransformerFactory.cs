using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeTransformers {
    public class IntakeTransformerFactory {
        public static IntakeTransformer Create(
            TransformType transformType,
            Func<double> getPower
        ) {
            switch (transformType) {
                case TransformType.NoTransform:
                    return new IdentityTransformer();
                case TransformType.Logarithmic:
                    return new LogTransformer();
                case TransformType.Power:
                    var power = getPower();
                    if (power == 0) {
                        return new LogTransformer();
                    }
                    return new PowerTransformer() {
                        Power = power
                    };
                default:
                    throw new Exception($"Failed to create GH transformer: unknown transform type {transformType}.");
            }
        }
    }
}
