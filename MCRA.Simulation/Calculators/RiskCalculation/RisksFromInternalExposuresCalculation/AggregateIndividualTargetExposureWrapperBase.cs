using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.Calculators.RiskCalculation {
    public abstract class AggregateIndividualTargetExposureWrapperBase<T> : ITargetIndividualExposure
        where T : AggregateIndividualExposure {

        protected readonly T _aggregateIndividualExposure;

        protected readonly TargetUnit _targetUnit;

        public IDictionary<Compound, ISubstanceTargetExposure> TargetExposuresBySubstance { get; set; }

        public AggregateIndividualTargetExposureWrapperBase(
            T aggregateIndividualDayExposure,
            TargetUnit targetUnit
        ) {
            _targetUnit = targetUnit;
            _aggregateIndividualExposure = aggregateIndividualDayExposure;
            TargetExposuresBySubstance = aggregateIndividualDayExposure
                .InternalTargetExposures[targetUnit.Target];
        }

        public ICollection<Compound> Substances {
            get {
                return TargetExposuresBySubstance.Keys;
            }
        }

        public double IndividualSamplingWeight {
            get {
                return _aggregateIndividualExposure.IndividualSamplingWeight;
            }
        }

        public Individual Individual {
            get {
                return _aggregateIndividualExposure.Individual;
            }
        }

        public double IntraSpeciesDraw { get ; set; }

        public int SimulatedIndividualId {
            get {
                return _aggregateIndividualExposure.SimulatedIndividualId;
            }
        }

        public double SimulatedIndividualBodyWeight => Individual.BodyWeight;

        public double GetSubstanceExposure(
             Compound substance
        ) {
            var result = _aggregateIndividualExposure
                .GetSubstanceTargetExposure(_targetUnit.Target, substance)?.Exposure ?? 0D;
            return result;
        }

        public double GetSubstanceExposure(
            Compound substance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            return TargetExposuresBySubstance.ContainsKey(substance)
                ? TargetExposuresBySubstance[substance].EquivalentSubstanceExposure(relativePotencyFactors[substance], membershipProbabilities[substance])
                : 0D;
        }

        public ISubstanceTargetExposure GetSubstanceTargetExposure(Compound compound) {
            return _aggregateIndividualExposure
                .GetSubstanceTargetExposure(_targetUnit.Target, compound);
        }

        public bool IsPositiveExposure() {
            throw new NotImplementedException();
        }

        public double GetCumulativeExposure(
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities
        ) {
            var result = _aggregateIndividualExposure
                .GetTotalExposureAtTarget(
                    _targetUnit.Target,
                    relativePotencyFactors,
                    membershipProbabilities
                );
            return result;
        }
    }
}
