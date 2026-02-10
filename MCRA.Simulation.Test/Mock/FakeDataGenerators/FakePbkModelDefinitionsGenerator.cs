using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.KineticModelDefinitions;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    public static class FakePbkModelDefinitionsGenerator {

        public static PbkModelDefinition CreateFromSbml(
            string filename
        ) {
            var name = Path.GetFileNameWithoutExtension(filename);
            var model = new PbkModelDefinition {
                IdModelDefinition = $"{name}",
                Name = $"{name}",
                FileName = $"{name}.sbml",
                Description = $"{name}",
                KineticModelDefinition = SbmlPbkModelSpecificationBuilder.CreateFromSbmlFile($"Resources/PbkModels/{name}.sbml")
            };
            return model;
        }
    }
}
