using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Action.UncertaintyFactorial {

    /// <summary>
    /// Class for generating uncertainty factorial designs.
    /// </summary>
    public static class UncertaintyFactorialDesignGenerator {

        /// <summary>
        /// Creates an uncertainty factorial design based on the provided uncertainty
        /// analysis settings.
        /// TODO: this method is deprecated and should be removed when switching random
        /// seed generation.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static UncertaintyFactorialDesign Create(UncertaintyAnalysisSettingsDto settings) {
            var uncertaintySources = new List<UncertaintySource>();
            if (settings.ReSampleConcentrations) {
                uncertaintySources.Add(UncertaintySource.Concentrations);
                uncertaintySources.Add(UncertaintySource.ConcentrationModelling);
                uncertaintySources.Add(UncertaintySource.ConcentrationNonDetectImputation);
                uncertaintySources.Add(UncertaintySource.ConcentrationMissingValueImputation);
            }
            if (settings.ResampleIndividuals) {
                uncertaintySources.Add(UncertaintySource.Individuals);
            }
            if (settings.ReSampleProcessingFactors) {
                uncertaintySources.Add(UncertaintySource.Processing);
            }
            if (settings.ReSampleRPFs) {
                uncertaintySources.Add(UncertaintySource.RPFs);
                uncertaintySources.Add(UncertaintySource.HazardCharacterisationsSelection);
                uncertaintySources.Add(UncertaintySource.HazardCharacterisationsImputation);
                uncertaintySources.Add(UncertaintySource.DoseResponseModels);
                uncertaintySources.Add(UncertaintySource.PointsOfDeparture);
            }
            if (settings.ReSampleInterspecies) {
                uncertaintySources.Add(UncertaintySource.InterSpecies);
            }
            if (settings.ReSampleIntraSpecies) {
                uncertaintySources.Add(UncertaintySource.IntraSpecies);
            }
            if (settings.ReSamplePortions) {
                uncertaintySources.Add(UncertaintySource.Portions);
            }
            if (settings.ReSampleNonDietaryExposures) {
                uncertaintySources.Add(UncertaintySource.NonDietaryExposures);
            }
            if (settings.ReSampleAssessmentGroupMemberships) {
                uncertaintySources.Add(UncertaintySource.AssessmentGroupMemberships);
            }
            if (settings.ReSampleImputationExposureDistributions) {
                uncertaintySources.Add(UncertaintySource.ImputeExposureDistributions);
            }
            if (settings.ResampleKineticModelParameters) {
                uncertaintySources.Add(UncertaintySource.KineticModelParameters);
            }

            return Create(uncertaintySources);
        }

        /// <summary>
        /// Creates an uncertainty factorial design based on the specified collection of
        /// uncertainty sources.
        /// </summary>
        /// <param name="uncertaintySources"></param>
        /// <returns></returns>
        public static UncertaintyFactorialDesign Create(ICollection<UncertaintySource> uncertaintySources) {
            var result = new UncertaintyFactorialDesign();
            var binaryTruthTable = createBinaryTruthTable(uncertaintySources);
            result.TruthTable = createTruthTable(uncertaintySources, binaryTruthTable);
            result.UncertaintySources = uncertaintySources.Select(c => c.ToString()).ToList();
            result.UncertaintySources.Insert(0, "MC");
            result.DesignMatrix = binaryTruthTable.AsDesignMatrix();
            return result;
        }

        /// <summary>
        /// Generate a binary truth table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        private static List<bool[]> createBinaryTruthTable<T>(IEnumerable<T> source) {
            var sourceItems = source.ToList();
            var n = sourceItems.Count;
            var binaryTruthTable = new List<bool[]>();
            for (int i = 0; i < (int)Math.Pow(2, n); i++) {
                var row = new bool[n];
                for (int j = 0; j < n; j++) {
                    row[j] = (i / (int)Math.Pow(2, j)) % 2 != 0;
                }
                binaryTruthTable.Add(row);
            }
            return binaryTruthTable;
        }

        /// <summary>
        /// Generates a full binary thuth table for the target collection of items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="binaryTruthTable"></param>
        /// <returns></returns>
        /// <example>
        /// For a collection with three integers {40,20,32}, the resulting truth table looks as follows:
        /// {}
        /// {32}
        /// {20}
        /// {20,32}
        /// {40}
        /// {40,32}
        /// {40,20}
        /// {40,20,32}
        /// </example>
        private static List<List<T>> createTruthTable<T>(
            IEnumerable<T> source, 
            IEnumerable<bool[]> binaryTruthTable
        ) {
            var sourceItems = source.ToList();
            var n = sourceItems.Count;
            var truthTable = new List<List<T>>();
            foreach (var binaryValues in binaryTruthTable) {
                var truthTableRow = new List<T>();
                for (int i = 0; i < n; i++) {
                    if (binaryValues[i] == true) {
                        truthTableRow.Add(sourceItems[i]);
                    }
                }
                truthTable.Add(truthTableRow);
            }

            return truthTable;
        }
    }
}
