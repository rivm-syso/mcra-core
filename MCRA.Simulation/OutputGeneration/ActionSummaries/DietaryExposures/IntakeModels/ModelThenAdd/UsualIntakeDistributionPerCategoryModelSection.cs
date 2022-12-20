namespace MCRA.Simulation.OutputGeneration {
    public sealed class UsualIntakeDistributionPerCategoryModelSection : DistributionSectionBase {
        public UsualIntakeDistributionPerCategoryModelSection() {
            IsTotalDistribution = true;
        }

        public string FoodNames { get; set; }
    }
}
