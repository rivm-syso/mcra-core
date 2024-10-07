using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Actions.DustExposureDeterminants {

    [ActionType(ActionType.DustExposureDeterminants)]
    public class DustExposureDeterminantsActionCalculator : ActionCalculatorBase<IDustExposureDeterminantsActionResult> {

        public DustExposureDeterminantsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.DustAvailabilityFractions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var dustIngestionUnit = ExternalExposureUnit.gPerDay;

            var adjustedDustIngestions = subsetManager.AllDustIngestions
                .Select(r => {
                    var alignmentFactor = r.ExposureUnit.GetSubstanceAmountUnit()
                        .GetMultiplicationFactor(dustIngestionUnit.GetSubstanceAmountUnit());
                             
                    var ingestion = r.Value * alignmentFactor;
                    var variability = r.CvVariability * alignmentFactor;
                    return new DustIngestion {
                        idSubgroup = r.idSubgroup,
                        AgeLower = r.AgeLower,
                        Sex = r.Sex,
                        Value = ingestion,
                        ExposureUnit = dustIngestionUnit,
                        DistributionType = r.DistributionType,
                        CvVariability = variability
                    };
                })
                .ToList();

            data.DustIngestions = adjustedDustIngestions;
            data.DustIngestionUnit = dustIngestionUnit;
            data.DustBodyExposureFractions = [.. subsetManager.AllDustBodyExposureFractions];
            data.DustAdherenceAmounts = [.. subsetManager.AllDustAdherenceAmounts];
            data.DustAvailabilityFractions = [.. subsetManager.AllDustAvailabilityFractions]; 
        }

        protected override void summarizeActionResult(IDustExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing dust exposure determinants", 0);
            var summarizer = new DustExposureDeterminantsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
