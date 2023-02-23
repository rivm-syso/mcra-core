using System.Text.RegularExpressions;

namespace MCRA.General {

    public static class ProbabilityDistributionConverter {
        private static string[] _lognormal = new string[] { "LOGNORMAL" };
        private static string[] _normal = new string[] { "NORMAL" };
        private static string[] _logistic = new string[] { "LOGISTICNORMAL" };
        public static ProbabilityDistribution FromString(string typeString) {
            if (string.IsNullOrEmpty(typeString)) {
                return ProbabilityDistribution.Deterministic;
            }
            var str = Regex.Replace(typeString, @"\s", "");
            if (string.IsNullOrEmpty(str)) {
                return ProbabilityDistribution.Deterministic;
            } else if (_lognormal.Contains(str, StringComparer.OrdinalIgnoreCase)) {
                return ProbabilityDistribution.LogNormal;
            } else if (_logistic.Contains(str, StringComparer.OrdinalIgnoreCase)) {
                return ProbabilityDistribution.LogisticNormal;
            } else if (_normal.Contains(str, StringComparer.OrdinalIgnoreCase)) {
                return ProbabilityDistribution.Normal;
            } else {
                throw new Exception($"Unrecognized distribution type \"{typeString}\" ");
            }
        }

    }
}
