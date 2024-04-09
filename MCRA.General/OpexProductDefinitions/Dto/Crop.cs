
using System.Xml.Serialization;
using MCRA.Utils.DataSourceReading.Attributes;

namespace MCRA.General.OpexProductDefinitions.Dto {
    /// <summary>
    /// OPEX definition for crops with content that should exactly match the OPEX crop.csv file.
    /// </summary>
    public record Crop(
    ) {
        // NOTE: all properties are in lowerCamelCase, to get the correct casing when used in the OPEX R script
        public string use { get; init; }
        public string crop { get; init; }
        public double maxRate { get; init; }
        public string unit { get; init; }

        [XmlIgnore]
        public int? maxNo { get { return !string.IsNullOrEmpty(maxNoAsText) ? int.Parse(maxNoAsText) : default(int?);} init { } }
        [XmlElement("maxNo")]
        [IgnoreField]
        public string maxNoAsText { get; init; }

        [XmlIgnore]
        public int? interval { get { return !string.IsNullOrEmpty(intervalAsText) ? int.Parse(intervalAsText) : default(int?); } init { } }
        [XmlElement("interval")]
        [IgnoreField]
        public string intervalAsText { get; init; }

        [XmlIgnore]
        public int? minVolume { get { return !string.IsNullOrEmpty(minVolumeAsText) ? int.Parse(minVolumeAsText) : default(int?); } init { } }
        [XmlElement("minVolume")]
        [IgnoreField]
        public string minVolumeAsText { get; init; }

        [XmlIgnore]
        public int? maxVolume { get { return !string.IsNullOrEmpty(maxVolumeAsText) ? int.Parse(maxVolumeAsText) : default(int?); } init { } }
        [XmlElement("maxVolume")]
        [IgnoreField]
        public string maxVolumeAsText { get; init; }

        public string indoor { get; init; }
        public string activity { get; init; }


        [XmlIgnore]
        public bool? activityTSF { get { return !string.IsNullOrEmpty(activityTSFAsText) ? bool.Parse(activityTSFAsText) : default(bool?); } init { } }
        [XmlElement("activityTSF")]
        [IgnoreField]
        public string activityTSFAsText { get; init; }


        [XmlIgnore]
        public int? bufferStrip { get { return !string.IsNullOrEmpty(bufferStripAsText) ? int.Parse(bufferStripAsText) : default(int?); } init { } }
        [XmlElement("bufferStrip")]
        [IgnoreField]
        public string bufferStripAsText { get; init; }

        [XmlIgnore]
        public int? driftReduction { get { return !string.IsNullOrEmpty(driftReductionAsText) ? int.Parse(driftReductionAsText) : default(int?); } init { } }
        [XmlElement("driftReduction")]
        [IgnoreField]
        public string driftReductionAsText { get; init; }

        public string applicationMethod { get; init; }
        public string density { get; init; }
        public string applicationEquipment { get; init; }
        public string id { get; init; }
    };      
}
