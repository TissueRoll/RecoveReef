using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBarThing : MonoBehaviour
{

    public void setNum(int x) {
        GameManager.instance.change_coral(x);
    }
}
