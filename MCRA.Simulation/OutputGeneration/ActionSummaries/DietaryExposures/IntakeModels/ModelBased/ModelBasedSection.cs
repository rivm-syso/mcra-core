using MCRA.Utils;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IntakeModelling;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class ModelBasedSection : ChronicSectionBase {

        /// <summary>
        /// Summarizes the reference results for OIM.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="intakes"></param>
        /// <param name="weights"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="isAcuteCovariateModelling"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="exposureLevels"></param>
        public void Summarize(
            SectionHeader header,
            List<double> intakes,
            List<double> weights,
            Compound referenceSubstance,
            bool isAcuteCovariateModelling,
            double[] selectedPercentiles,
            double[] exposureLevels
        ) {
            var intakeDistributionSection = new ModelBasedDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(intakeDistributionSection, "Graph total", 1);
            intakeDistributionSection.IsAcuteCovariateModelling = isAcuteCovariateModelling;
            intakeDistributionSection.Summarize(intakes, weights);
            subHeader.SaveSummarySection(intakeDistributionSection);

            var percentileSection = new IntakePercentileSection();
            subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 2);
            percentileSection.Summarize(intakes, weights, referenceSubstance, selectedPercentiles);
            subHeader.SaveSummarySection(percentileSection);

            var percentageSection = new IntakePercentageSection();
            subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 3);
            percentageSection.Summarize(intakes, weights, referenceSubstance, exposureLevels);
            subHeader.SaveSummarySection(percentageSection);
        }

        /// <summary>
        /// Summarizes the marginal usual intakes for BNN,LNN,LNN0
        /// Note that weights are not needed because the marginal distribution is already based on weights, therefor supply standard weights
        /// </summary>
        /// <param name="header"></param>
        /// <param name="modelBasedIntakes"></param>
        /// <param name="referenceSubstance"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="exposureLevels"></param>
        public void Summarize(
            SectionHeader header,
            List<double> modelBasedIntakes,
            Compound referenceSubstance,
            double[] selectedPercentiles,
            double[] exposureLevels
        ) {
            Summarize(header, modelBasedIntakes, null, referenceSubstance, false, selectedPercentiles, exposureLevels);
        }

        /// <summary>
        /// Summarizes uncertainty results for BNN,LNN,LNN0
        /// Note that weights are not needed because the marginal distribution is already based on weights, therefor supply standard weights
        /// </summary>
        /// <param name="modelBasedIntakes"></param>
        /// <param name="uncertaintyAnalysisSettings"></param>
        public void SummarizeUncertainty(
                SectionHeader header,
                List<double> modelBasedIntakes,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            var subHeader = header.GetSubSectionHeader<ModelBasedDistributionSection>();
            if (subHeader != null) {
                var intakeDistributionSection = subHeader.GetSummarySection() as ModelBasedDistributionSection;
                intakeDistributionSection.SummarizeUncertainty(modelBasedIntakes, null, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(modelBasedIntakes, null, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(modelBasedIntakes, null, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }

        /// <summary>
        /// Summarizes the reference results for ISUF.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="isufModel"></param>
        /// <param name="selectedPercentiles"></param>
        /// <param name="exposureLevels"></param>
        public void Summarize(
            SectionHeader header,
            ISUFModel isufModel,
            Compound referenceSubstance,
            double [] selectedPercentiles,
            double [] exposureLevels
        ) {
            var intakeDistributionSection = new ModelBasedDistributionSection();
            var subHeader = header.AddSubSectionHeaderFor(intakeDistributionSection, "Graph total", 1);
            if (subHeader != null) {
                intakeDistributionSection.Summarize(isufModel.UsualIntakeResult.UsualIntakes.Select(c => c.UsualIntake).ToList(), null);
                intakeDistributionSection.Summarize(isufModel, GriddingFunctions.GetPlotPercentages());
                subHeader.SaveSummarySection(intakeDistributionSection);
            }
            var percentileSection = new IntakePercentileSection();
            if (subHeader != null) {
                subHeader = header.AddSubSectionHeaderFor(percentileSection, "Percentiles", 2);
                percentileSection.Summarize(isufModel, referenceSubstance, selectedPercentiles);
                subHeader.SaveSummarySection(percentileSection);
            }
            var percentageSection = new IntakePercentageSection();
            if (subHeader != null) {
                subHeader = header.AddSubSectionHeaderFor(percentageSection, "Percentages", 3);
                percentageSection.Summarize(isufModel, referenceSubstance, exposureLevels);
                subHeader.SaveSummarySection(percentageSection);
            }
        }

        /// <summary>
        /// Summarizes uncertainty results for ISUF
        /// </summary>
        /// <param name="header"></param>
        /// <param name="isufModel"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        public void SummarizeUncertainty(
                SectionHeader header,
                ISUFModel isufModel,
                double uncertaintyLowerBound,
                double uncertaintyUpperBound
            ) {
            var subHeader = header.GetSubSectionHeader<ModelBasedDistributionSection>();
            if (subHeader != null) {
                var intakeDistributionSection = subHeader.GetSummarySection() as ModelBasedDistributionSection;
                intakeDistributionSection.SummarizeUncertainty(isufModel, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentileSection>();
            if (subHeader != null) {
                var percentileSection = subHeader.GetSummarySection() as IntakePercentileSection;
                percentileSection.SummarizeUncertainty(isufModel, uncertaintyLowerBound, uncertaintyUpperBound);
            }
            subHeader = header.GetSubSectionHeader<IntakePercentageSection>();
            if (subHeader != null) {
                var percentageSection = subHeader.GetSummarySection() as IntakePercentageSection;
                percentageSection.SummarizeUncertainty(isufModel, uncertaintyLowerBound, uncertaintyUpperBound);
            }
        }
    }
}
