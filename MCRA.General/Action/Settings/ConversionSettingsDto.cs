using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {

    public class ConversionSettingsDto {

        public virtual bool SubstanceIndependent { get; set; } = false;

        /// <summary>
        /// Deprecated setting: specifies whether the "old step 2a: use processing factors"
        /// should be used.
        /// </summary>
        public virtual bool UseProcessing { get; set; } = false;

        public virtual bool UseComposition { get; set; } = true;

        public virtual bool UseReadAcrossFoodTranslations { get; set; }

        public virtual bool UseMarketShares { get; set; }

        public virtual bool UseSubTypes { get; set; }

        public virtual bool UseSuperTypes { get; set; }

        public virtual bool UseDefaultProcessingFactor { get; set; } = true;

        public virtual bool UseWorstCaseValues { get; set; }

        public virtual bool FoodIncludeNonDetects { get; set; } = true;

        public virtual bool CompoundIncludeNonDetects { get; set; } = true;

        public virtual bool CompoundIncludeNoMeasurements { get; set; }

        public virtual bool DeriveModelledFoodsFromSampleBasedConcentrations { get; set; } = true;

        public virtual bool DeriveModelledFoodsFromSingleValueConcentrations { get; set; }

        [XmlIgnore]
        public ModelledFoodsCalculationSource ModelledFoodsCalculationSource {
            get {
                if (DeriveModelledFoodsFromSampleBasedConcentrations) {
                    return ModelledFoodsCalculationSource.DeriveModelledFoodsFromSampleBasedConcentrations;
                } else if (DeriveModelledFoodsFromSingleValueConcentrations) {
                    return ModelledFoodsCalculationSource.DeriveModelledFoodsFromSingleValueConcentrations;
                } else if (UseWorstCaseValues) {
                    return ModelledFoodsCalculationSource.UseWorstCaseValues;
                }
                return ModelledFoodsCalculationSource.UseWorstCaseValues;
            }
            set {
                switch (value) {
                    case ModelledFoodsCalculationSource.DeriveModelledFoodsFromSampleBasedConcentrations:
                        DeriveModelledFoodsFromSampleBasedConcentrations = true;
                        DeriveModelledFoodsFromSingleValueConcentrations = false;
                        UseWorstCaseValues = false;
                        break;
                    case ModelledFoodsCalculationSource.DeriveModelledFoodsFromSingleValueConcentrations:
                        DeriveModelledFoodsFromSampleBasedConcentrations = false;
                        DeriveModelledFoodsFromSingleValueConcentrations = true;
                        UseWorstCaseValues = false;
                        break;
                    case ModelledFoodsCalculationSource.UseWorstCaseValues:
                        DeriveModelledFoodsFromSampleBasedConcentrations = false;
                        DeriveModelledFoodsFromSingleValueConcentrations = false;
                        UseWorstCaseValues = true;
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
