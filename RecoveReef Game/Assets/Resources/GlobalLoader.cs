using UnityEngine;
using System.Collections;

public class GlobalLoader : MonoBehaviour
{
    public const string path = "Globalss";

    void Start() {
        GlobalContainer g = GlobalContainer.Load(path);
        print(g.gvars.what_are());
    }
}