using UnityEngine;
using System.Collections;

public class CoralDataLoader : MonoBehaviour
{
    public const string path = "CoralDataXML";

    void Start() {
        CoralDataContainer ic = CoralDataContainer.Load(path);
        foreach(CoralData coral in ic.corals) {
            print(coral.toString());
        }
    }
}
