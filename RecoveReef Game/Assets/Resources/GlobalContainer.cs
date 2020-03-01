using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("GlobalCollection")]
public class GlobalContainer
{

    [XmlArray("Globals")]
    [XmlArrayItem("Global")]
    public List<Globals> globals = new List<Globals>();

    public static GlobalContainer Load(string path) {
        TextAsset ta = Resources.Load<TextAsset>(path);
        XmlSerializer serializer = new XmlSerializer(typeof(GlobalContainer));
        StringReader reader = new StringReader(ta.text);
        GlobalContainer items = serializer.Deserialize(reader) as GlobalContainer;
        reader.Close();
        return items;
    }
}
