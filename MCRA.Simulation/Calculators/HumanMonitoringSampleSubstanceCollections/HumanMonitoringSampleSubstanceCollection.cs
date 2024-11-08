using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections {

    /// <summary>
    /// This class holds a collection of sample compound records for a specified food.
    /// </summary>
    public sealed class HumanMonitoringSampleSubstanceCollection {

        /// <summary>
        /// Initializes a new instance of the <see cref="HumanMonitoringSampleSubstanceCollection" /> class.
        /// </summary>
        /// <param name="hbmSamplingMethod"></param>
        /// <param name="hbmSampleSubstanceRecords"></param>
        public HumanMonitoringSampleSubstanceCollection(
            HumanMonitoringSamplingMethod hbmSamplingMethod,
            List<HumanMonitoringSampleSubstanceRecord> hbmSampleSubstanceRecords,
            ConcentrationUnit targetConcentrationUnit,
            ExpressionType expressionType,
            ConcentrationUnit triglycConcentrationUnit,
            ConcentrationUnit cholestConcentrationUnit,
            ConcentrationUnit lipidConcentrationUnit,
            ConcentrationUnit creatConcentrationUnit
        ) {
            HumanMonitoringSampleSubstanceRecords = hbmSampleSubstanceRecords;
            SamplingMethod = hbmSamplingMethod;
            ConcentrationUnit = targetConcentrationUnit;
            ExpressionType = expressionType;
            TriglycConcentrationUnit = triglycConcentrationUnit;
            CholestConcentrationUnit = cholestConcentrationUnit;
            LipidConcentrationUnit = lipidConcentrationUnit;
            CreatConcentrationUnit = creatConcentrationUnit;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HumanMonitoringSampleSubstanceCollection" /> class.
        /// </summary>
        public HumanMonitoringSampleSubstanceCollection() {
        }

        /// <summary>
        /// The sampling method.
        /// </summary>
        public HumanMonitoringSamplingMethod SamplingMethod { get; set; }

        /// <summary>
        /// Gets the biological matrix of the sampling method.
        /// </summary>
        public BiologicalMatrix BiologicalMatrix => SamplingMethod.BiologicalMatrix;

        /// <summary>
        /// If applicable, specifies how the sample concentration is standardised or otherwise expressed, different from the default, which
        /// is a sample concentration per unit volume.
        /// </summary>
        public ExpressionType ExpressionType { get; set; } = ExpressionType.None;

        /// <summary>
        /// The target concentration unit of the sample concentrations in this collection.
        /// </summary>
        public ConcentrationUnit ConcentrationUnit { get; set; }

        /// <summary>
        /// If applicable (i.e., for blood samples), the concentration unit for triglycerides
        /// measurements of the samples in this collection.
        /// </summary>
        public ConcentrationUnit TriglycConcentrationUnit { get; set; }

        /// <summary>
        /// If applicable, the concentration unit of cholesterol measurements of the samples in
        /// this collection.
        /// </summary>
        public ConcentrationUnit CholestConcentrationUnit { get; set; }

        /// <summary>
        /// If applicable, the concentration unit of the lipid measurements of the samples in
        /// this collection.
        /// </summary>
        public ConcentrationUnit LipidConcentrationUnit { get; set; }

        /// <summary>
        /// If applicable, the concentration unit of creatinine of the samples in this collection.
        /// </summary>
        public ConcentrationUnit CreatConcentrationUnit { get; set; }

        /// <summary>
        /// The sample substance records.
        /// </summary>
        public List<HumanMonitoringSampleSubstanceRecord> HumanMonitoringSampleSubstanceRecords { get; set; }

        public HumanMonitoringSampleSubstanceCollection Clone() {
            return new HumanMonitoringSampleSubstanceCollection() {
                SamplingMethod = SamplingMethod,
                CholestConcentrationUnit = CholestConcentrationUnit,
                ConcentrationUnit = ConcentrationUnit,
                CreatConcentrationUnit = CreatConcentrationUnit,
                ExpressionType = ExpressionType,
                LipidConcentrationUnit = LipidConcentrationUnit,
                TriglycConcentrationUnit = TriglycConcentrationUnit,
                HumanMonitoringSampleSubstanceRecords = HumanMonitoringSampleSubstanceRecords.Select(h => h.Clone()).ToList()
            };
        }
    }
}
