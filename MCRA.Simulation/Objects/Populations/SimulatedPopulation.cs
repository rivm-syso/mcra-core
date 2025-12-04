using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.SimulatedPopulations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Objects.Populations {
    public class SimulatedPopulation : Population {

        private readonly Population _inner;

        public SimulatedPopulation() { }

        // Immutable properties from inner
        public override string Code { get => _inner.Code; }
        public override string Name { get => _inner.Name; }
        public override string Description { get => _inner.Description; }
        public override string Location { get => _inner.Location; }
        public override DateTime? StartDate { get => _inner.StartDate; }
        public override DateTime? EndDate { get => _inner.EndDate; }
        public override double NominalBodyWeight { get => _inner.NominalBodyWeight; }
        public override PopulationSizeDistributionType SizeUncertaintyDistribution { get => _inner.SizeUncertaintyDistribution; }
        public override double? SizeUncertaintyLower { get => _inner.SizeUncertaintyLower; }
        public override double? SizeUncertaintyUpper { get => _inner.SizeUncertaintyUpper; }
        public override BodyWeightUnit BodyWeightUnit { get => _inner.BodyWeightUnit; }
        public override Dictionary<string, PopulationIndividualPropertyValue> PopulationIndividualPropertyValues { get => _inner.PopulationIndividualPropertyValues; }
        public override ICollection<PopulationCharacteristic> PopulationCharacteristics { get => _inner.PopulationCharacteristics; }

        // Mutable property
        public override double Size { get; set; }

        public SimulatedPopulation(Population population) {
            ArgumentNullException.ThrowIfNull(population);
            _inner = population;
            Size = population.Size;
        }

        public void ResamplePopulationSize(IRandom random) {
            var populationSizeModel = PopulationSizeModelBuilder.Create(_inner);
            Size = populationSizeModel.Draw(random);
        }
    }
}