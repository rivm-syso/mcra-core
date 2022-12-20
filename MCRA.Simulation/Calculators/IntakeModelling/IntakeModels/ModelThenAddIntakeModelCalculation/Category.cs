namespace MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation {

    public class Category {

        public Category() { }

        public Category(string idCategory, string name) {
            Id = idCategory;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
