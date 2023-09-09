namespace MCRA.General {

    public class ExposureTarget : IEquatable<ExposureTarget> {

        public ExposureTarget(
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType
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
