using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.SimulatedPopulations {
    public class PopulationSizeModelBuilder {

        public static IPopulationSizeModel Create(Population pop) {
            switch (pop.SizeUncertaintyDistribution) {
                case PopulationSizeDistributionType.Constant: {
                        return new PopulationSizeConstantModel(pop.Size);
                    }
                case PopulationSizeDistributionType.Normal: {
                        if (!pop.SizeUncertaintyUpper.HasValue) {
                            var msg = $"Missing upper population size normal uncertainty distribution for population {pop.Code}.";
                            throw new Exception(msg);
                        }
                        var distribution = NormalDistribution.FromMeanAndUpper(pop.Size, pop.SizeUncertaintyUpper.Value);
                        return new PopulationSizeDistributionModel<NormalDistribution>(distribution);
                    }
                case PopulationSizeDistributionType.Triangular: {
                        if (!pop.SizeUncertaintyLower.HasValue) {
                            var msg = $"Missing lower population size triangular uncertainty distribution for population {pop.Code}.";
                            throw new Exception(msg);
                        }
                        if (!pop.SizeUncertaintyUpper.HasValue) {
                            var msg = $"Missing upper population size triangular uncertainty distribution for population {pop.Code}.";
                            throw new Exception(msg);
                        }
                        var distribution = TriangularDistribution.FromModeLowerandUpper(pop.Size, pop.SizeUncertaintyLower.Value, pop.SizeUncertaintyUpper.Value);
                        return new PopulationSizeDistributionModel<TriangularDistribution>(distribution);
                    }
                default: {
                        var msg = $"No population size for distribution type ${pop.SizeUncertaintyDistribution}.";
                        throw new NotImplementedException(msg);
                    }
            }
        }
    }
}
