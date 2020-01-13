using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float degreeLeft, degreeTotal;
    GameObject turningAroundObject;

    public void TurnAround(GameObject obj, float degree) {
        degreeLeft = Mathf.Abs(degree);
        degreeTotal = degree;
        turningAroundObject = obj;
    }

    void FixCameraPosition() {

        if (degreeLeft > 0) {
            this.transform.RotateAround(turningAroundObject.transform.position, Vector3.up, degreeTotal * Time.deltaTime);
            degreeLeft -= Mathf.Abs(degreeTotal) * Time.deltaTime;

            if (degreeLeft < Mathf.Abs(degreeTotal) * Time.deltaTime) {
                this.transform.RotateAround(turningAroundObject.transform.position, Vector3.up, degreeLeft * Mathf.Sign(degreeTotal));
                degreeLeft = 0;
            }

        } else {
            return;
        }
    }

    void Update()
    {
        FixCameraPosition();
    }
}
