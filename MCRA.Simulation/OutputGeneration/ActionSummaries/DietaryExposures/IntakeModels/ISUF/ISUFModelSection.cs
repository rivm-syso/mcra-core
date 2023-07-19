//using MCRA.Utils.ProgressReporting;
//using MCRA.General.Action.Settings;
//using MCRA.Simulation.Calculators.IntakeModelling;
//using System.ComponentModel;

//namespace MCRA.Simulation.OutputGeneration {
//    public sealed class ISUFModelSection : ChronicIntakeModelSection {

//        #region Sections
//        [DisplayName("Estimates, diagnostics")]
//        public ISUFModelResultsSection ISUFModelResultsSection { get; set; }
//        #endregion

//        public override void Summarize(SectionHeader header, ProjectDto project, ActionData dataSource, UsualIntakeResults usualIntakeResults,  ProgressState p) {
//            //p.Update("Summarizing ISUF model", 0);
//            //ISUFModelResultsSection = new ISUFModelResultsSection();
//            //header.AddSubSectionHeaderFor(ISUFModelResultsSection, "Estimates, diagnostics", 1);
//            //ISUFModelResultsSection.Summarize(usualIntakeResults.IntakeModel as ISUFModel);

//            //p.Update("Creating usual exposure percentiles ISUF", 25);
//            //DietaryChronicMarginalSection = new ChronicMarginalSection();
//            //var subHeader = header.AddSubSectionHeaderFor(DietaryChronicMarginalSection, "Exposure distribution (ISUF)", 40);
//            //DietaryChronicMarginalSection.SummarizeReferenceResults(subHeader, usualIntakeResults.IntakeModel as ISUFModel, project.OutputDetailSettings, reference);
//            //p.Update(100);
//        }

//        public override void SummarizeUncertainty(ProjectDto project, UsualIntakeResults usualIntakeResults, ProgressState p) {
//            //p.Update("Summarizing uncertainty ISUF model", 100);
//            //DietaryChronicMarginalSection.SummarizeUncertaintyResults(usualIntakeResults.IntakeModel as ISUFModel, project.UncertaintyAnalysisSettings);
//        }
//    }
//}
