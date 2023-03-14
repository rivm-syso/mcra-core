using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections {

    /// <summary>
    /// This class holds a collection of sample compound records for a specified food.
    /// </summary>
    public sealed class HumanMonitoringSampleSubstanceCollection {

        /// <summary>
        /// The sampling method.
        /// </summary>
        public HumanMonitoringSamplingMethod SamplingMethod { get; set; }

        /// <summary>
        /// The sample substance records.
        /// </summary>
        public List<HumanMonitoringSampleSubstanceRecord> HumanMonitoringSampleSubstanceRecords { get; set; }

        public ConcentrationUnit TriglycConcentrationUnit { get; set; }
        public ConcentrationUnit CholestConcentrationUnit { get; set; }
        public ConcentrationUnit LipidConcentrationUnit { get; set; }
        public ConcentrationUnit CreatConcentrationUnit { get; set; }

        public HumanMonitoringSampleSubstanceCollection() {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HumanMonitoringSampleSubstanceCollection" /> class.
        /// </summary>
        /// <param name="hbmSamplingMethod"></param>
        /// <param name="hbmSampleSubstanceRecords"></param>
        public HumanMonitoringSampleSubstanceCollection(
            HumanMonitoringSamplingMethod hbmSamplingMethod,
            List<HumanMonitoringSampleSubstanceRecord> hbmSampleSubstanceRecords,
            ConcentrationUnit triglycConcentrationUnit,
            ConcentrationUnit cholestConcentrationUnit,
            ConcentrationUnit lipidConcentrationUnit,
            ConcentrationUnit creatConcentrationUnit
        ) {
            HumanMonitoringSampleSubstanceRecords = hbmSampleSubstanceRecords;
            SamplingMethod = hbmSamplingMethod;
            TriglycConcentrationUnit= triglycConcentrationUnit; 
            CholestConcentrationUnit= cholestConcentrationUnit;
            LipidConcentrationUnit= lipidConcentrationUnit;
            CreatConcentrationUnit= creatConcentrationUnit;
        }
    }
}
