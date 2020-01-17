using UnityEngine;
using System.Xml;
using System.Xml.Serialization;

public class Globals
{
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

    public string what_are() {
        string output = "---";
        output += "\nzoom: " + zoom;
        output += "\nupdateDelay: " + updateDelay;
        output += "\nmaxSpaceInNursery: " + maxSpaceInNursery;
        output += "\nmaxSpacePerCoral: " + maxSpacePerCoral;
        output += "\nfeedbackDelayTime: " + feedbackDelayTime;
        output += "\nmaxGameTime: " + maxGameTime;
        return output;
    }
}
