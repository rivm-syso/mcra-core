using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {

    public class EffectSettings {

        public virtual string CodeFocalEffect { get; set; }

        public virtual bool IncludeAopNetwork { get; set; }

        public virtual string CodeAopNetwork { get; set; }

        public virtual bool RestrictAopByFocalUpstreamEffect { get; set; }

        public virtual string CodeFocalUpstreamEffect { get; set; }

        public virtual string CodeReferenceCompound { get; set; }

        // Deprecated
        public virtual CompoundGroupSelectionMethodType CompoundGroupSelectionMethod { get; set; }

        public virtual HazardDoseImputationMethodType HazardDoseImputationMethod { get; set; }

        public virtual bool UseDoseResponseData { get; set; }

        public virtual bool UseDoseResponseModels { get; set; }
        public virtual bool UseBMDL{ get; set; } 

        public virtual bool ImputeMissingHazardDoses { get; set; }

        public virtual bool MultipleEffects { get; set; }
        public virtual bool RestrictToCriticalEffect { get; set; }

        public virtual TargetDoseSelectionMethod TargetDoseSelectionMethod { get; set; }

        public virtual PointOfDeparture PointOfDeparture { get; set; }

        public virtual AssessmentGroupMembershipCalculationMethod AssessmentGroupMembershipCalculationMethod { get; set; }

        public virtual bool UseMolecularDockingModels { get; set; }

        public virtual bool UseQsarModels { get; set; }

        public virtual double PriorMembershipProbability { get; set; } = .5;

        public virtual bool RestrictToAvailableHazardDoses { get; set; } = false;

        public virtual bool RestrictToAvailableHazardCharacterisations { get; set; }

        public virtual CombinationMethodMembershipInfoAndPodPresence CombinationMethodMembershipInfoAndPodPresence { get; set; }

        public virtual bool RestrictToCertainMembership { get; set; }

        public virtual bool IncludeSubstancesWithUnknowMemberships { get; set; }

        public virtual bool UseProbabilisticMemberships { get; set; }

        public virtual TargetLevelType TargetDoseLevelType { get; set; } = TargetLevelType.External;

        public virtual bool MergeDoseResponseExperimentsData { get; set; }

        public virtual TargetDosesCalculationMethod TargetDosesCalculationMethod { get; set; }

        public virtual bool UseInterSpeciesConversionFactors { get; set; }

        public virtual bool UseIntraSpeciesConversionFactors { get; set; }

        public virtual bool UseAdditionalAssessmentFactor { get; set; }
        
        public virtual double AdditionalAssessmentFactor { get; set; } = 100;

        public virtual BiologicalMatrix TargetMatrix { get; set; } = BiologicalMatrix.Undefined;

        public virtual bool HazardCharacterisationsConvertToSingleTargetMatrix { get; set; }

        #region CalculatedSettings
        [XmlIgnore]
        public bool IncludeAopNetworks => !MultipleEffects && IncludeAopNetwork;

        #endregion
    }
}
