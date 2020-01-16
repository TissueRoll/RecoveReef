using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class Item
{
    [XmlAttribute("name")]
    public string name;

    [XmlAttribute("Damage")]
    public string damage;
    
    [XmlAttribute("Durability")]
    public string durability;
}
