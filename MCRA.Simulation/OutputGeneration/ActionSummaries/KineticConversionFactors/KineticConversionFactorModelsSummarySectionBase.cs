using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public abstract class KineticConversionFactorModelsSummarySectionBase<T> : SummarySection
        where T : KineticConversionFactorModelSummaryRecord, new()
    {

        public override bool SaveTemporaryData => true;

        public List<T> Records { get; set; } = [];

        /// <summary>
        /// Summarize kinetic conversion factors.
        /// </summary>
        public void Summarize(ICollection<IKineticConversionFactorModel> conversionFactorModels) {
            Records = getSummaryRecords(conversionFactorModels);
        }

        /// <summary>
        /// Summarize drawn model conversion factors of uncertainty runs.
        /// </summary>
        public void SummarizeUncertain(
            ICollection<IKineticConversionFactorModel> conversionFactorModels
        ) {
            var recordsLookup = Records.ToDictionary(r => r.GetKey());
            var records = getSummaryRecords(conversionFactorModels);
            foreach (var record in records) {
                if (recordsLookup.TryGetValue(record.GetKey(), out var sectionRecord)) {
                    sectionRecord.UncertaintyValues ??= [];
                    sectionRecord.UncertaintyValues.Add(record.NominalFactor);
                }
            }
        }

        protected static List<T> getSummaryRecords(
            ICollection<IKineticConversionFactorModel> conversionFactorModels
        ) {
            var records = new List<T>();
            foreach (var model in conversionFactorModels) {
                var result = getSummaryRecord(model);
                records.Add(result);
            }
            return records;
        }

        protected static T getSummaryRecord(IKineticConversionFactorModel model) {
            return new T() {
                SubstanceCodeFrom = model.SubstanceFrom.Code,
                SubstanceNameFrom = model.SubstanceFrom.Name,
                BiologicalMatrixFrom = model.TargetFrom.TargetLevelType == TargetLevelType.Internal
                    ? model.TargetFrom.BiologicalMatrix.GetDisplayName() : null,
                ExposureRouteFrom = model.TargetFrom.TargetLevelType == TargetLevelType.External
                    ? model.TargetFrom.ExposureRoute.GetDisplayName() : null,
                UnitFrom = model.UnitFrom.GetShortDisplayName(),
                ExpressionTypeFrom = model.TargetFrom.ExpressionType != ExpressionType.None
                    ? model.TargetFrom.ExpressionType.GetDisplayName() : null,
                SubstanceCodeTo = model.SubstanceTo.Code,
                SubstanceNameTo = model.SubstanceTo.Name,
                BiologicalMatrixTo = model.TargetTo.BiologicalMatrix != BiologicalMatrix.Undefined
                    ? model.TargetTo.BiologicalMatrix.GetDisplayName() : null,
                ExposureRouteTo = model.TargetTo.ExposureRoute != ExposureRoute.Undefined
                    ? model.TargetTo.ExposureRoute.GetDisplayName() : null,
                UnitTo = model.UnitTo.GetShortDisplayName(),
                ExpressionTypeTo = model.TargetTo.ExpressionType != ExpressionType.None
                    ? model.TargetTo.ExpressionType.GetDisplayName() : null,
                NominalFactor = model.GetConversionFactor(),
                HasCovariateAge = model.HasCovariate(KineticConversionFactorCovariateType.Age),
                HasCovariateSex = model.HasCovariate(KineticConversionFactorCovariateType.Sex)
            };
        }
    }
}