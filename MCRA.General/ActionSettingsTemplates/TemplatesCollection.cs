using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.ActionSettingsTemplates {

    [Serializable]
    [XmlRoot("TemplatesCollection")]
    public sealed class TemplatesCollection : Collection<SettingsTemplates> {

    }
}
