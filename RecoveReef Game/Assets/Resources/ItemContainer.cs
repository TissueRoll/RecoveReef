using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("ItemCollection")]
public class ItemContainer
{

    [XmlArray("Items")]
    [XmlArrayItem("Item")]
    public List<Item> items = new List<Item>();

    public static ItemContainer Load(string path) {
        TextAsset ta = Resources.Load<TextAsset>(path);
        XmlSerializer serializer = new XmlSerializer(typeof(ItemContainer));
        StringReader reader = new StringReader(ta.text);
        ItemContainer items = serializer.Deserialize(reader) as ItemContainer;
        reader.Close();
        return items;
    }
}
