using MCRA.Utils.ExtensionMethods;
using System.Reflection;

namespace MCRA.Simulation.OutputGeneration.Helpers.HtmlBuilders {

    /// <summary>
    /// Class that builds HTML tables based on a generic list of records.
    /// </summary>
    public abstract class HtmlElementBuilder {

        /// <summary>
        /// The id of the element.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Additional attributes.
        /// </summary>
        public IDictionary<string, object> HtmlAttributes { get; set; }

        /// <summary>
        /// View parameters
        /// </summary>
        public ViewParameters ViewBag { get; set; }

        /// <summary>
        /// Replace any unit keys in property names with unit values
        /// </summary>
        /// <returns></returns>
        protected Func<PropertyInfo, string> createHeaderFormatter() {
            var templateValues = ViewBag?.UnitsDictionary;
            string headerNameFormatter(PropertyInfo property) {
                var displayName = property.GetShortName();
                if (templateValues != null) {
                    foreach (var unit in templateValues) {
                        displayName = displayName.Replace("(" + unit.Key, "(" + unit.Value);
                        displayName = displayName.Replace("{" + unit.Key + "}", unit.Value);
                    }
                }
                return displayName;
            }
            return headerNameFormatter;
        }

        /// <summary>
        /// Replace any unit keys in property descriptions with unit values
        /// </summary>
        /// <returns></returns>
        protected Func<PropertyInfo, string> createDescriptionFormatter() {
            var templateValues = ViewBag?.UnitsDictionary;
            string descriptionFormatter(PropertyInfo property) {
                var description = property.GetDescription();
                if (!string.IsNullOrEmpty(description)) {
                    if (templateValues != null) {
                        foreach (var unit in templateValues) {
                            description = description.Replace("(" + unit.Key, "(" + unit.Value);
                            description = description.Replace("{" + unit.Key + "}", unit.Value);
                        }
                    }
                }
                return description;
            }
            return descriptionFormatter;
        }
    }
}
