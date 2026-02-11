using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalTaskModelCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalTaskExposureModelsSection : ActionSummarySectionBase {
        public List<OccupationalTaskExposureModelRecord> Records { get; set; }

        public void Summarize(
            ICollection<IOccupationalTaskExposureModel> models
        ) {
            
            var records = new List<OccupationalTaskExposureModelRecord>();
            foreach (var r in models) {
                var record = new OccupationalTaskExposureModelRecord() {
                    TaskCode = r.Task.Code,
                    TaskName = r.Task.Name,
                    RpeType = r.Determinants.RPEType != RPEType.Undefined
                        ? r.Determinants.RPEType.GetShortDisplayName() : null,
                    HandProtectionType = r.Determinants.HandProtectionType != HandProtectionType.Undefined
                            ? r.Determinants.HandProtectionType.ToString() : null,
                    ProtectiveClothingType = r.Determinants.ProtectiveClothingType != ProtectiveClothingType.Undefined
                            ? r.Determinants.ProtectiveClothingType.ToString() : null,
                    ExposureRoute = r.Route.GetShortDisplayName(),
                    SubstanceCode = r.Substance.Code,
                    SubstanceName = r.Substance.Name,
                    ModelType = r.GetModelType(),
                    Unit = r.Unit.GetShortDisplayName(),
                    Value = r.GetNominal(),
                    ModelBasis = r.GetModelDescription()
                };
                records.Add(record);
            }

            Records = records
                .OrderBy(c => c.TaskName)
                .ThenBy(c => c.ExposureRoute)
                .ThenBy(c => c.SubstanceName)
                .ThenBy(c => c.RpeType)
                .ToList();
        }
    }
}
