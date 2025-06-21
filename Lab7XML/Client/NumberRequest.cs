using System.Xml.Serialization;

namespace Client
{
    [XmlRoot("Request")]
    public class NumberRequest
    {
        [XmlArray("Numbers")]
        [XmlArrayItem("Number")]
        public List<int> Numbers { get; set; } = new();
    }
}
