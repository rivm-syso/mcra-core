
using MCRA.Utils.DataSourceReading.Attributes;
using System.Xml.Serialization;

namespace MCRA.General.OpexProductDefinitions.Dto {
    /// <summary>
    /// OPEX definition for substances with content that should exactly match the OPEX substance.csv file.
    /// </summary>
    public record Substance(
    ) {
        // NOTE: all properties are in lowerCamelCase, to get the correct casing when used in the OPEX R script
        public string id { get; set; }
        public string substance { get; set; }
        public double aoel { get; set; }
        public double aaoel { get; set; }
        public double pressure { get; set; }

        [XmlIgnore]
        public int? oral { get { return !string.IsNullOrEmpty(oralAsText) ? int.Parse(oralAsText) : default(int?); } init { } }
        [XmlElement("oral")]
        [IgnoreField]
        public string oralAsText { get; set; }

        [XmlIgnore]
        public int? inhalation { get { return !string.IsNullOrEmpty(inhalationAsText) ? int.Parse(inhalationAsText) : default(int?); } init { } }
        [XmlElement("inhalation")]
        [IgnoreField]
        public string inhalationAsText { get; set; }

        public double molecularWeight { get; set; }
        public double vapourConcentrationExp { get; set; }
        public double concentration { get; set; }
        public double mlDermal { get; set; }
    };
}
