namespace MCRA.General.Action.Settings {

    public class UncertaintyAnalysisSettings {

        public virtual bool DoUncertaintyAnalysis { get; set; }

        public virtual int NumberOfIterationsPerResampledSet { get; set; } = 10000;

        public virtual int NumberOfResampleCycles { get; set; }

        public virtual bool ReSampleConcentrations { get; set; } = true;

        public virtual bool IsParametric { get; set; }

        /// <summary>
        /// Obsolete
        /// </summary>
        public virtual UncertaintyType UncertaintyType { get; set; }

        public virtual bool ReSampleNonDietaryExposures { get; set; }

        public virtual bool ResampleIndividuals { get; set; } = true;

        public virtual bool DoUncertaintyFactorial { get; set; }

        public virtual bool ReSamplePortions { get; set; }

        public virtual bool ReSampleProcessingFactors { get; set; }

        public virtual bool ReSampleAssessmentGroupMemberships { get; set; }

        public virtual bool ReSampleImputationExposureDistributions { get; set; }

        public virtual bool ReSampleRPFs { get; set; }

        public virtual bool ReSampleInterspecies { get; set; }

        public virtual bool ReSampleIntraSpecies { get; set; }

        public virtual bool ReSampleParameterValues { get; set; }

        public virtual bool ResampleKineticModelParameters { get; set; }

        public virtual bool RecomputeOccurrencePatterns { get; set; }

        public virtual double UncertaintyLowerBound { get; set; } = 2.5D;

        public virtual double UncertaintyUpperBound { get; set; } = 97.5D;
        public virtual bool ResampleHBMIndividuals { get; set; } = true;

    }
}
