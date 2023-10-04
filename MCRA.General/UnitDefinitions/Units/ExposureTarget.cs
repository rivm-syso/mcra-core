namespace MCRA.General {


    public class ExposureTarget : IEquatable<ExposureTarget> {
        public ExposureTarget() {
        }

        /// <summary>
        /// Static definition for dietary exposure target.
        /// </summary>
        public static ExposureTarget DietaryExposureTarget
            = new(ExposureRouteType.Dietary);

        /// <summary>
        /// Static definition for default internal exposure target
        /// (i.e., whole body internal model).
        /// </summary>
        public static ExposureTarget DefaultInternalExposureTarget 
            = new(BiologicalMatrix.WholeBody, ExpressionType.None);

        public ExposureTarget(
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType = ExpressionType.None
        ) {
            ExposureRoute = ExposureRouteType.AtTarget;
            BiologicalMatrix = biologicalMatrix;
            ExpressionType = expressionType;
        }

        public ExposureTarget(
            ExposureRouteType exposureRoute
        ) {
            ExposureRoute = exposureRoute;
            BiologicalMatrix = BiologicalMatrix.Undefined;
            ExpressionType = ExpressionType.None;
        }

        /// <summary>
        /// For external exposures, the exposure route.
        /// </summary>
        public ExposureRouteType ExposureRoute { get; set; }

        /// <summary>
        /// For internal exposures, the biological matrix. May be internal
        /// organs, (e.g., liver) or body fluids (e.g., blood/urine).
        /// </summary>
        public BiologicalMatrix BiologicalMatrix { get; set; }
        
        /// <summary>
        /// The expression type, e.g., "lipids", "creatinine".
        /// </summary>
        public ExpressionType ExpressionType { get; set; }

        /// <summary>
        /// Gets the target level type. I.e., internal or external.
        /// </summary>
        public TargetLevelType TargetLevelType {
            get {
                if (ExposureRoute != ExposureRouteType.Undefined
                    && ExposureRoute != ExposureRouteType.AtTarget) {
                    return TargetLevelType.External;
                } else {
                    return TargetLevelType.Internal;
                }
            }
        }

        /// <summary>
        /// Identification code of this target.
        /// </summary>
        public string Code {
            get {
                return CreateUnitKey(
                    (BiologicalMatrix, ExpressionType)
                );
            } 
        }

        public static string CreateUnitKey((BiologicalMatrix BiologicalMatrix, ExpressionType ExpressionType) key) {
            if (key.ExpressionType == ExpressionType.None) {
                return $"{key.BiologicalMatrix.ToString().ToLower()}";
            } else {
                return $"{key.BiologicalMatrix.ToString().ToLower()}:{key.ExpressionType.ToString().ToLower()}";
            }
        }

        /// <summary>
        /// Override ToString method.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            if (TargetLevelType == TargetLevelType.External) {
                return ExposureRoute.ToString();
            } else {
                return $"{BiologicalMatrix}:{ExpressionType}";
            }
        }

        /// <summary>
        /// Override equals.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (obj is not ExposureTarget other) {
                return false;
            } else {
                return Equals(other);
            }
        }

        /// <summary>
        /// Checks for equality.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(ExposureTarget other) {
            return TargetLevelType.Equals(other.TargetLevelType)
                && ExposureRoute.Equals(other.ExposureRoute)
                && BiologicalMatrix.Equals(other.BiologicalMatrix)
                && ExpressionType.Equals(other.ExpressionType);
        }

        /// <summary>
        /// Implements hash code method.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return HashCode.Combine(
                TargetLevelType,
                ExposureRoute,
                BiologicalMatrix,
                ExpressionType
            );
        }
    }
}
