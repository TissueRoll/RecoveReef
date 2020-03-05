using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class Globals
{
    [XmlAttribute("level")]
    public int level;
    [XmlElement("Zoom")]
    public float zoom;

    [XmlElement("UpdateDelay")]
    public float updateDelay;

    [XmlElement("MaxSpaceInNursery")]
    public int maxSpaceInNursery;

    [XmlElement("MaxSpacePerCoral")]
    public int maxSpacePerCoral;

    [XmlElement("FeedbackDelayTime")]
    public float feedbackDelayTime;
    [XmlElement("MaxGameTime")]
    public float maxGameTime;
    [XmlElement("TimeUntilClimateChange")]
    public float timeUntilClimateChange;
    [XmlElement("Goal")]
    public float goal;

    public string what_are() {
        string output = "---";
        output += "\nlevel: " + level;
        output += "\nzoom: " + zoom;
        output += "\nupdateDelay: " + updateDelay;
        output += "\nmaxSpaceInNursery: " + maxSpaceInNursery;
        output += "\nmaxSpacePerCoral: " + maxSpacePerCoral;
        output += "\nfeedbackDelayTime: " + feedbackDelayTime;
        output += "\nmaxGameTime: " + maxGameTime;
        output += "\ntimeUntilClimateChange: " + timeUntilClimateChange;
        output += "\ngoal: " + goal;
        return output;
    }
}
