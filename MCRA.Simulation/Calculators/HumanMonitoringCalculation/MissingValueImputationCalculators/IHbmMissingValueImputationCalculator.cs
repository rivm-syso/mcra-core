using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public interface IHbmMissingValueImputationCalculator {

        List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           IRandom random
        );
    }
}
