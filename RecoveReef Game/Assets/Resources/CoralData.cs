using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class CoralData
{
    [XmlAttribute("name")]
    public string name;

    [XmlElement("GrowTime")]
    public int growTime;

    [XmlElement("Survivability")]
    public int survivability;
    [XmlElement("Propagatability")]
    public int propagatability;

    [XmlElement("CFProduction")]
    public float cfProduction;

    [XmlElement("HFProduction")]
    public float hfProduction;

    [XmlElement("CoralType")]
    public string coralType;

    [XmlElement("PrefTerrain")]
    public string prefTerrain;

    public string dataToString() {
        string output = "---CoralData---";
        output += "\nname: " + name;
        output += "\nGrow Time: " + growTime;
        output += "\nSurvivability: " + survivability;
        output += "\nCarnivorous Fish Interest Base: " + cfProduction;
        output += "\nHerbivorous Fish Interest Base: " + hfProduction;
        output += "\nCoral Type: " + coralType;
        output += "\nPref Terrain: " + prefTerrain;
        return output;
    }
}
