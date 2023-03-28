using MCRA.General;

namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// .
    /// </summary>
    public sealed class HbmSubstanceTargetUnit : IEquatable<HbmSubstanceTargetUnit> {

        public Compound Substance { get; set; }

        public TargetUnit TargetUnit { get; set; }    

        public bool Equals(HbmSubstanceTargetUnit other) {
            return this.Substance.Code.Equals(other.Substance.Code)
                    && this.TargetUnit.BiologicalMatrix.Equals(other.TargetUnit.BiologicalMatrix)
                    && this.TargetUnit.ExpressionType.Equals(other.TargetUnit.ExpressionType);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Substance.Code, TargetUnit.BiologicalMatrix, TargetUnit.ExpressionType);
        }
    }
}
