using System.Xml.Serialization;

namespace Client
{
    [XmlRoot("Response")]
    public class NumberResponse
    {
        [XmlElement("Sum")]
        public int Sum { get; set; }

        [XmlElement("Error")]
        public string? Error { get; set; }
    }
}
