using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustIndividualDayExposure : IExternalIndividualDayExposure {
        public int SimulatedIndividualId { get; set; }
        public double IndividualSamplingWeight { get; set; }
        public Individual Individual { get; set; }
        public string Day { get; set; }
        public int SimulatedIndividualDayId { get; set; }

        public Dictionary<ExposureRoute, List<DustExposurePerSubstance>> ExposurePerSubstanceRoute { get; set; }

        public Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance =>
            ExposurePerSubstanceRoute
                .ToDictionary(item => item.Key.GetExposurePath(),
                item => item.Value
                    .Cast<IIntakePerCompound>()
                    .ToList() as ICollection<IIntakePerCompound>
                );

        public double GetSubstanceExposureForRoute(ExposurePathType route, Compound substance, bool isPerPerson) {
            throw new NotImplementedException();
        }

        public double GetTotalExternalExposure(IDictionary<Compound, double> rpfs, IDictionary<Compound, double> memberships, bool isPerPerson) {
            var totalExternalExposure = 0d;
            totalExternalExposure += ExposurePerSubstanceRoute
                .SelectMany(r => r.Value)
                .Where(r => r.Amount > 0)
                .Sum(r => r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]));
            return totalExternalExposure / (isPerPerson ? 1 : Individual.BodyWeight);
        }

        public double GetTotalExternalExposure(IDictionary<Compound, double> rpfs, IDictionary<Compound, double> memberships, IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors, bool isPerPerson) {
            throw new NotImplementedException();
        }

        public double GetTotalExternalExposureForSubstance(Compound substance, bool isPerPerson) {
            throw new NotImplementedException();
        }

        public double GetTotalExternalExposureForSubstance(Compound substance, IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors, bool isPerPerson) {
            throw new NotImplementedException();
        }

        public double GetTotalRouteExposure(ExposurePathType route, IDictionary<Compound, double> rpfs, IDictionary<Compound, double> memberships, bool isPerPerson) {
            throw new NotImplementedException();
        }

        public double GetTotalRouteExposure(ExposurePathType route, IDictionary<Compound, double> rpfs, IDictionary<Compound, double> memberships, IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors, bool isPerPerson) {
            throw new NotImplementedException();
        }

        public ICollection<IIntakePerCompound> GetTotalExposurePerCompound() {
            var exposurePerSubstance = ExposurePerSubstanceRoute
                .SelectMany(r => r.Value)
                .GroupBy(dipc => dipc.Compound)
                .Select(g => new AggregateIntakePerCompound() {
                    Amount = g.Sum(dipc => dipc.Amount),
                    Compound = g.Key,
                })
                .Cast<IIntakePerCompound>()
                .ToList();
            return exposurePerSubstance;
        }

        public ICollection<IIntakePerCompound> GetTotalExposurePerRouteSubstance(
            ExposureRoute exposureRoute
        ) {
            var exposuresPerSubstance = ExposurePerSubstanceRoute
                .Where(r => r.Key == exposureRoute)
                .SelectMany(r => r.Value)
                .GroupBy(ipc => ipc.Compound)
                .Select(g => new AggregateIntakePerCompound {
                    Compound = g.Key,
                    Amount = g.Sum(c => c.Amount),
                })
                .Cast<IIntakePerCompound>()
                .ToList();
            return exposuresPerSubstance;
        }

        public DustIndividualDayExposure Clone() {
            return new DustIndividualDayExposure() {
                SimulatedIndividualId = SimulatedIndividualId,
                SimulatedIndividualDayId = SimulatedIndividualDayId,
                IndividualSamplingWeight = IndividualSamplingWeight,
                Individual = Individual,
                Day = Day,
                ExposurePerSubstanceRoute = ExposurePerSubstanceRoute
            };
        }
    }
}


