using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    bool collidedOnce;

    float radius = 30.0F;
    float power = 100.0F;

    GameObject parentObject;

    void Start() {
        this.GetComponent<Rigidbody>().isKinematic = true;
    }

    void DiasbleAnimator() {
        if (parentObject.GetComponent<Animator>()) {
            parentObject.GetComponent<Animator>().enabled = false;
        }
    }

    void AddExplosion() {

        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

        foreach (Collider hit in colliders) {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null){
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
            }
        }
    }

    void ActivateGravity(GameObject obj) {

        parentObject = obj;

        while (parentObject.transform.parent) {
            parentObject = parentObject.transform.parent.gameObject;
        }

        FindChilds(parentObject.transform);

        void FindChilds(Transform parent) {
            foreach (Transform child in parent) {
                if (child.gameObject.tag == "Cube" || child.gameObject.tag == "Target") {
                    child.gameObject.GetComponent<Rigidbody>().useGravity = true;
                    Destroy(child.gameObject, 10f);
                }
                FindChilds(child);
            }
        }
    }

    void OnCollisionEnter(Collision collision) {

        GameObject gameController = GameObject.Find("Game Controller");
        var controllerScript = gameController.GetComponent<GameController>();

        switch (collision.gameObject.tag) {

            case "Target":
                if (!collidedOnce) {
                    controllerScript.TargetHit(true);
                    collidedOnce = true;
                    ActivateGravity(collision.gameObject);
                    DiasbleAnimator();
                }
                AddExplosion();
                break;

            case "Cube":
                if (!collidedOnce) {
                    controllerScript.TargetHit(false);
                    collidedOnce = true;
                    ActivateGravity(collision.gameObject);
                    DiasbleAnimator();
                }
                AddExplosion();
                break;

            default:
                break;
        }
    }
}
