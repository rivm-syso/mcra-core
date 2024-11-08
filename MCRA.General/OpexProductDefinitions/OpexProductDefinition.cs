using MCRA.General.OpexProductDefinitions.Dto;

namespace MCRA.General.OpexProductDefinitions {
    public record OpexProductDefinition(
        string Id,
        string Name,
        string Description,
        Product Product,
        List<Absorption> Absorptions,
        List<Crop> Crops,
        List<Substance> Substances
    ) {
        public OpexProductDefinition() : this("", "", "", new Product(), [], [], []) { }
    };
}
