using System.Text;

namespace MCRA.Simulation.OutputGeneration.Helpers {

    public interface ISectionView {
        void Initialize(SummarySection section, SummaryToc toc);
        void RenderSectionHtml(StringBuilder sb);
        ViewParameters ViewBag { get; set; }
    }

    public class ViewParameters {
        public Dictionary<string, string> UnitsDictionary { get; set; } = new();
        public string TitlePath { get; set; } = null;
        public string TempPath { get; set; } = null;

        //Get unit from unitsdictionary, otherwise return the key
        public string GetUnit(string key) {
            var value = $"?{key}?";
            UnitsDictionary?.TryGetValue(key, out value);
            return value;
        }
    }

    public abstract class SectionView<T> : ISectionView where T : SummarySection {

        protected T Model;
        protected SummaryToc Toc;

        public ViewParameters ViewBag { get; set; } = new();

        public void Initialize(SummarySection section, SummaryToc toc) {
            Model = (T)section;
            Toc = toc;
        }

        public abstract void RenderSectionHtml(StringBuilder sb);

        protected void renderSectionView(StringBuilder sb, string sectionTypeName, SummarySection section) {
            var view = SectionViewBuilder.CreateView(section, sectionTypeName, Toc);
            view.ViewBag = ViewBag;
            view.RenderSectionHtml(sb);
            // add any data sections of the partial section that was just rendered to this
            // Model's data section so it will be saved in the database
            if (Model != section) {
                foreach (var dataSection in section.DataSections) {
                    Model.DataSections.Add(dataSection);
                }
                foreach (var chartSection in section.ChartSections) {
                    Model.ChartSections.Add(chartSection);
                }
            }
        }
    }
}
