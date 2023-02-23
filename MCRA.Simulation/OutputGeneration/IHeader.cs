namespace MCRA.Simulation.OutputGeneration {
    public interface IHeader {
        Guid SectionId { get; set; }
        string  SectionLabel { get; set; }
        string Name { get; set; }
        string TitlePath { get; set; }
    }
}
