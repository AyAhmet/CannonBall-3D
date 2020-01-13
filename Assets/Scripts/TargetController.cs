using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    void EnableCollidersForChildren(Transform parent) {
        foreach (Transform child in parent) {
            if (child.gameObject.tag == "Cube" || child.gameObject.tag == "Target") {
                child.gameObject.GetComponent<Collider>().enabled = true;
            }
            EnableCollidersForChildren(child);
        }
    }

    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Cube" || collider.gameObject.tag == "Target") {
            collider.gameObject.GetComponent<Collider>().enabled = false;
        }

        if (collider.gameObject.tag == "Ball") {
            this.GetComponent<Collider>().enabled = false;
            EnableCollidersForChildren(this.transform);
        }
    }

}
