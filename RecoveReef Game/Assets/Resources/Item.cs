using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class Item
{
    [XmlAttribute("name")]
    public string name;

    [XmlElement("Damage")]
    public int damage;
    
    [XmlElement("Durability")]
    public int durability;

    public string rekt() {
        string output = "";
        output += name + "\n" + damage + "\n" + durability + "\n";
        return output;
    }
}
