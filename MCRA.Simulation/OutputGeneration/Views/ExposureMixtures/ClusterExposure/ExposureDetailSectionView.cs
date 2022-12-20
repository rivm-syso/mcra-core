using MCRA.Simulation.OutputGeneration.Helpers;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class ExposureDetailSectionView : SectionView<ExposureDetailSection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            var exposureMatrixDataSection = DataSectionHelper.CreateCsvDataSection(
                "ExposureMatrix",
                Model,
                ViewBag,
                (r) => Model.WriteExposureMatrixCsv(r)
            );
            sb.AppendDescriptionParagraph($"Number of substances: {Model.ExposureMatrix.RowDimension}.");
            sb.AppendDescriptionParagraph($"Number of individuals: {Model.ExposureMatrix.ColumnDimension}.");
            sb.AppendDescriptionParagraph($"Download link for the raw exposure-matrix (substances x individuals).");
            sb.Append(TableHelpers.CsvExportLink(exposureMatrixDataSection));
        }
    }
}
