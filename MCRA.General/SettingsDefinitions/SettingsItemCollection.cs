using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MCRA.General.SettingsDefinitions {

    [Serializable()]
    [XmlRoot("SettingsItems")]
    public class SettingsItemCollection : Collection<SettingsItem> {
    }
}
