using UnityEngine;
using System.Collections;

public class CoralDataLoader : MonoBehaviour
{
    public const string path = "CoralData";

    void Start() {
        CoralDataContainer ic = CoralDataContainer.Load(path);
        foreach(CoralData coral in ic.cd) {
            print(coral.toString());
        }
    }
}
