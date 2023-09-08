using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationsPruning {

    /// <summary>
    /// Pruner class to remove HBM concentration records for substances with two 
    /// expression types for the same matrix.
    /// recorded 
    /// </summary>
    public class HbmIndividualDayConcentrationsCollectionPruner {

        public HashSet<string> StandardiseBloodExcludedSubstancesSubset { get; set; }

        public HashSet<string> StandardiseUrineExcludedSubstancesSubset { get; set; }

        public HbmIndividualDayConcentrationsCollectionPruner(
            HashSet<string> standardiseBloodExcludedSubstancesSubset,
            HashSet<string> standardiseUrineExcludedSubstancesSubset
        ) {
            StandardiseBloodExcludedSubstancesSubset = standardiseBloodExcludedSubstancesSubset;
            StandardiseUrineExcludedSubstancesSubset = standardiseUrineExcludedSubstancesSubset;
        }

        public ICollection<HbmIndividualDayCollection> Prune(
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections
        ) {
            // Clone all collections
            var clonedCollections = hbmIndividualDayCollections
                .Select(r => r.Clone())
                .ToList();

            // Prune individual day collections:
            // it is not allowed to have one substance with two expression
            // types for the same matrix.
            var collectionsByMatrix = clonedCollections
                .GroupBy(r => r.TargetUnit.BiologicalMatrix);

            // Group collections by matrix
            foreach (var group in collectionsByMatrix) {
                var targetMatrix = group.Key;

                // If we find a matrix with multiple (i.e., two) expression types then
                // we need to check the substances and possibly prune
                if (group.Count() > 1) {

                    // We assume max. two expression types per matrix
                    if (group.Count() > 2) {
                        throw new NotImplementedException($"Pruning of HBM individual day collections not supported for more than two expression types for the same matrix.");
                    }

                    // Get all pairs of expression type and substance
                    var substancesPerExpressionType = group
                        .SelectMany(r =>
                            r.HbmIndividualDayConcentrations
                                .SelectMany(r => r.ConcentrationsBySubstance.Keys)
                                .Distinct(),
                            (r, s) => (ExpressionType: r.TargetUnit.ExpressionType, Substance: s)
                        )
                        .ToList();

                    // Get pairs for which overlaps are found
                    var overlappingSubstances = substancesPerExpressionType
                        .GroupBy(r => r.Substance)
                        .Where(r => r.Count() > 1);

                    // Get the standardised/corrected and non-standardised HBM individual day collection
                    var standardised = group.Single(r => r.TargetUnit.ExpressionType != ExpressionType.None);
                    var nonStandardised = group.Single(r => r.TargetUnit.ExpressionType == ExpressionType.None);

                    // Predicate to check whether a substance should be included in the standardized
                    // or non-standardized collection.
                    Predicate<Compound> useStandardizedPredicate;
                    if (targetMatrix.IsBlood()) {
                        useStandardizedPredicate = (substance) => substance.IsLipidSoluble != true
                            && !StandardiseBloodExcludedSubstancesSubset.Contains(substance.Code);
                    } else if (targetMatrix.IsUrine()) {
                        useStandardizedPredicate = (substance) => !StandardiseUrineExcludedSubstancesSubset
                            .Contains(substance.Code);
                    } else {
                        throw new NotImplementedException("Not implemented");
                    }

                    // Loop over substances and decide whether to keep each substance in the
                    // standardized or non-standardized collection
                    foreach (var substance in overlappingSubstances) {
                        if (useStandardizedPredicate(substance.Key)) {
                            // Remove from non-standardised
                            foreach (var hbmIndividualDayConcentration in nonStandardised.HbmIndividualDayConcentrations) {
                                hbmIndividualDayConcentration.ConcentrationsBySubstance.Remove(substance.Key);
                            }
                        } else {
                            // Remove from standardised
                            foreach (var hbmIndividualDayConcentration in standardised.HbmIndividualDayConcentrations) {
                                hbmIndividualDayConcentration.ConcentrationsBySubstance.Remove(substance.Key);
                            }
                        }
                    }
                }
            }

            return clonedCollections;
        }
    }
}
