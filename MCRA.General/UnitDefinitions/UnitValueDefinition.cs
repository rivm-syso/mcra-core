using System.Xml.Serialization;
using System.Text;

namespace MCRA.General {
    public class UnitValueDefinition {

        private static object _lock = new();
        private HashSet<string> _acceptedFormats;
        private HashSet<string> _acceptedUris;

        /// <summary>
        /// Gets/sets the unit id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets/sets the unit name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets the short name of this unit.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Gets/sets the known aliases for this unit value.
        /// </summary>
        [XmlArrayItem("Alias")]
        public List<string> Aliases { get; set; }

        /// <summary>
        /// Gets/sets the known aliases for this unit value.
        /// </summary>
        [XmlArray("URIs")]
        [XmlArrayItem("URI")]
        public List<string> Uris { get; set; }

        /// <summary>
        /// Gets/sets the description of the name.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Returns the accepted formats (aliases + id) for this unit value definition.
        /// </summary>
        /// <returns></returns>
        public HashSet<string> AcceptedFormats {
            get {
                if (_acceptedFormats == null && Id != null) {
                    lock (_lock) {
                        if (_acceptedFormats == null && Id != null) {
                            var acceptedFormats = Aliases
                                .Select(r => r.Normalize(NormalizationForm.FormKD))
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
                            acceptedFormats.Add(Id.Normalize(NormalizationForm.FormKD));
                            _acceptedFormats = acceptedFormats;
                        }
                    }
                }
                return _acceptedFormats;
            }
        }

        /// <summary>
        /// Returns the accepted formats (aliases + id) for this unit value definition.
        /// </summary>
        /// <returns></returns>
        public HashSet<string> AcceptedUris{
            get {
                if (_acceptedUris == null && Id != null) {
                    lock (_lock) {
                        if (_acceptedUris == null && Id != null) {
                            var acceptedFormats = Uris
                                .Select(r => r.Normalize(NormalizationForm.FormKD))
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
                            acceptedFormats.Add(Id.Normalize(NormalizationForm.FormKD));
                            _acceptedUris = acceptedFormats;
                        }
                    }
                }
                return _acceptedUris;
            }
        }

        /// <summary>
        /// Returns whether the string is accepted as a valid representation of this unit value.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool AcceptsFormat(string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            var normalizedStr = str.Normalize(NormalizationForm.FormKD);
            return AcceptedFormats
                .Any(r => r.Normalize(NormalizationForm.FormKD)
                .Equals(normalizedStr, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Returns whether the URI string is accepted as a valid representation of this unit value.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool AcceptsUri(string str) {
            if (string.IsNullOrEmpty(str)) {
                return false;
            }
            var normalizedStr = str.Normalize(NormalizationForm.FormKD);
            return AcceptedUris
                .Any(r => r.Normalize(NormalizationForm.FormKD)
                .Equals(normalizedStr, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
