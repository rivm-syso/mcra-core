
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Actions.HumanMonitoringData {
    public class HumanMonitoringDataOutputData : IModuleOutputData {
        public ICollection<HumanMonitoringSurvey> HbmSurveys { get; set; }
        public ICollection<Individual> HbmIndividuals { get; set; }
        public ICollection<HumanMonitoringSample> HbmAllSamples { get; set; }
        public ICollection<HumanMonitoringSamplingMethod> HbmSamplingMethods { get; set; }
        public ICollection<HumanMonitoringSampleSubstanceCollection> HbmSampleSubstanceCollections { get; set; }
        public IModuleOutputData Copy() {
            return new HumanMonitoringDataOutputData() {
                HbmSurveys = HbmSurveys,
                HbmIndividuals = HbmIndividuals,
                HbmAllSamples = HbmAllSamples,
                HbmSamplingMethods = HbmSamplingMethods,
                HbmSampleSubstanceCollections = HbmSampleSubstanceCollections
            };
        }
    }
}

