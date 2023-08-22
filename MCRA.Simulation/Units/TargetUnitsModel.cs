using MCRA.Data.Compiled.Objects;
using MCRA.General;
using static MCRA.General.TargetUnit;

namespace MCRA.Simulation.Units {

    /// <summary>
    /// Collects the target units per substance, for one module.
    /// </summary>
    public class TargetUnitsModel {

        public Dictionary<TargetUnit, HashSet<Compound>> SubstanceTargetUnits { get; set; } 
            = new Dictionary<TargetUnit, HashSet<Compound>>(new TargetUnitComparer());

        public TargetUnit Add(Compound substance, TargetUnit targetUnit) {
            if (!SubstanceTargetUnits.TryGetValue(targetUnit, out var substances)) {
                substances = new HashSet<Compound>();
                SubstanceTargetUnits.Add(targetUnit, substances);
            }
            substances.Add(substance);
            return targetUnit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="substance"></param>
        /// <param name="biologicalMatrixFrom"></param>
        /// <param name="targetUnitTo"></param>
        public void Update(Compound substance, BiologicalMatrix biologicalMatrixFrom, TargetUnit targetUnitTo) {
            if (biologicalMatrixFrom != targetUnitTo.BiologicalMatrix
                // Only check when converting from one matrix to another
                && TryGetTargetUnit(targetUnitTo.BiologicalMatrix, out var targetUnit, c => c == substance)) {
                // Already in correct biological matrix
                RemoveWhere(biologicalMatrixFrom, s => s == substance);
                return;
            }
            RemoveWhere(biologicalMatrixFrom, s => s == substance);
            Add(substance, targetUnitTo);
        }

        /// <summary>
        /// This method gets the target unit based on substance code as string. 
        /// This is not the best for performance. It is used in the output generation
        /// where only the substance code is known but not the substance object instance.
        /// </summary>
        public bool TryGetTargetUnitBySubstanceCode(BiologicalMatrix biologicalMatrix, out TargetUnit targetUnit, out Compound value, string findSubstanceCode) {
            return TryGetTargetUnit(biologicalMatrix, out targetUnit, out value, s => s.Code == findSubstanceCode);
        }

        public TargetUnit GetUnit(Compound substance, BiologicalMatrix biologicalMatrix) {
            bool result = TryGetTargetUnit(biologicalMatrix, out var targetUnit, out var Substance, c => c == substance);
            System.Diagnostics.Debug.Assert(result);

            return targetUnit;
        }

        /// <summary>
        /// Get all target units for a given substance. A substance may have more than one target unit.
        /// </summary>
        public List<TargetUnit> GetTargetUnits(Compound substance) {
            return SubstanceTargetUnits.Where(kv => kv.Value.Any(c => c == substance)).Select(kv => kv.Key).ToList();
        }

        public string GetTargetUnitString(Compound substance, BiologicalMatrix biologicalMatrix) {
            if (TryGetTargetUnit(biologicalMatrix, out TargetUnit targetUnit, out Compound substanceFound, s => s == substance)) {
                return targetUnit.GetShortDisplayName(DisplayOption.AppendExpressionType);
            }
            return "<unit not found>";
        }

        private void RemoveWhere(BiologicalMatrix biologicalMatrix, Predicate<Compound> predicate) {
            foreach (var kvp in SubstanceTargetUnits) {
                if (kvp.Key.BiologicalMatrix != biologicalMatrix) {
                    continue;
                }
                var hashSetValues = kvp.Value;

                hashSetValues.RemoveWhere(predicate);
            }
        }

        private bool TryGetTargetUnit(BiologicalMatrix biologicalMatrix, out TargetUnit targetUnit, Func<Compound, bool> predicate) {
            return TryGetTargetUnit(biologicalMatrix, out targetUnit, out Compound value, predicate);
        }

        private bool TryGetTargetUnit(BiologicalMatrix biologicalMatrix, out TargetUnit targetUnit, out Compound value, Func<Compound, bool> predicate) {
            targetUnit = null;
            value = default;
            foreach (var kv in SubstanceTargetUnits) {
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
    }
}
