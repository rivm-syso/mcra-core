using MCRA.Simulation.OutputGeneration.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCRA.Simulation.OutputGeneration.Views {
    public class KineticModelsSummarySectionView : SectionView<KineticModelsSummarySection> {
        public override void RenderSectionHtml(StringBuilder sb) {
            //Render HTML
            sb.AppendTable(
               Model,
               Model.Records,
               "KineticModelsSummarySectionTable",
               ViewBag,
               caption: "Kinetic models summary.",
               saveCsv: true,
               header: true
           );
            sb.AppendTable(
               Model,
               Model.AbsorptionFactorRecords,
               "AbsorptionFactorsSummarySectionTable",
               ViewBag,
               caption: "Absorption factors summary.",
               saveCsv: true,
               header: true
            );

            if (Model.ParameterSubstanceIndependentRecords?.Any() ?? false) {
                var hiddenProperties = new List<string>();
                hiddenProperties.Add("Code");
                hiddenProperties.Add("Name");
                sb.AppendTable(
                    Model,
                    Model.ParameterSubstanceIndependentRecords,
                    "DescriptionKineticParametersSubstanceIndependentTable",
                    ViewBag,
                    header: true,
                    caption: "Substance independent kinetic parameters.",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: hiddenProperties
                );
            }

            if (Model.ParameterSubstanceDependentRecords?.Any() ?? false) {
                var substances = Model.ParameterSubstanceDependentRecords.Select(c => c.Code).Distinct().ToList();
                var hiddenProperties = new List<string>();
                hiddenProperties.Add("Code");
                hiddenProperties.Add("Name");
                hiddenProperties.Add("Value");

                //if substancesCount == 1 render complete table otherwise only parameternames, description and units
                sb.AppendTable(
                    Model,
                    Model.ParameterSubstanceDependentRecords.Where(c => c.Code == substances.First()).ToList(),
                    "DescriptionKineticParametersSubstanceDependent1Table",
                    ViewBag,
                    header: true,
                    caption: "Description substance dependent kinetic parameters.",
                    saveCsv: true,
                    sortable: true,
                    hiddenProperties: substances.Count > 1 ? hiddenProperties : null
                );
            }

            if (Model.ParameterSubstanceDependentRecords?.Any() ?? false) {
                var substances = Model.ParameterSubstanceDependentRecords.Select(c => c.Code).Distinct().ToList();
                //render complete table 
                var hiddenProperties = new List<string>();
                hiddenProperties.Add("Description");
                hiddenProperties.Add("Unit");
                if (substances.Count > 1) {
                    sb.AppendTable(
                        Model,
                        Model.ParameterSubstanceDependentRecords,
                        "DescriptionKineticParametersSubstanceDependent2Table",
                        ViewBag,
                        header: true,
                        caption: "Description substance dependent kinetic parameters.",
                        saveCsv: true,
                        sortable: true,
                        displayLimit: 20,
                        hiddenProperties: hiddenProperties
                    );
                }
            }
        }
    }
}
