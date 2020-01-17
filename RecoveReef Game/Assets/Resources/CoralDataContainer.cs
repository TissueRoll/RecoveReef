using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("CoralDataCollection")]
public class CoralDataContainer
{
    [XmlArray("CoralDatas")]
    [XmlArrayItem("CoralData")]
    public List<CoralData> corals = new List<CoralData>();

    public static CoralDataContainer Load(string path) {
        TextAsset ta = Resources.Load<TextAsset>(path);
        XmlSerializer serializer = new XmlSerializer(typeof(CoralDataContainer));
        StringReader reader = new StringReader(ta.text);
        CoralDataContainer items = serializer.Deserialize(reader) as CoralDataContainer;
        reader.Close();
        return items;
    }
}
