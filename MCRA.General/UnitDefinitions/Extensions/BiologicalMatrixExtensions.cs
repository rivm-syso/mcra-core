namespace MCRA.General {
    public static class BiologicalMatrixUnitExtensions {

        private static readonly HashSet<BiologicalMatrix> _bloodMatrices = new HashSet<BiologicalMatrix>() {
            BiologicalMatrix.Blood,
            BiologicalMatrix.BloodPlasma,
            BiologicalMatrix.BloodSerum,
            BiologicalMatrix.CordBlood,
            BiologicalMatrix.VenousBlood,
            BiologicalMatrix.ArterialBlood,
            BiologicalMatrix.BrainBlood,
        };

        private static readonly HashSet<BiologicalMatrix> _urineMatrices = new HashSet<BiologicalMatrix>() {
            BiologicalMatrix.Urine
        };

        public static bool IsBlood(this BiologicalMatrix matrix) {
            return _bloodMatrices.Contains(matrix);
        }
        public static bool IsUrine(this BiologicalMatrix matrix) {
            return _urineMatrices.Contains(matrix);
        }

        public static bool IsUndefined(this BiologicalMatrix matrix) {
            return matrix == BiologicalMatrix.Undefined;
        }

        /// <summary>
        /// Returns a default target concentration unit, to align the per target biological matrix.
        /// </summary>
        public static ConcentrationUnit GetTargetConcentrationUnit(this BiologicalMatrix matrix) {
            return matrix switch {
                // Two exceptional cases
                BiologicalMatrix.Undefined => ConcentrationUnit.ugPerL,
                BiologicalMatrix.WholeBody => ConcentrationUnit.mgPerKg,

                // Concentration values that are expressed per gram mass unit
                BiologicalMatrix.Hair => ConcentrationUnit.ugPerg,
                BiologicalMatrix.ToeNails => ConcentrationUnit.ugPerg,
                BiologicalMatrix.BigToeNails => ConcentrationUnit.ugPerg,
                BiologicalMatrix.PlacentaTissue => ConcentrationUnit.ugPerg,
                BiologicalMatrix.OuterSkin => ConcentrationUnit.ugPerg,

                // Default for all other (aqueous) cases
                _ => ConcentrationUnit.ugPerL,
            };
        }
    }
}
