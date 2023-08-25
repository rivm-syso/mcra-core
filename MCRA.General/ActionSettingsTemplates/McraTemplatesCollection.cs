using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.ActionSettingsTemplates {
    public class McraTemplatesCollection {
        private static McraTemplatesCollection _instance;

        private Dictionary<ActionType, Dictionary<SettingsTemplateType, SettingsTemplate>> _actionTypeSettings = new();

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraTemplatesCollection Instance => _instance ??= new McraTemplatesCollection();

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private McraTemplatesCollection() => loadFromXml();

        /// <summary>
        /// Retrieve the settings tiers per action Module
        /// </summary>
        /// <param name="actionType">The module type</param>
        /// <returns></returns>
        public IDictionary<SettingsTemplateType, SettingsTemplate> GetModuleTemplate(ActionType actionType) {
            return _actionTypeSettings.TryGetValue(actionType, out var settings) ? settings : null;
        }

        /// <summary>
        /// Returns action settings templates as defined in the internal xml.
        /// </summary>
        private void loadFromXml() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.ActionSettingsTemplates.TemplatesCollection.Generated.xml")) {
                var xs = new XmlSerializer(typeof(TemplatesCollection));
                var templateCollection = (TemplatesCollection)xs.Deserialize(stream);

                foreach(var moduleTemplates in templateCollection) {
                    foreach(var template in moduleTemplates) {
                        //add template to actiontype settings dictionary
                        if (!_actionTypeSettings.TryGetValue(template.ActionType, out var actionSettings)) {
                            _actionTypeSettings.Add(template.ActionType, actionSettings = new());
                        }
                        actionSettings.Add(template.Tier, template);
                    }
                }
            }
        }
    }
}
