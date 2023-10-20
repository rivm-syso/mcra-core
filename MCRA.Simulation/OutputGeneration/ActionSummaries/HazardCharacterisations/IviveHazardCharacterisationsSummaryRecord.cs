using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IviveHazardCharacterisationsSummaryRecord : HazardCharacterisationsSummaryRecordBase {

        [Display(AutoGenerateField = false)]
        public bool IsReferenceSubstance { get; set; }

        [Description("External relative potency factor")]
        [DisplayName("RPF external")]
        [Display(Name = "RPF external", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ExternalRpf { get; set; }

        [Description("Potency info")]
        [DisplayName("Potency origin")]
        [Display(Name = "Potency origin", Order = 100)]
        public override string PotencyOrigin {
            get {
                return IsReferenceSubstance ? "REF" : "IVIVE";
            }
        }

        [Description("Nominal interspecies conversion factor to extrapolate the test system hazard characterisation on to human.")]
        [DisplayName("Interspecies conversion factor")]
        [Display(Name = "Interspecies conversion factor", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NominalInterSpeciesConversionFactor {get; set; }

        [Description("Nominal intra-species conversion factor to extrapolate the hazard characterisation to the sensitive individual (1/EFintra).")]
        [DisplayName("Intra-species conversion factor")]
        [Display(Name = "Intra-species conversion factor", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NominalIntraSpeciesConversionFactor { get; set; }

        [Description("Additional conversion factor to translate the test system hazard characterisation to a human hazard characterisation (1/AF).")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(Name = "Additional conversion factor", Order = 100)]
        public double AdditionalConversionFactor { get; set; }

        [Description("Nominal conversion factor modelling the effect of kinetic-processes when translating the system hazard characterisation to the target hazard characterisation.")]
        [DisplayName("Kinetic conversion factor")]
        [Display(Name = "Kinetic conversion factor", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NominalKineticConversionFactor { get; set; }

        [Description("Internal hazard characterisation (TargetConcentrationUnit)")]
        [DisplayName("Internal hazard characterisation (TargetConcentrationUnit)")]
        [Display(Name = "Internal hazard characterisation (TargetConcentrationUnit)", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double SystemHazardCharacterisation { get; set; }

        [Description("Internal relative potency factor based on concentrations in g/kg.")]
        [DisplayName("Internal RPF (mass based)")]
        [Display(Name = "Internal RPF (mass based)", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double InternalRpf { get; set; }

        [Description("Molecular mass")]
        [DisplayName("Molecular mass")]
        [Display(Name = "Molecular mass", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MolecularMass { get; set; }

        [Description("Internal RPFs based on concentrations in mol/L.")]
        [DisplayName("Internal RPF (mol based)")]
        [Display(Name = "Internal RPF (mol based)", Order = 100)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MolBasedRpf { get; set; }

    }
}
