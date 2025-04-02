using GerGO.Models;
using GerGO.Utils;
using System.Xml.Serialization;

namespace GerGO.DataAcces.MetaData.FileHandler
{
    [XmlRoot("DataBaseXmlWrapper")]
    public class DataBaseXmlWrapper
    {
        [XmlArray("Databases")]
        [XmlArrayItem("DataBase")]
        public List<DataBase> DataBases { get; set; }
        public DataBaseXmlWrapper()
        {

        }
    }
    class XMLFileHandler : IFileHandler
    {
        private ILogger _logger = LoggerFactory.GetLogger();
        public void WriteDataBaseData(string path, List<DataBase> dataBases)
        {
            try
            {
                string resultXml = SerializeToXml(new DataBaseXmlWrapper() { DataBases = dataBases });

                File.WriteAllText(path, resultXml);
            }
            catch (IOException)
            {
                _logger.Error("Failed to write to file!");
                throw new FileHandlerException("Failed to write to file!");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to serialize: {ex.Message}");
                throw new FileHandlerException("Failed to serialize!");
            }
        }

        public List<DataBase> ReadDataBaseData(string path)
        {
            DataBaseXmlWrapper wrapper = DeserializeFromXml(path);

            return wrapper.DataBases;
        }

        private string SerializeToXml(DataBaseXmlWrapper wrapper)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DataBaseXmlWrapper));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, wrapper);
                return sw.ToString();
            }
        }

        private DataBaseXmlWrapper DeserializeFromXml(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataBaseXmlWrapper));
                using (StreamReader sr = new StreamReader(path))
                {
                    return (DataBaseXmlWrapper)serializer.Deserialize(sr);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to deserialize: {ex.Message}");
                throw new FileHandlerException("Failed to deserialize!");
            }
        }
    }
}
