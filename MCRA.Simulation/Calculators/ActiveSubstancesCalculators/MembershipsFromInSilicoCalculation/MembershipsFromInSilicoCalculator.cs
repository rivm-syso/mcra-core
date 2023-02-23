using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation {

    /// <summary>
    /// Calculator for creating assessment group membership models from molecular
    /// docking models and QSAR models.
    /// </summary>
    public class MembershipsFromInSilicoCalculator {

        private readonly bool _useDockingModels = true;
        private readonly bool _useQsarModels = true;

        /// <summary>
        /// Creates a new <see cref="IMembershipsFromInSilicoCalculatorSettings"/> instance.
        /// </summary>
        /// <param name="settings"></param>
        public MembershipsFromInSilicoCalculator(IMembershipsFromInSilicoCalculatorSettings settings) {
            _useDockingModels = settings.UseMolecularDockingModels;
            _useQsarModels = settings.UseQsarModels;
        }

        /// <summary>
        /// Creates a new <see cref="ActiveSubstancesCalculators"/> instance.
        /// </summary>
        /// <param name="useDockingModels"></param>
        /// <param name="useQsarModels"></param>
        public MembershipsFromInSilicoCalculator(bool useDockingModels, bool useQsarModels) {
            _useDockingModels = useDockingModels;
            _useQsarModels = useQsarModels;
        }

        /// <summary>
        /// Creates a collection of assessment group membership models from the
        /// available molecular docking models and QSAR models for the specified
        /// compounds, focal effect, and related effects.
        /// </summary>
        /// <param name="dockingModels"></param>
        /// <param name="qsarModels"></param>
        /// <param name="substances"></param>
        /// <param name="effects"></param>
        /// <returns></returns>
        public List<ActiveSubstanceModel> CalculateAvailableMembershipModels(
            ICollection<MolecularDockingModel> dockingModels,
            ICollection<QsarMembershipModel> qsarModels,
            ICollection<Compound> substances,
            ICollection<Effect> effects
        ) {
            return collectAvailableAssessmentMembershipModels(dockingModels, qsarModels, substances, effects);
        }

        private List<ActiveSubstanceModel> collectAvailableAssessmentMembershipModels(
            ICollection<MolecularDockingModel> dockingModels,
            ICollection<QsarMembershipModel> qsarModels,
            ICollection<Compound> substances,
            ICollection<Effect> effects
        ) {
            var result = new List<ActiveSubstanceModel>();

            if (_useDockingModels) {
                dockingModels = dockingModels.Where(r => effects.Contains(r.Effect)).ToList();
                foreach (var dockingModel in dockingModels) {
                    var model = new ActiveSubstanceModel() {
                        Code = $"AG-{dockingModel.Code}",
                        Name = $"AG-{dockingModel.Name}",
                        Effect = dockingModel.Effect,
                        Description = $"Assessment group memberships computed from docking model {dockingModel.Name} ({dockingModel.Code})",
                        MembershipProbabilities = substances
                            .Where(r => dockingModel.BindingEnergies.TryGetValue(r, out var energy) && !double.IsNaN(energy))
                            .ToDictionary(c => c, c => dockingModel.BindingEnergies[c] < (double)dockingModel.Threshold ? 1D : 0D),
                        Sensitivity = double.NaN,
                        Specificity = double.NaN,
                    };
                    result.Add(model);
                }
            }

            if (_useQsarModels) {
                qsarModels = qsarModels.Where(r => effects.Contains(r.Effect)).ToList();
                foreach (var qsarModel in qsarModels) {
                    var model = new ActiveSubstanceModel() {
                        Code = $"AG-{qsarModel.Code}",
                        Name = $"AG-{qsarModel.Name}",
                        Effect = qsarModel.Effect,
                        Description = $"Assessment group memberships computed from QSAR membership model {qsarModel.Name} ({qsarModel.Code})",
                        MembershipProbabilities = substances
                            .Where(r => qsarModel.MembershipScores.ContainsKey(r) && !double.IsNaN(qsarModel.MembershipScores[r]))
                            .ToDictionary(c => c, c => qsarModel.MembershipScores[c]),
                        Sensitivity = qsarModel.Sensitivity ?? double.NaN,
                        Specificity = qsarModel.Specificity ?? double.NaN,

                    };
                    result.Add(model);
                }
            }

            return result;
        }
    }
}
