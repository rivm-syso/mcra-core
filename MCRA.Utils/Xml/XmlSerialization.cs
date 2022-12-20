using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace MCRA.Utils.Xml {
    /// <summary>
    /// Generic serialization and Deserialization methods (extension) methods.
    /// </summary>
    public static class XmlSerialization {

        /// <summary>
        /// Creates an object of type T from an XML string.
        /// </summary>
        /// <typeparam name="T">The target type. Should be a baseclass of the xml-serialized object</typeparam>
        /// <param name="xmlString">The source xmlString</param>
        /// <param name="sourceType">Optionally specify the source type of the xmlString to be deserialized (specific subclass of T)</param>
        /// <returns>The deserialized object</returns>
        public static T FromXml<T>(string xmlString, Type sourceType = null) {
            try {
                using (var stringReader = new StringReader(xmlString)) {
                    var ser = new XmlSerializer(sourceType ?? typeof(T));
                    using (var xmlReader = new XmlTextReader(stringReader)) {
                        T obj;
                        obj = (T)ser.Deserialize(xmlReader);
                        xmlReader.Close();
                        return obj;
                    }
                }
            } catch {
                using (var stringReader = new StringReader(xmlString)) {
                    var rootNodeName = XElement.Load(stringReader).Name.LocalName;
                    sourceType =
                        Assembly.GetExecutingAssembly().GetTypes()
                            .FirstOrDefault(t => (t.IsSubclassOf(typeof(T)) || t == typeof(T)) && t.Name == rootNodeName)
                        ??
                        Assembly.GetAssembly(typeof(T)).GetTypes()
                            .FirstOrDefault(t => (t.IsSubclassOf(typeof(T)) || t == typeof(T)) && t.Name == rootNodeName);

                    if (sourceType == null) {
                        throw new Exception("Xml source data type could not be determined");
                    }
                }
                using (var stringReader = new StringReader(xmlString)) {
                    if (sourceType.IsSubclassOf(typeof(T)) || sourceType == typeof(T)) {
                        var ser = new XmlSerializer(sourceType);
                        using (var xmlReader = new XmlTextReader(stringReader)) {
                            T obj;
                            obj = (T)ser.Deserialize(xmlReader);
                            xmlReader.Close();
                            return obj;
                        }
                    } else {
                        throw new InvalidCastException(sourceType.FullName + " cannot be cast to " + typeof(T).FullName);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an object of type T from an XML string.
        /// </summary>
        /// <typeparam name="T">The target type. Should be a baseclass of the xml-serialized object</typeparam>
        /// <param name="xmlFileName">The source xmlString</param>
        /// <param name="sourceType">Optionally specify the source type of the xmlString to be deserialized (specific subclass of T)</param>
        /// <returns>The deserialized object</returns>
        public static T FromXmlFile<T>(string xmlFileName, Type sourceType = null) {
            try {
                var ser = new XmlSerializer(sourceType ?? typeof(T));
                using (var xmlReader = new XmlTextReader(xmlFileName)) {
                    T obj;
                    obj = (T)ser.Deserialize(xmlReader);
                    xmlReader.Close();
                    return obj;
                }
            } catch {
                var rootNodeName = XElement.Load(xmlFileName).Name.LocalName;
                sourceType =
                    Assembly.GetExecutingAssembly().GetTypes()
                        .FirstOrDefault(t => (t.IsSubclassOf(typeof(T)) || t == typeof(T)) && t.Name == rootNodeName)
                    ??
                    Assembly.GetAssembly(typeof(T)).GetTypes()
                        .FirstOrDefault(t => (t.IsSubclassOf(typeof(T)) || t == typeof(T)) && t.Name == rootNodeName);

                if (sourceType == null) {
                    throw new Exception("Xml source data type could not be determined");
                }
                if (sourceType.IsSubclassOf(typeof(T)) || sourceType == typeof(T)) {
                    var ser = new XmlSerializer(sourceType);
                    using (var xmlReader = new XmlTextReader(xmlFileName)) {
                        T obj;
                        obj = (T)ser.Deserialize(xmlReader);
                        xmlReader.Close();
                        return obj;
                    }
                } else {
                    throw new InvalidCastException(sourceType.FullName + " cannot be cast to " + typeof(T).FullName);
                }
            }
        }

        /// <summary>
        /// Deserialized an xml string as object
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static object FromXml(string xmlString) {
            return FromXml<object>(xmlString);
        }

        /// <summary>
        /// Serializes the <i>Obj</i> to an XML string.
        /// </summary>
        public static string ToXml<T>(this T source, bool format = false) {
            using (var memStream = new MemoryStream()) {
                var settings = new XmlWriterSettings {
                    OmitXmlDeclaration = true,
                    NamespaceHandling = NamespaceHandling.OmitDuplicates,
                    Encoding = new UTF8Encoding(false)
                };

                if (format) {
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                }
                using (var xmlWriter = XmlWriter.Create(memStream, settings)) {
                    var ser = new XmlSerializer(source.GetType());
                    ser.Serialize(xmlWriter, source);
                }

                var xml = Encoding.UTF8.GetString(memStream.GetBuffer());
                return xml.Substring(0, xml.LastIndexOf('>') + 1);
            }
        }

        /// <summary>
        /// Serializes object of type T to a compressed byte array
        /// </summary>
        /// <typeparam name="T">The type of object to be deserialized</typeparam>
        /// <param name="source">The source object to serialize</param>
        /// <returns>byte array containing the compressed serialized object</returns>
        public static byte[] ToCompressedXml<T>(this T source) {
            using (var memStream = new MemoryStream()) {
                using (var zipWriter = new GZipStream(memStream, CompressionMode.Compress)) {
                    var settings = new XmlWriterSettings {
                        OmitXmlDeclaration = true,
                        NamespaceHandling = NamespaceHandling.OmitDuplicates,
                        Encoding = new UTF8Encoding(false)
                    };

                    using (var xmlWriter = XmlWriter.Create(zipWriter, settings)) {
                        var ser = new XmlSerializer(source.GetType());
                        ser.Serialize(xmlWriter, source);
                    }
                }
                return memStream.ToArray();
            }
        }

        /// <summary>
        /// Unzips the input stream and reconstitutes the serialized object from Xml
        /// </summary>
        /// <typeparam name="T">The type of object to be deserialized</typeparam>
        /// <param name="compressedXml">The compressed XML</param>
        /// <param name="sourceType">Optionally specify the source type of the xmlString to be deserialized (specific subclass of T)</param>
        /// <returns>The deserialized object of type T</returns>
        public static T FromCompressedXml<T>(byte[] compressedXml, Type sourceType = null) {
            var xml = UncompressBytes(compressedXml);
            return xml == null ? default : FromXml<T>(xml, sourceType);
        }

        /// <summary>
        /// Decompress GZip compressed string
        /// </summary>
        /// <param name="compressedData">byte array with compressed data</param>
        /// <returns>decompressed string</returns>
        public static string UncompressBytes(byte[] compressedData) {
            if(compressedData == null || compressedData.Length == 0) {
                return null;
            }

            using (var compressedStream = new MemoryStream(compressedData))
            using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
                using (var outputStream = new MemoryStream()) {
                    decompressedStream.CopyTo(outputStream);
                    //return the decompressed string
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Decompress GZip compressed string to a file
        /// </summary>
        /// <param name="compressedData">byte array with compressed data</param>
        /// <param name="newFileName">name of a file to create output to, if the file exists, a windows temp file
        /// is created instead</param>
        /// <returns>filename of the decompressed data</returns>
        public static string UncompressToFile(byte[] compressedData, string newFileName = null) {
            using (var compressedStream = new MemoryStream(compressedData))
            using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
                //use filestream
                if (File.Exists(newFileName)) {
                    newFileName = Path.GetTempFileName();
                }
                using (var wf = File.Create(newFileName)) {
                    decompressedStream.CopyTo(wf);
                }
            }
            //return name of file that was created
            return newFileName;
        }

        /// <summary>
        /// Compress to GZip compressed string
        /// </summary>
        /// <param name="value"></param>
        /// <returns>byte array of compressed string</returns>
        public static byte[] CompressString(string value) {
            var bytes = Encoding.UTF8.GetBytes(value);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream()) {
                using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
                    msi.CopyTo(gs);
                }
                return mso.ToArray();
            }
        }

        /// <summary>
        /// transforms an xml string with an xsl string
        /// </summary>
        /// <param name="xmlString">xml to transform</param>
        /// <param name="XSLStylesheet">stylesheet</param>
        /// <returns>xml transformed</returns>
        public static string TransformXmlStringWithXslString(string xmlString, string xslStylesheet) {
            if (string.IsNullOrWhiteSpace(xslStylesheet)) {
                return xmlString;
            }

            //process our xml
            var xmlTextReader = new XmlTextReader(new StringReader(xmlString));
            var xPathDocument = new XPathDocument(xmlTextReader);

            //process the xsl
            var xmlTextReaderXslt = new XmlTextReader(new StringReader(xslStylesheet));
            var xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load(xmlTextReaderXslt);

            //handle the output stream
            var stringBuilder = new StringBuilder();
            var textWriter = new StringWriter(stringBuilder);

            //do the transform
            xslCompiledTransform.Transform(xPathDocument, null, textWriter);
            return stringBuilder.ToString();
        }
    }
}
