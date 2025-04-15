using MCRA.General;

namespace MCRA.Simulation.Objects {
    /// <summary>
    /// Represents a unique (key) combination of one exposure source and one exposure route, for external exposures.
    /// Not to be confused with the exposure pathway definition from aggregate modelling, which represents more the 
    /// course a substance takes.
    /// </summary>
    public class ExposurePath(
        ExposureSource source,
        ExposureRoute route
        ) : IEquatable<ExposurePath> {

        public ExposureSource Source => source;
        public ExposureRoute Route => route;

        /// <summary>
        /// Identification code of this path.
        /// </summary>
        public string Code {
            get {
                var code = "";
                if (Source != ExposureSource.Undefined) {
                    code = Source.ToString().ToLower();
                }
                if (Route != ExposureRoute.Undefined) {
                    if (!string.IsNullOrEmpty(code)) {
                        code += "-";
                    }
                    code += Route.ToString().ToLower();
                }
                return code;
            }
        }
        public override string ToString() {
            return Code;
        }

        /// <summary>
        /// Checks for equality, type safe.
        /// </summary>
        public bool Equals(ExposurePath other) {
            if (other is null) {
                return false;
            }
            return Route == other.Route && Source == other.Source;
        }

        /// <summary>
        /// Checks for equality, non-type safe.
        /// </summary>
        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (obj is not ExposurePath other) {
                return false;
            } else {
                return Equals(other);
            }
        }
        public static bool operator ==(ExposurePath b1, ExposurePath b2) {
            if ((object)b1 == null) {
                return (object)b2 == null;
            }
            return b1.Equals(b2);
        }

        public static bool operator !=(ExposurePath b1, ExposurePath b2) {
            return !(b1 == b2);
        }
        public override int GetHashCode() => HashCode.Combine(Source, Route);
    }
}
