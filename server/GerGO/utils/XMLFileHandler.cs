using GerGO.Models;
using System.Xml.Serialization;

namespace GerGO.Utils
{
    class XMLFileHandler : FileHandler
    {
        private Logger _logger = LoggerFactory.GetLogger();
        public void WriteDataBaseData(string path, List<DataBase> dataBases)
        {
            try
            {
                string resultXml = SerializeToXml(dataBases);
                _logger.Info(resultXml);

                File.WriteAllText(path, resultXml);
            }
            catch (IOException)
            {
                _logger.Error("Failed to write to file!");
                throw new FileHandlerException("Failed to write to file!");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to serialize! " + ex.Message);
                throw new FileHandlerException("Failed to serialize!");
            }
        }

        private string SerializeToXml<T>(List<T> obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
            using (StringWriter sw = new StringWriter())
            {
                serializer.Serialize(sw, obj);
                return sw.ToString();
            }
        }
    }
}
