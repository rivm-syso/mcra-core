using MCRA.Utils.ExtensionMethods;

namespace MCRA.General {


    public class ExposureTarget : IEquatable<ExposureTarget> {
        
        /// <summary>
        /// Static definition for dietary exposure target.
        /// </summary>
        public static ExposureTarget DietaryExposureTarget = new(ExposureRoute.Oral);

        /// <summary>
        /// Static definition for default internal exposure target
        /// (i.e., whole body internal model).
        /// </summary>
        public static ExposureTarget DefaultInternalExposureTarget = new(BiologicalMatrix.WholeBody);

        public ExposureTarget()  : this(ExposureRoute.Undefined) {
        }

        public ExposureTarget(
            BiologicalMatrix biologicalMatrix,
            ExpressionType expressionType = ExpressionType.None
        ) {
            ExposureRoute = ExposureRoute.Undefined;
            BiologicalMatrix = biologicalMatrix;
            ExpressionType = expressionType;
        }

        public ExposureTarget(
            ExposureRoute exposureRoute
        ) {
            ExposureRoute = exposureRoute;
            BiologicalMatrix = BiologicalMatrix.Undefined;
            ExpressionType = ExpressionType.None;
        }

        /// <summary>
        /// For external exposures, the exposure route.
        /// </summary>
        public ExposureRoute ExposureRoute { get; set; }

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
                if (ExposureRoute != ExposureRoute.Undefined) {
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
                if (TargetLevelType == TargetLevelType.External) {
                    if (ExposureRoute != ExposureRoute.Undefined) {
                        return ExposureRoute.ToString().ToLower();
                    } else {
                        return TargetLevelType.ToString().ToLower();
                    }
                } else {
                    if (BiologicalMatrix != BiologicalMatrix.Undefined) {
                        if (ExpressionType == ExpressionType.None) {
                            return BiologicalMatrix.ToString().ToLower();
                        } else {
                            return $"{BiologicalMatrix.ToString().ToLower()}-{ExpressionType.ToString().ToLower()}";
                        }
                    } else {
                        return TargetLevelType.ToString().ToLower();
                    }
                }
            } 
        }

        /// <summary>
        /// Identification code of this target.
        /// </summary>
        public string GetDisplayName() {
            if (TargetLevelType == TargetLevelType.External) {
                if (ExposureRoute != ExposureRoute.Undefined) {
                    return ExposureRoute.GetDisplayName();
                } else {
                    return TargetLevelType.GetDisplayName();
                }
            } else {
                if (BiologicalMatrix != BiologicalMatrix.Undefined) {
                    if (ExpressionType == ExpressionType.None) {
                        return BiologicalMatrix.GetDisplayName();
                    } else {
                        return $"{BiologicalMatrix.GetDisplayName()} ({ExpressionType.GetDisplayName()})";
                    }
                } else {
                    return TargetLevelType.GetDisplayName();
                }
            }
        }

        /// <summary>
        /// Override ToString method; returns the code.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Code;
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
            return other != null
                && TargetLevelType.Equals(other.TargetLevelType)
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

        /// <summary>
        /// Implement == operator.
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static bool operator ==(ExposureTarget b1, ExposureTarget b2) {
            if ((object)b1 == null) {
                return (object)b2 == null;
            }
            return b1.Equals(b2);
        }

        /// <summary>
        /// Implement != operator.
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static bool operator !=(ExposureTarget b1, ExposureTarget b2) {
            return !(b1 == b2);
        }
    }
}
