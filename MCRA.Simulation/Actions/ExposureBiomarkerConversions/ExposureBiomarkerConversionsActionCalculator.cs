﻿using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Simulation.Actions.ExposureBiomarkerConversions {

    [ActionType(ActionType.ExposureBiomarkerConversions)]
    public class ExposureBiomarkerConversionsActionCalculator : ActionCalculatorBase<IExposureBiomarkerConversionsActionResult> {

        public ExposureBiomarkerConversionsActionCalculator(ProjectDto project) : base(project) {
            _actionDataLinkRequirements[ScopingType.ExposureBiomarkerConversions][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        protected override void verify() {
        }

        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var allCompunds = data.AllCompounds.ToHashSet();
            var relevantSubstancesTo = subsetManager.AllExposureBiomarkerConversions
                .Where(r => allCompunds.Contains(r.SubstanceFrom))
                .Select(r => r.SubstanceTo)
                .ToHashSet();
            data.ExposureBiomarkerConversions = subsetManager.AllExposureBiomarkerConversions
                .Where(r => relevantSubstancesTo.Contains(r.SubstanceTo))
                .ToList();
        }

        protected override void summarizeActionResult(IExposureBiomarkerConversionsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new ExposureBiomarkerConversionsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}