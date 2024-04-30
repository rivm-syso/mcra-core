using System.Xml.Serialization;

namespace MCRA.General.ModuleDefinitions.Settings {
    public partial class ModelledFoodsModuleConfig {
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
