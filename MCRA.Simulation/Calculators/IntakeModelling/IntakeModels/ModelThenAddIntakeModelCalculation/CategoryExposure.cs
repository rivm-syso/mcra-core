using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace MCRA.Simulation.Calculators.IntakeModelling.ModelThenAddIntakeModelCalculation {

    /// <summary>
    /// Holds an exposure value associated with a category.
    /// </summary>
    public class CategoryExposure {

        public CategoryExposure() { }

        public CategoryExposure(string idCategory, double exposure) {
            IdCategory = idCategory;
            Exposure = exposure;
        }

        public string IdCategory { get; set; }
        public double Exposure { get; set; }
    }
}
