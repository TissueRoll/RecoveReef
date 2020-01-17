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
    public float survivability;

    [XmlElement("CarnivorousFishInterestBase")]
    public float carnivorousFishInterestBase;

    [XmlElement("HerbivorousFishInterestBase")]
    public float herbivorousFishInterestBase;

    [XmlElement("CoralType")]
    public string coralType;

    [XmlElement("PrefTerrain")]
    public string prefTerrain;

    public string dataToString() {
        string output = "";
        output += "name: " + name;
        output += "\nGrow Time: " + growTime;
        output += "\nSurvivability: " + survivability;
        output += "\nCarnivorous Fish Interest Base: " + carnivorousFishInterestBase;
        output += "\nHerbivorous Fish Interest Base: " + herbivorousFishInterestBase;
        output += "\nCoral Type: " + coralType;
        output += "\nPref Terrain: " + prefTerrain;
        return output;
    }
}
