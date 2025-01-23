namespace MCRA.General.ModuleDefinitions.Interfaces {
    public interface IActiveSubstanceAllocationSettings {
        SubstanceTranslationAllocationMethod ReplacementMethod { get; }
        bool UseSubstanceAuthorisations { get; }
        bool RetainAllAllocatedSubstancesAfterAllocation { get; }
        bool TryFixDuplicateAllocationInconsistencies { get; }
    }
}
