namespace MCRA.General.Action.Settings {
    public class SubsetSettingsDto {

        public virtual bool PopulationSubsetSelection { get; set; }
        public virtual bool SampleSubsetSelection { get; set; }
        public virtual bool ConsumerDaysOnly { get; set; }
        public virtual bool IsDefaultSamplingWeight { get; set; }
        public virtual bool IsPerPerson { get; set; }
        public virtual int MinimumLevelsRangeSubsetSelection { get; set; } = 3;

        public virtual bool RestrictPopulationByFoodAsEatenSubset { get; set; }
        public virtual bool RestrictConsumptionsByFoodAsEatenSubset { get; set; }
        public virtual bool ModelledFoodsConsumerDaysOnly { get; set; }
        public virtual bool RestrictPopulationByModelledFoodSubset { get; set; }
        public virtual bool RestrictToModelledFoodSubset { get; set; }

        public bool ExpressSingleValueConsumptionsPerPerson { get; } = true;
        public virtual bool UseBodyWeightStandardisedConsumptionDistribution { get; set; }

        public virtual IndividualSubsetType MatchHbmIndividualSubsetWithPopulation { get; set; } = IndividualSubsetType.MatchToPopulationDefinition;
        public virtual IndividualSubsetType MatchIndividualSubsetWithPopulation { get; set; } = IndividualSubsetType.MatchToPopulationDefinition;
        public virtual bool UseHbmSamplingWeights { get; set; }
        public virtual bool ExcludeIndividualsWithLessThanNDays { get; set; }
        public virtual int MinimumNumberOfDays { get; set; } = 2;
    }
}
