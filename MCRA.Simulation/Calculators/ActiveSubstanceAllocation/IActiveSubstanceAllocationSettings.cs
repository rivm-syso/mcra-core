using MCRA.General;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {
    public interface IActiveSubstanceAllocationSettings {
        SubstanceTranslationAllocationMethod ReplacementMethod { get; }
        bool UseSubstanceAuthorisations { get; }
        bool RetainAllAllocatedSubstancesAfterAllocation { get; }
        bool TryFixDuplicateAllocationInconsistencies { get; }
    }
}
