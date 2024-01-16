using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class ExternalTargetExposuresCalculator : ITargetExposuresCalculator {

        public ExternalTargetExposuresCalculator() {
        }

        public ICollection<TargetIndividualDayExposure> ComputeTargetIndividualDayExposures(
            ICollection<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple targetExposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> kineticModelInstances,
            ProgressState progressState
        ) {
            //Create an array of the external individual day exposures
            //so we can create a parallel for loop using an index
            var exposuresArray = externalIndividualDayExposures.ToArray();
            //create a result array with the same size, which will be
            //filled in the parallel loop using the abovementioned index
            var resultArray = new TargetIndividualDayExposure[exposuresArray.Length];

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1000 }; //, CancellationToken = cancelToken };

            Parallel.For(0, resultArray.Length, i => {
                var exposure = exposuresArray[i];
                var targetExposure = resultArray[i] = new TargetIndividualDayExposure() {
                    Individual = exposure.Individual,
                    IndividualSamplingWeight = exposure.IndividualSamplingWeight,
                    SimulatedIndividualId = exposure.SimulatedIndividualId,
                    SimulatedIndividualDayId = exposure.SimulatedIndividualDayId,
                    TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                    RelativeCompartmentWeight = 1D,
                };

                Dictionary<Compound, SubstanceTargetExposure> substExpSums = null;
                if (exposure.ExposuresPerRouteSubstance.TryGetValue(ExposurePathType.Dietary, out var eprs)) {
                    substExpSums = eprs
                        .GroupBy(s => s.Compound)
                        .ToDictionary(s => s.Key, s => new SubstanceTargetExposure(s.Key, s.Sum(c => c.Exposure)));
                }

                foreach (var subst in substances) {
                    if (substExpSums == null || !substExpSums.TryGetValue(subst, out var exposureSum)) {
                        exposureSum = new SubstanceTargetExposure { Substance = subst, SubstanceAmount = 0D };
                    }
                    targetExposure.TargetExposuresBySubstance.Add(subst, exposureSum);
                }
            });
            return resultArray;
        }

        public ICollection<TargetIndividualExposure> ComputeTargetIndividualExposures(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            Compound indexSubstance,
            ICollection<ExposurePathType> exposureRoutes,
            ExposureUnitTriple targetExposureUnit,
            IRandom generator,
            ICollection<KineticModelInstance> KineticModelInstances,
            ProgressState progressState
        ) {
            //Create an array of the external individual day exposures
            //so we can create a parallel for loop using an index
            var exposuresArray = externalIndividualExposures.ToArray();
            //create a result array with the same size, which will be
            //filled in the parallel loop using the abovementioned index
            var resultArray = new TargetIndividualExposure[exposuresArray.Length];

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 1000 }; //, CancellationToken = cancelToken };

            Parallel.For(0, resultArray.Length, i => {
                var exposure = exposuresArray[i];
                var targetExposure = resultArray[i] = new TargetIndividualExposure() {
                    Individual = exposure.Individual,
                    IndividualSamplingWeight = exposure.IndividualSamplingWeight,
                    SimulatedIndividualId = exposure.SimulatedIndividualId,
                    TargetExposuresBySubstance = new Dictionary<Compound, ISubstanceTargetExposure>(),
                    RelativeCompartmentWeight = 1D
                };

                Dictionary<Compound, SubstanceTargetExposure> substExpSums = null;
                if (exposure.ExposuresPerRouteSubstance.TryGetValue(ExposurePathType.Dietary, out var eprs)) {
                    substExpSums = eprs
                        .GroupBy(s => s.Compound)
                        .ToDictionary(s => s.Key, s => new SubstanceTargetExposure(s.Key, s.Sum(c => c.Exposure)));
                }

                foreach (var subst in substances) {
                    if (substExpSums == null || !substExpSums.TryGetValue(subst, out var exposureSum)) {
                        exposureSum = new SubstanceTargetExposure { Substance = subst, SubstanceAmount = 0D };
                    }
                    targetExposure.TargetExposuresBySubstance.Add(subst, exposureSum);
                }
            });
            return resultArray;
        }

        public IDictionary<(ExposurePathType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            List<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ExposureUnitTriple targetExposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var result = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var substance in substances) {
                result.Add((ExposurePathType.Dietary, substance), 1D);
            }
            return result;
        }

        public IDictionary<(ExposurePathType, Compound), double> ComputeKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> exposureRoutes,
            List<AggregateIndividualExposure> aggregateIndividualExposures,
            ExposureUnitTriple targetExposureUnit,
            double nominalBodyWeight,
            IRandom generator
        ) {
            var result = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var substance in substances) {
                result.Add((ExposurePathType.Dietary, substance), 1D);
            }
            return result;
        }
    }
}
