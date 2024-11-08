using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// A data object holding information for a sample/compound measurement
    /// </summary>
    public sealed class SampleCompound {

        public SampleCompound() {
        }

        public SampleCompound(
            Compound compound,
            ResType resType,
            double residue,
            double lod,
            double loq,
            bool isExtrapolated = false
        ) {
            ActiveSubstance = compound;
            MeasuredSubstance = compound;
            ResType = resType;
            Residue = residue;
            Lod = lod;
            Loq = loq;
            IsExtrapolated = isExtrapolated;
        }

        /// <summary>
        /// The active substance to which the measurement applies.
        /// </summary>
        public Compound ActiveSubstance { get; set; }

        /// <summary>
        /// The measured substance from which this record is derived
        /// (can differ for complex residue definitions).
        /// </summary>
        public Compound MeasuredSubstance { get; set; }

        /// <summary>
        /// A boolean stating whether the measurement is a non-detect or non-quantification:  ResType = LOD or LOQ
        /// </summary>
        public bool IsCensoredValue => ResType == ResType.LOD || ResType == ResType.LOQ;
        /// <summary>
        /// Defines restype of concentration value (LOD, LOQ, MV or VAL)
        /// </summary>
        public ResType ResType { get; set; }

        /// <summary>
        /// A boolean stating whether the measurement is a non-detect value: ResType = LOD. f * lod
        /// </summary>
        public bool IsNonDetect => ResType == ResType.LOD;
        /// <summary>
        /// A boolean stating whether the measurement is a non-quantification: ResType = LOQ. lod + f (loq-lod)
        /// </summary>
        public bool IsNonQuantification => ResType == ResType.LOQ;

        /// <summary>
        /// A boolean that returns whether the measurement is a missing value.
        /// </summary>
        public bool IsMissingValue => ResType == ResType.MV;

        /// <summary>
        /// A double holding the measured concentration.
        /// </summary>
        public double Residue { get; set; }

        /// <summary>
        /// A double holding the value of the LOR for this measurement.
        /// In MCRA, LOR just means the limit below which no quantitative result has been reported.
        /// Depending on a laboratory's format of reporting, LOR may be a limit of detection (LOD),
        /// a limit of quantification (LOQ) or another limit.
        /// </summary>
        public double Lor => !double.IsNaN(Loq) ? Loq : Lod;

        /// <summary>
        /// A double holding the value of the LOD for this measurement, non-detect.
        /// </summary>
        public double Lod { get; set; }

        /// <summary>
        /// A double holding the value of the LOQ for this measurement, non-quantification.
        /// </summary>
        public double Loq { get; set; }

        /// <summary>
        /// A boolean stating whether this record originates from extrapolation.
        /// </summary>
        public bool IsExtrapolated { get; set; }

        /// <summary>
        /// Returns whether this sample compound record holds a positive measurement.
        /// I.e., not missing, not a censored value, and a residue value > 0.
        /// </summary>
        public bool IsPositiveResidue => !IsMissingValue && !IsCensoredValue && !double.IsNaN(Residue) && Residue > 0;

        /// <summary>
        /// Returns whether this sample compound record holds a zero concentration value.
        /// I.e., not a missing value, not a censored value, and a residue value equal to zero.
        /// </summary>
        public bool IsZeroConcentration => !IsMissingValue && !IsCensoredValue && Residue == 0;

        public SampleCompound Clone() {
            return new SampleCompound() {
                ActiveSubstance = ActiveSubstance,
                MeasuredSubstance = MeasuredSubstance,
                ResType = ResType,
                IsExtrapolated = IsExtrapolated,
                Lod = Lod,
                Loq = Loq,
                Residue = Residue
            };
        }
    }
}
