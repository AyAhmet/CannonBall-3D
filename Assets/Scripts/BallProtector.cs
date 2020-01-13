using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallProtector : MonoBehaviour
{
    void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Cube" || collider.gameObject.tag == "Target") {
            collider.gameObject.GetComponent<Collider>().enabled = false;
        }
    }

}
