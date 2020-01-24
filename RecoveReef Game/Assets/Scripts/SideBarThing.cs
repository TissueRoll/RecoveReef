using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SideBarThing : MonoBehaviour
{
    public void growCoral(int x) {
        GameManager.instance.tryGrowCoral(x);
    }
    public void setReveal() {
        GameManager.instance.openThing();
    }
    public void setNum(int x) {
        GameManager.instance.change_coral(x);
    }
    public void opNursery() {
        GameManager.instance.operateNursery();
    }
}
