using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("AlgaeDataCollection")]
public class AlgaeDataContainer
{
    [XmlArray("AlgaeDatas")]
    [XmlArrayItem("AlgaeData")]
    public List<AlgaeData> algae = new List<AlgaeData>();

    public static AlgaeDataContainer Load(string path) {
        TextAsset ta = Resources.Load<TextAsset>(path);
        XmlSerializer serializer = new XmlSerializer(typeof(AlgaeDataContainer));
        StringReader reader = new StringReader(ta.text);
        AlgaeDataContainer items = serializer.Deserialize(reader) as AlgaeDataContainer;
        reader.Close();
        return items;
    }
}
