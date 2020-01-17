using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("SubstrataCollection")]
public class SubstrataDataContainer
{
    [XmlArray("Substratas")]
    [XmlArrayItem("Substrata")]
    public List<SubstrataData> subsrata = new List<SubstrataData>();

    public static SubstrataDataContainer Load(string path) {
        TextAsset ta = Resources.Load<TextAsset>(path);
        XmlSerializer serializer = new XmlSerializer(typeof(SubstrataDataContainer));
        StringReader reader = new StringReader(ta.text);
        SubstrataDataContainer items = serializer.Deserialize(reader) as SubstrataDataContainer;
        reader.Close();
        return items;
    }
}
