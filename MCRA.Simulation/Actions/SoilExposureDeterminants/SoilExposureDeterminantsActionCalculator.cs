using MCRA.Utils.ProgressReporting;
using MCRA.Data.Management;
using MCRA.General;
using MCRA.General.Annotations;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Actions.SoilExposureDeterminants {

    [ActionType(ActionType.SoilExposureDeterminants)]
    public class SoilExposureDeterminantsActionCalculator : ActionCalculatorBase<ISoilExposureDeterminantsActionResult> {

        public SoilExposureDeterminantsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var soilIngestionUnit = ExternalExposureUnit.gPerDay;

            var adjustedSoilIngestions = subsetManager.AllSoilIngestions
                .Select(r => {
                    var alignmentFactor = r.ExposureUnit.GetSubstanceAmountUnit()
                        .GetMultiplicationFactor(soilIngestionUnit.GetSubstanceAmountUnit());

                    var ingestion = r.Value * alignmentFactor;
                    var variability = r.CvVariability * alignmentFactor;
                    return new SoilIngestion {
                        idSubgroup = r.idSubgroup,
                        AgeLower = r.AgeLower,
                        Sex = r.Sex,
                        Value = ingestion,
                        ExposureUnit = soilIngestionUnit,
                        DistributionType = r.DistributionType,
                        CvVariability = variability
                    };
                })
                .ToList();

            data.SoilIngestions = adjustedSoilIngestions;
            data.SoilIngestionUnit = soilIngestionUnit;
        }

        protected override void summarizeActionResult(ISoilExposureDeterminantsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing soil exposure determinants", 0);
            var summarizer = new SoilExposureDeterminantsSummarizer();
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
