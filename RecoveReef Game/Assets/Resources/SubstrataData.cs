using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class SubstrataData
{
    [XmlAttribute("name")]
    public string name;
    [XmlElement("GroundViability")]
    public int groundViability;

    public string dataToString() {
        string output = "---SubstrataData---";
        output += "\nname: " + name;
        output += "\ngroundViability: " + groundViability;
        return output;
    }
}
