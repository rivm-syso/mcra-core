using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.ActiveSubstanceAllocation {

    public class ActiveSubstanceAllocationCalculatorFactory {

        private readonly IActiveSubstanceAllocationSettings _settings;

        public ActiveSubstanceAllocationCalculatorFactory(IActiveSubstanceAllocationSettings settings) {
            _settings = settings;
        }

        public IActiveSubstanceAllocationCalculator Create(
            ICollection<SubstanceConversion> substanceConversions,
            IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations,
            IDictionary<Compound, double> relativePotencyFactors
        ) {
            switch (_settings.ReplacementMethod) {
                case SubstanceTranslationAllocationMethod.UseMostToxic:
                    return new MostToxicActiveSubstanceAllocationCalculator(
                        substanceConversions,
                        substanceAuthorisations,
                        _settings.UseSubstanceAuthorisations,
                        _settings.RetainAllAllocatedSubstancesAfterAllocation,
                        relativePotencyFactors,
                        _settings.TryFixDuplicateAllocationInconsistencies);
                case SubstanceTranslationAllocationMethod.DrawRandom:
                    return new RandomActiveSubstanceAllocationCalculator(
                        substanceConversions,
                        substanceAuthorisations,
                        _settings.UseSubstanceAuthorisations,
                        _settings.RetainAllAllocatedSubstancesAfterAllocation,
                        _settings.TryFixDuplicateAllocationInconsistencies);
                case SubstanceTranslationAllocationMethod.NominalEstimates:
                    return new NominalActiveSubstanceAllocationCalculator(
                        substanceConversions,
                        substanceAuthorisations,
                        _settings.UseSubstanceAuthorisations,
                        _settings.RetainAllAllocatedSubstancesAfterAllocation,
                        _settings.TryFixDuplicateAllocationInconsistencies);
                case SubstanceTranslationAllocationMethod.AllocateToAll:
                    return new AllocateToAllActiveSubstancesCalculator(
                        substanceConversions,
                        substanceAuthorisations,
                        _settings.UseSubstanceAuthorisations,
                        _settings.RetainAllAllocatedSubstancesAfterAllocation,
                        _settings.TryFixDuplicateAllocationInconsistencies);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
