using static MCRA.General.TargetUnit;

namespace MCRA.General {
    public static class TargetUnitExtensions {

        public static void NewOrAdd<T>(this Dictionary<TargetUnit, HashSet<T>> dict, TargetUnit key, T value) {
            if (!dict.ContainsKey(key)) {
                dict[key] = new HashSet<T>();
            }
            dict[key].Add(value);
        }

        public static void RemoveWhere<T>(this Dictionary<TargetUnit, HashSet<T>> dict, BiologicalMatrix biologicalMatrix, Predicate<T> predicate) {
            foreach (var kvp in dict) {
                if (kvp.Key.BiologicalMatrix != biologicalMatrix) {
                    continue;
                }
                var hashSetValues = kvp.Value;

                hashSetValues.RemoveWhere(predicate);
            }
        }

        public static bool TryGetTargetUnit<T>(this Dictionary<TargetUnit, HashSet<T>> dict, BiologicalMatrix biologicalMatrix, out TargetUnit targetUnit, out T value, Func<T, bool> predicate) {
            targetUnit = null;
            value = default(T);
            foreach (var kv in dict) {
                if (kv.Key.BiologicalMatrix != biologicalMatrix) {
                    continue;
                }
                var found = kv.Value.FirstOrDefault(s => predicate(s));
                if (found != null) {
                    targetUnit = kv.Key;
                    value = found;

                    return true;
                }
            }
            return false;
        }

        public static string GetTargetUnitString<T>(this Dictionary<TargetUnit, HashSet<T>> dict, BiologicalMatrix biologicalMatrix, Func<T, bool> predicate) {
            if (dict.TryGetTargetUnit(biologicalMatrix, out TargetUnit targetUnit, out T substanceFound, predicate)) {
                return targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType);
            }
            return "<unit not found>";
        }
    }
}
