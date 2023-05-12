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
    }
}
