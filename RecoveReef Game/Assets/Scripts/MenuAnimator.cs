using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAnimator : MonoBehaviour
{
    [SerializeField] private GameObject thing;
    public void OpenThing() {
        if (thing != null) {
            Animator animator = thing.GetComponent<Animator>();
            if (animator != null) {
                bool isOpen = animator.GetBool("open");
                animator.SetBool("open", !isOpen);
            }
        }
    }
}
