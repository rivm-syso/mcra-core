
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Actions.HumanMonitoringData {
    public class HumanMonitoringDataOutputData : IModuleOutputData {
        public ICollection<HumanMonitoringSurvey> HbmSurveys { get; set; }
        public ICollection<Individual> HbmIndividuals { get; set; }
        public ICollection<HumanMonitoringSample> HbmSamples { get; set; }
        public ICollection<HumanMonitoringSamplingMethod> HbmSamplingMethods { get; set; }
        public ICollection<HumanMonitoringSampleSubstanceCollection> HbmSampleSubstanceCollections { get; set; }
        public IModuleOutputData Copy() {
            return new HumanMonitoringDataOutputData() {
                HbmSurveys = HbmSurveys,
                HbmIndividuals = HbmIndividuals,
                HbmSamples = HbmSamples,
                HbmSamplingMethods = HbmSamplingMethods,
                HbmSampleSubstanceCollections = HbmSampleSubstanceCollections
            };
        }
    }
}

