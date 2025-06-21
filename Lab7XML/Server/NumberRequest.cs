using System.Xml.Serialization;

namespace Server
{
    [XmlRoot("Request")]
    public class NumberRequest
    {
        [XmlArray("Numbers")]
        [XmlArrayItem("Number")]
        public List<int> Numbers { get; set; } = new();
    }
}
