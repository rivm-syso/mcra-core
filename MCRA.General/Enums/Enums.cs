using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum DataTableColumnNames {
        [Description("Indentifier label for mixtures in datatable column Substance")]
        [Display(Name = "Mixture", ShortName = "mixture")]
        Mixture,

        [Description("Datatable column name substance")]
        [Display(Name = "Substance", ShortName = "substance")]
        Substance,

        [Description("Datatable column name standard deviation")]
        [Display(Name = "SD", ShortName = "sd")]
        SD,

        [Description("Datatable column name coefficient of variation")]
        [Display(Name = "CV", ShortName = "cv")]
        CV,

        [Description("Datatable column name number of observations")]
        [Display(Name = "N", ShortName = "n")]
        N,

        [Description("Datatable column name blocks")]
        [Display(Name = "Block", ShortName = "block")]
        Block,

        [Description("Datatable column name experimental unit")]
        [Display(Name = "ExperimentalUnit", ShortName = "experimentalunit")]
        ExperimentalUnit,

        [Description("Datatable column name uncertainty upper")]
        [Display(Name = "UncertaintyUpper", ShortName = "uncertaintyupper")]
        UncertaintyUpper
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum DistributionType {
        [Description("Use Logistic Normal.")]
        [Display(Name = "Logistic Normal")]
        LogisticNormal = 1,
        [Description("Log Normal.")]
        [Display(Name = "Log Normal")]
        LogNormal = 2,
        [Description("Uniform.")]
        [Display(Name = "Uniform")]
        Uniform = 3,
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum FoodConversionStepType {
        [Display(Name = "No match", ShortName = "-1")]
        [Description("")]
        NoMatch = 0,
        [Display(Name = "Circular loop", ShortName = "-1")]
        [Description("")]
        CircularLoop = 1,
        [Display(Name = "1. Identical code", ShortName = "1")]
        [Description("")]
        Concentration = 2,
        [Display(Name = "2a. Processing link exact", ShortName = "2a")]
        [Description("")]
        ProcessingExact = 3,
        [Display(Name = "2a+. Processing link without substance", ShortName = "2a+")]
        [Description("")]
        ProcessingIgnoreCompound = 4,
        [Display(Name = "2c. Processing facet", ShortName = "2c")]
        [Description("")]
        ProcessingFacet = 15,
        [Display(Name = "3a. Composition exact", ShortName = "3a")]
        [Description("")]
        CompositionExact = 7,
        [Display(Name = "3d. Composition strip processing", ShortName = "3d")]
        [Description("")]
        CompositionStripProcessing = 8,
        [Display(Name = "4. Subtype", ShortName = "4")]
        [Description("")]
        Subtype = 9,
        [Display(Name = "5a. Supertype", ShortName = "5")]
        [Description("")]
        Supertype = 10,
        [Display(Name = "5b. Hierarchy supertype", ShortName = "5")]
        [Description("")]
        HierarchySupertype = 16,
        [Display(Name = "6a. Default processing", ShortName = "6a")]
        [Description("")]
        DefaultProcessing = 11,
        [Display(Name = "6b. Composite facet processing", ShortName = "b6")]
        [Description("")]
        CompositeFacetProcessing = 18,
        [Display(Name = "6c. Remove facets", ShortName = "6c")]
        [Description("")]
        RemoveFacets = 17,
        [Display(Name = "7. Worst-case value", ShortName = "7")]
        [Description("")]
        WorstCaseValue = 12,
        [Display(Name = "3b. TDS Composition exact", ShortName = "3b")]
        [Description("")]
        TDSCompositionExact = 13,
        [Display(Name = "3c. Read across", ShortName = "3c")]
        [Description("")]
        ReadAcross = 14,
        [Display(Name = "3a*. Processing translation", ShortName = "3a*")]
        [Description("")]
        ProcessingTranslation = 15,
    }

    /// <summary>
    /// Description purposes only
    /// Use of EFSA Pesticide Residue Intake Model (EFSA PRIMo revision 3), ADOPTED: 19 December 2017
    /// Acute. doi: 10.2903/j.efsa.2018.5147
    /// </summary>
    public enum IESTIType {
        [Description("Undefined IESTI case.")]
        [Display(Name = "-", ShortName = "-")]
        Undefined = -1,
        [Description("The unit weight (UW) is <= 25 g.")]
        [Display(Name = "Case 1", ShortName = "Case 1")]
        Case1,
        [Description("The unit weight of the edible portion (UE) is lower than that of the large portion (LP)")]
        [Display(Name = "Case 2a", ShortName = "Case 2a")]
        Case2a,
        [Description("The unit weight of the edible portion (UE) is higher than that of the large portion (LP)")]
        [Display(Name = "Case 2b", ShortName = "Case 2b")]
        Case2b,
        [Description("Bulked or blending")]
        [Display(Name = "Case 3", ShortName = "Case 3")]
        Case3,
        [Description("New IESTI case 1 and 3")]
        [Display(Name = "Case new 1 and 3", ShortName = "Case new 1 and 3")]
        CaseNew1_3,
        [Description("New IESTI case 2a and 2b")]
        [Display(Name = "Case new 2a and 2b", ShortName = "Case new 2a and 2b")]
        CaseNew2a_2b,
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum IncidentalIntakeType {
        [Description("Incidental intake")]
        Incidental,
        [Description("Intake probability < 0.003")]
        Zero,
        [Description("Intake probability > 0.997")]
        One,
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum PotencyOrigin {
        [Display(Name = "Unknown", ShortName = "Unknown")]
        Unknown = 0,
        [Display(Name = "Imputed", ShortName = "Imputed")]
        Imputed = 1,
        [Display(Name = "Relative potency factor", ShortName = "RPF")]
        RelativePotencyFactor = 2,
        [Display(Name = "Hazard dose", ShortName = "HD")]
        LimitDose = 3,
        [Display(Name = "Benchmark dose", ShortName = "BMD")]
        Bmd = 4,
        [Display(Name = "No observed adverse effect level", ShortName = "NOAEL")]
        Noael = 5,
        [Display(Name = "Lowest observed adverse effect level", ShortName = "LOAEL")]
        Loael = 6,
        [Display(Name = "Aggregated hazard characterisation", ShortName = "Aggregated")]
        Aggregated = 7,
        [Display(Name = "RPF * PoD", ShortName = "RPF * PoD")]
        Ivive = 8,
        [Display(Name = "Munro NOEL", ShortName = "Munro NOEL")]
        Munro = 9,
        [Display(Name = "No observed effect level", ShortName = "NOEL")]
        Noel = 10,
        [Display(Name = "Median lethal dose", ShortName = "LD50")]
        Ld50 = 11,
        [Display(Name = "Acute reference dose", ShortName = "ARfD")]
        ARfD = 12,
        [Display(Name = "Average daily intake", ShortName = "ADI")]
        ADI = 13,
        [Display(Name = "Tolerable weekly intake", ShortName = "TWI")]
        Twi = 14,
        [Display(Name = "Benchmark dose lower confidence limit of 1%", ShortName = "BMDL01")]
        Bmdl01 = 15,
        [Display(Name = "Benchmark dose lower confidence limit of 10%", ShortName = "BMDL10")]
        Bmdl10 = 16,
        [Display(Name = "Tolerable daily intake", ShortName = "TDI")]
        Tdi = 17,
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum PropertyType {
        [Display(Name = "Covariable")]
        [Description("Quantitative levels.")]
        Covariable,
        [Display(Name = "Cofactor")]
        [Description("Qualitative levels.")]
        Cofactor,
        [Display(Name = "DateTime")]
        [Description("DateTimes.")]
        DateTime
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum RiskReferenceCompoundType {
        [Description("Cumulative risk calculation using RPFs.")]
        [Display(Name = "Cumulative (RPF weighted)", ShortName = "Cumulative (RPF weighted)")]
        RpfWeighted,
        [Description("Cumulative risk calculation based on sum of risk rations.")]
        [Display(Name = "Cumulative (sum of ratios)", ShortName = "Cumulative (sum of ratios)")]
        SumOfRiskRatios,
    }

    /// <summary>
    /// Description purposes only
    /// </summary>
    public enum TranslationType {
        [Display(Name = "Composition")]
        [Description("")]
        Composition,
        [Display(Name = "ReadAcross")]
        [Description("")]
        ReadAcross,
        [Display(Name = "Unknown")]
        [Description("")]
        Unknown,
    }
}
