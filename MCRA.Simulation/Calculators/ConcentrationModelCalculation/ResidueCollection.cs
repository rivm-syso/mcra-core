using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.ConcentrationModelCalculation {

    /// <summary>
    /// Holds residue data with information on positives, censored, and other characteristics.
    /// </summary>
    public class ResidueCollection {

        private List<double> _censoredValues;
        private List<double> _nonDetectValues;
        private List<double> _nonQuantificationValues;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ResidueCollection() {
            Positives = [];
            CensoredValuesCollection = [];
        }

        /// <summary>
        /// The substance
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// The list of positive measurements.
        /// </summary>
        public List<double> Positives { get; set; }

        /// <summary>
        /// The LODs, LOQs and ResType for the given food/compound.
        /// </summary
        public List<CensoredValue> CensoredValuesCollection { get; set; }

        /// <summary>
        /// The number of zero measurements.
        /// </summary>
        public int ZerosCount { get; set; }

        /// <summary>
        /// Standard deviation of the specified food/compound combination.
        /// </summary>
        public double? StandardDeviation { get; set; }

        /// <summary>
        /// The list of censored value measurements (contains the LORs of the measurements, that is LOQ or LOD).
        /// </summary>
        public List<double> CensoredValues {
            get {
                if (_censoredValues == null) {
                    _censoredValues = CensoredValuesCollection
                        .Select(c => !double.IsNaN(c.LOQ) ? c.LOQ : c.LOD).ToList();
                    _censoredValues.Sort();
                }
                return _censoredValues;
            }
        }

        /// <summary>
        /// The list of non-detect value measurements (contains the LODs of the measurements).
        /// </summary>
        public List<double> NonDetectValues {
            get {
                if (_nonDetectValues == null) {
                    _nonDetectValues = CensoredValuesCollection
                        .Where(c => c.ResType == ResType.LOD)
                        .Select(c => c.LOD)
                        .ToList();
                    _nonDetectValues.Sort();
                }
                return _nonDetectValues;
            }
        }

        /// <summary>
        /// The list of non-quantifications value measurements (contains the LOQs of the measurements).
        /// </summary>
        public List<double> NonQuantificationValues {
            get {
                if (_nonQuantificationValues == null) {
                    _nonQuantificationValues = CensoredValuesCollection
                        .Where(c => c.ResType == ResType.LOQ)
                        .Select(c => c.LOQ)
                        .ToList();
                    _nonQuantificationValues.Sort();
                }
                return _nonQuantificationValues;
            }
        }

        /// <summary>
        /// Fraction of positives.
        /// </summary>
        public double FractionPositives {
            get {
                return (double)Positives.Count / NumberOfResidues;
            }
        }

        /// <summary>
        /// Fraction of zeros. Includes both NDs treated as zero and the zeros counts.
        /// </summary>
        public double FractionZeros {
            get {
                return (double)ZerosCount / NumberOfResidues;
            }
        }

        /// <summary>
        /// Fraction of censored values.
        /// </summary>
        public double FractionCensoredValues {
            get {
                return (double)CensoredValues.Count / NumberOfResidues;
            }
        }

        /// <summary>
        /// Fraction of nondetect values.
        /// </summary>
        public double FractionNonDetectValues {
            get {
                return (double)NonDetectValues.Count / NumberOfResidues;
            }
        }

        /// <summary>
        /// Fraction of nondetect values.
        /// </summary>
        public double FractionNonQuantificationValues {
            get {
                return (double)NonQuantificationValues.Count / NumberOfResidues;
            }
        }

        /// <summary>
        /// Total number of detects and censored values.
        /// </summary>
        public int NumberOfResidues {
            get {
                return Positives.Count + CensoredValues.Count + ZerosCount;
            }
        }

        /// <summary>
        /// Mean of all positives over all residues (number of detects and censored values).
        /// </summary>
        public double MeanAllresidues {
            get {
                return Positives.Sum() / NumberOfResidues;
            }
        }

        /// <summary>
        /// Gets the average non-detects value based on the provided non-detects handling method
        /// and the specified replacement fraction.
        /// </summary>
        public double GetAverageNonDetects(NonDetectsHandlingMethod nonDetectsHandlingMethod, double fractionOfLor) {
            if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                return CensoredValues.AverageOrZero() * fractionOfLor;
            } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                return CensoredValuesCollection
                    .AverageOrZero(c => {
                        var loq = !double.IsNaN(c.LOQ) ? c.LOQ: c.LOD;
                        var lod = !double.IsNaN(c.LOD) ? c.LOD : loq;
                        var result = c.ResType == ResType.LOD
                            ? lod * fractionOfLor
                            : lod + fractionOfLor * (loq - lod);
                        return result;
                    });
            } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZeroLOQSystem) {
                return CensoredValuesCollection
                    .AverageOrZero(c => c.ResType == ResType.LOD ? 0 : fractionOfLor * c.LOQ);
            } else if (nonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero) {
                return 0D;
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
