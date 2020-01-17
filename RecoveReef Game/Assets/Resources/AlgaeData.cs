using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;

public class AlgaeData
{
    [XmlAttribute("name")]
    public string name;
    [XmlElement("GrowTime")]
    public float growTime;
    [XmlElement("Survivability")]
    public float survivability;
    [XmlElement("CarnivorousFishInterestBase")]
    public float carnivorousFishInterestBase;
    [XmlElement("HerbivorousFishInterestBase")]
    public float herbivorousFishInterestBase;
    [XmlElement("AlgaeType")]
    public string algaeType;
    [XmlElement("PrefTerrain")]
    public string prefTerrain;

    public string dataToString() {
        string output = "---AlgaeData---";
        output += "\nname: " + name;
        output += "\nGrow Time: " + growTime;
        output += "\nSurvivability: " + survivability;
        output += "\nCarnivorous Fish Interest Base: " + carnivorousFishInterestBase;
        output += "\nHerbivorous Fish Interest Base: " + herbivorousFishInterestBase;
        output += "\nAlgae Type: " + algaeType;
        output += "\nPref Terrain: " + prefTerrain;
        return output;
    }
}
