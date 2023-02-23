namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Class that builds HTML tables based on a generic list of records.
    /// </summary>
    public abstract class HtmlGenericElementBuilder<TRecord> : HtmlElementBuilder {

        /// <summary>
        /// A tabu list for properties that shouldn't be rendered.
        /// </summary>
        public IList<string> HiddenProperties { get; set; }
    }
}
