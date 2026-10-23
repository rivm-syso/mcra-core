using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;

namespace MCRA.Simulation.OutputGeneration {
    public class HbmIndividualDayKineticConversionBySubstanceSection : HbmKineticConversionBySubstanceSectionBase {

        public void Summarize(
            ICollection<HbmIndividualDayCollection> convertedCollections,
            ICollection<HbmIndividualDayCollection> measuredCollections,
            ICollection<Compound> substances,
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            var kcfMatrices = kineticConversionFactorModels?.Select(c => c.TargetFrom.BiologicalMatrix).ToHashSet() ?? [];
            var kmiMatrices = kineticModelInstances?.SelectMany(c => c.KineticModelDefinition.GetOutputMatrices()).ToHashSet() ?? [];
            var matrices = kmiMatrices.Union(kcfMatrices).ToList();

            // For setting HbmConvertToSingleTargetMatrix = false, the converted collection also contains the original (measured) collection
            // So remve this collection first();
            var collections = convertedCollections.Count > 1
                ? [.. convertedCollections.ExceptBy(measuredCollections.Select(c => c.Target), c => c.Target)]
                : convertedCollections;

            var convertedExposures = collections
                .SelectMany(c => c.HbmIndividualDayConcentrations,
                    (c, d) => new HbmConcentrationBySubstanceCollection() {
                        ExposureTarget = c.Target,
                        TargetUnit = c.TargetUnit,
                        Id = d.SimulatedIndividualDayId,
                        ConcentrationsBySubstance = [.. d.ConcentrationsBySubstance.Select(r => (r.Key, r.Value.Exposure))]
                    }
                ).ToList();

            // Remove collections that are not converted.
            var measuredExposures = measuredCollections
                .Where(c => matrices.Contains(c.Target.BiologicalMatrix))
                .SelectMany(c => c.HbmIndividualDayConcentrations,
                    (c, d) => new HbmConcentrationBySubstanceCollection() {
                        ExposureTarget = c.Target,
                        TargetUnit = c.TargetUnit,
                        Id = d.SimulatedIndividualDayId,
                        ConcentrationsBySubstance = [.. d.ConcentrationsBySubstance.Select(r => (r.Key, r.Value.Exposure))]
                    }
                ).ToList();

            var kineticModelTypes = GetKineticModelType(
                substances,
                measuredCollections.Select(c => c.Target.BiologicalMatrix).ToList(),
                kineticConversionFactorModels,
                kineticModelInstances
            );

            // The measured targets are the targets for which we have measured concentrations (HBM),
            // The assumption is that the converted exposures are aggregated into one target.
            Records = Summarize(
                convertedExposures,
                measuredExposures,
                substances,
                kineticModelTypes
            );
            KineticModelTypes = kineticModelTypes;
        }
    }
}
