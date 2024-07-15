using System.Reflection;
using System.Xml.Serialization;

namespace MCRA.General.ActionSettingsTemplates {
    public class McraTemplatesCollection {
        private static McraTemplatesCollection _instance;

        private readonly Dictionary<SettingsTemplateType, SettingsTemplate> _templatesDictionary = [];

        private readonly Dictionary<ActionType, HashSet<SettingsTemplate>> _actionTemplateTypes = [];
        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static McraTemplatesCollection Instance => _instance ??= new ();

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private McraTemplatesCollection() => loadFromXml();

        /// <summary>
        /// Retrieve the settings tier for a template type
        /// </summary>
        /// <param name="templateType">The template type (tier)</param>
        /// <returns></returns>
        public SettingsTemplate GetTemplate(SettingsTemplateType templateType) =>
            _templatesDictionary.TryGetValue(templateType, out var template) ? template : null;

        /// <summary>
        /// Get available tiers for a certain action type
        /// </summary>
        public ICollection<SettingsTemplate> GetTiers(ActionType actionType) =>
            _actionTemplateTypes.TryGetValue(actionType, out var tiers) ? tiers : null;

        /// <summary>
        /// Returns action settings templates as defined in the internal xml.
        /// </summary>
        private void loadFromXml() {
            var assembly = Assembly.Load("MCRA.General");
            using (var stream = assembly.GetManifestResourceStream("MCRA.General.ActionSettingsTemplates.TemplatesCollection.Generated.xml")) {
                var xs = new XmlSerializer(typeof(SettingsTemplates));
                var templates = (SettingsTemplates)xs.Deserialize(stream);
                foreach (var template in templates) {
                    //add template to templates dictionary
                    _templatesDictionary.Add(template.Tier, template);

                    foreach (var actionType in template.ConfigurationsDictionary.Keys) {
                        if(!_actionTemplateTypes.TryGetValue(actionType, out var templateTypes)) {
                            _actionTemplateTypes.Add(actionType, templateTypes = []);
                        }
                        templateTypes.Add(template);
                    }
                }
            }
        }
    }
}

