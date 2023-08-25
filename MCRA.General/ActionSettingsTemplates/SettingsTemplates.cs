using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.ActionSettingsTemplates {
    [Serializable]
    [XmlType("SettingsTemplates")]
    public sealed class SettingsTemplates: Collection<SettingsTemplate> {

    }
}
