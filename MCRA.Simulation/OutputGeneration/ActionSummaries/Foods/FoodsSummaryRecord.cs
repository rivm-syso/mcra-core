using MCRA.Utils.DataTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodsSummaryRecord {

        [DisplayName("Food name")]
        public string Name { get; set; }

        [DisplayName("Food code")]
        public string Code { get; set; }

        [DisplayName("Parent code")]
        public string CodeParent { get; set; }

        [DisplayName("Base food name")]
        [Description("Base (raw/unprocessed) food name.")]
        public string BaseFoodName { get; set; }

        [DisplayName("Base food code")]
        [Description("Base (raw/unprocessed) food code.")]
        public string BaseFoodCode { get; set; }

        [DisplayName("Treatment code(s)")]
        [Description("Code(s) of the treatments/facets of the consumed food.")]
        public string TreatmentCodes { get; set; }

        [DisplayName("Treatment name(s)")]
        [Description("Name(s) of the treatments/facets of the consumed food.")]
        public string TreatmentNames { get; set; }

        [Display(AutoGenerateField = false)]
        public QualifiedValue DefaultUnitWeightRacQualifiedValue { get; set; }

        [Description("Unit weight raw agricultural commodity (g).")]
        [DisplayName("Unit weight RAC (g)")]
        public string DefaultUnitWeightRac {
            get {
                return DefaultUnitWeightRacQualifiedValue?.ToString("F0");
            }
        }

        [Display(AutoGenerateField = false)]
        public QualifiedValue DefaultUnitWeightEpQualifiedValue { get; set; }

        [Description("Unit weight edible portion (g).")]
        [DisplayName("Unit weight EP (g)")]
        public string DefaultUnitWeightEp {
            get {
                return DefaultUnitWeightEpQualifiedValue?.ToString("F0");
            }
        }

        [Display(AutoGenerateField = false)]
        public List<string> LocationUnitWeightsRacLocations { get; set; }

        [Display(AutoGenerateField = false)]
        public List<QualifiedValue> LocationUnitWeightsRacValues { get; set; }

        [Description("Location specific unit weights raw agricultural commodity (g).")]
        [DisplayName("Location unit weights RAC (g)")]
        public string LocationUnitWeightsRac {
            get {
                if ((LocationUnitWeightsRacLocations?.Any() ?? false) && (LocationUnitWeightsRacValues?.Any() ?? false)) {
                    return string.Join(", ", LocationUnitWeightsRacLocations.Zip(LocationUnitWeightsRacValues, (l, v) => $"{v.ToString("G3")}g ({l})"));
                }
                return null;
            }
        }

        [Display(AutoGenerateField = false)]
        public List<string> LocationUnitWeightsEpLocations { get; set; }

        [Display(AutoGenerateField = false)]
        public List<QualifiedValue> LocationUnitWeightsEpValues { get; set; }

        [Description("Location specific unit weights edible portion (g).")]
        [DisplayName("Location unit weights EP (g)")]
        public string LocationUnitWeightsEp {
            get {
                if ((LocationUnitWeightsEpLocations?.Any() ?? false) && (LocationUnitWeightsEpValues?.Any() ?? false)) {
                    return string.Join(", ", LocationUnitWeightsEpLocations.Zip(LocationUnitWeightsEpValues, (l,v) => $"{v.ToString("G3")}g ({l})"));
                }
                return null;
            }
        }
    }
}
