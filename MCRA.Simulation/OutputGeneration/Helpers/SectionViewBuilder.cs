namespace MCRA.Simulation.OutputGeneration.Helpers {
    public static class SectionViewBuilder {

        /// <summary>
        /// Tries to find and instantiate a section view for the provided summary section
        /// based on the provided view name.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="viewName"></param>
        /// <returns></returns>
        public static ISectionView CreateView(SummarySection section, string viewName = null, SummaryToc toc = null) {
            viewName = !string.IsNullOrEmpty(viewName) ? viewName : section.GetType().Name;
            var viewType = Type.GetType($"MCRA.Simulation.OutputGeneration.Views.{viewName}View");
            if (viewType == null) {
                viewType = Type.GetType($"MCRA.Simulation.OutputGeneration.CombinedViews.{viewName}View");
            }
            if (viewType != null) {
                var instance = (ISectionView)Activator.CreateInstance(viewType);
                instance.Initialize(section, toc);
                return instance;
            }
            return null;
        }
    }
}
