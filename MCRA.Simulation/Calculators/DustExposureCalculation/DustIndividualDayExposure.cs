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
        public ExposureUnitTriple ExposureUnit { get; set; }

        public Dictionary<ExposureRoute, List<DustExposurePerSubstance>> ExposurePerSubstanceRoute { get; set; }

        public Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance => throw new NotImplementedException();

        public double GetSubstanceExposureForRoute(ExposurePathType route, Compound substance, bool isPerPerson) {
            throw new NotImplementedException();
        }       

        public double GetTotalExternalExposure(IDictionary<Compound, double> rpfs, IDictionary<Compound, double> memberships, bool isPerPerson) {
            throw new NotImplementedException();
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
    }
}


