namespace MCRA.Simulation.Calculators.HazardCharacterisationCalculation {
    public sealed class AggregateHazardCharacterisation : HazardCharacterisationModel {

        /// <summary>
        /// The collection of hazard characterisations that were the sources
        /// of this aggregated hazard characterisation.
        /// </summary>
        public ICollection<IHazardCharacterisationModel> Sources { get; set; }

    }
}
