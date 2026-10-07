using MCRA.General;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ErfDataSubgroupSummarySection : ActionSummarySectionBase {

        public List<ErfDataSubgroupSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<IExposureResponseModel> exposureResponseFunctionModels) {
            var exposureResponseFunctions = exposureResponseFunctionModels
                .Select(r => r.ExposureResponseFunction)
                .ToList();

            Records = [.. exposureResponseFunctions
                .SelectMany(r => r.ErfSubgroups
                .Select(s => {
                    var record = new ErfDataSubgroupSummaryRecord {
                        ExposureResponseFunctionCode = r.Code,
                        ErSubgroupCode = s.idSubgroup,
                        ExposureUpper = s.ExposureUpper,
                        ErSpecificationUncertaintyType = s.ERFUncertaintyDistribution != ExposureResponseSpecificationDistributionType.Constant
                            ? s.ERFUncertaintyDistribution.GetShortDisplayName()
                            : null,
                        ExposureResponseSpecification = s.ExposureResponseSpecification.ExpressionString,
                        ErSpecificationUncLower = s.ExposureResponseSpecificationLower.ExpressionString.Length != 0
                            ? s.ExposureResponseSpecificationLower.ExpressionString
                            : null,
                        ErSpecificationUncUpper = s.ExposureResponseSpecificationUpper.ExpressionString.Length != 0
                            ? s.ExposureResponseSpecificationUpper.ExpressionString
                            : null
                    };
                    return record;
                })
            )];
        }
    }
}
