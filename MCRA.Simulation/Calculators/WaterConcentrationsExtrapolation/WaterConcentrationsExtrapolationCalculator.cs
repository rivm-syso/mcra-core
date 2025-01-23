using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.Simulation.Calculators.FoodExtrapolationsCalculation {
    public sealed class WaterConcentrationsExtrapolationCalculator {

        private readonly IWaterConcentrationsExtrapolationCalculatorSettings _settings;

        public WaterConcentrationsExtrapolationCalculator(IWaterConcentrationsExtrapolationCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Water imputation: adds additional samples for water.
        /// </summary>
        /// <param name="activeSubstances"></param>
        /// <param name="food"></param>
        /// <param name="authorisations"></param>
        /// <param name="numberOfSubstances"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="concentrationUnit"></param>
        /// <returns></returns>
        public SampleCompoundCollection Create(
            ICollection<Compound> activeSubstances,
            Food food,
            IDictionary<(Food, Compound), SubstanceAuthorisation> authorisations,
            IDictionary<Compound, SubstanceApproval> substanceApprovals,
            int numberOfSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ConcentrationUnit concentrationUnit
        ) {
            authorisations = _settings.RestrictWaterImputationToAuthorisedUses ? authorisations : null;
            substanceApprovals = _settings.RestrictWaterImputationToApprovedSubstances ? substanceApprovals : null;

            var imputationSubstances = (_settings.RestrictWaterImputationToMostPotentSubstances && activeSubstances.Count > numberOfSubstances)
                ? getMostPotentSubstances(activeSubstances, food, authorisations, substanceApprovals, relativePotencyFactors, numberOfSubstances)
                : activeSubstances;

            var concentrationUnitCorrectionFactor = ConcentrationUnit.ugPerKg.GetConcentrationUnitMultiplier(concentrationUnit);
            var residueValue = concentrationUnitCorrectionFactor * _settings.WaterConcentrationValue;
            var sampleCompoundRecord = new SampleCompoundRecord() {
                AuthorisedUse = true,
                SampleCompounds = activeSubstances
                    .ToDictionary(r => r, r => {
                        return new SampleCompound() {
                            ActiveSubstance = r,
                            MeasuredSubstance = r,
                            IsExtrapolated = true,
                            Residue = imputationSubstances.Contains(r) ? residueValue : 0D
                        };
                    })
            };
            var sampleCompoundRecords = new List<SampleCompoundRecord>() { sampleCompoundRecord };

            var result = new SampleCompoundCollection(food, sampleCompoundRecords);
            return result;
        }

        private static HashSet<Compound> getMostPotentSubstances(
            ICollection<Compound> activeSubstances,
            Food food,
            IDictionary<(Food, Compound), SubstanceAuthorisation> authorisations,
            IDictionary<Compound, SubstanceApproval> substanceApprovals,
            IDictionary<Compound, double> relativePotencyFactors,
            int nrOfMostToxicSubstances
        ) {
            if (relativePotencyFactors == null) {
                throw new Exception($"Missing RPFs for imputation of water concentrations for {nrOfMostToxicSubstances} most potent substances.");
            }
            return activeSubstances
                .Where(r => IsAuthorised(food, r, authorisations) && IsApproved(r, substanceApprovals))
                .OrderByDescending(r => relativePotencyFactors.ContainsKey(r) ? relativePotencyFactors[r] : 0D)
                .Take(nrOfMostToxicSubstances)
                .ToHashSet();
        }

        private static bool IsApproved(Compound r, IDictionary<Compound, SubstanceApproval> substanceApprovals) {
            return (substanceApprovals == null) || (substanceApprovals.ContainsKey(r) && substanceApprovals[r].IsApproved);
        }

        private static bool IsAuthorised(Food food, Compound r, IDictionary<(Food, Compound), SubstanceAuthorisation> authorisations) {
            return (authorisations == null)
                    || authorisations.ContainsKey((food, r))
                    || (food.BaseFood != null && authorisations.ContainsKey((food.BaseFood, r)));
        }
    }
}
