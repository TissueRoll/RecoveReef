using UnityEngine;
using System.Collections;

public class GlobalLoader : MonoBehaviour
{
    public const string path = "GlobalsXML";

    void Start() {
        GlobalContainer g = GlobalContainer.Load(path);
        // print(g.globalVariables.what_are());
    }
}
