using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.XR;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

public class Gaze : MonoBehaviour
{
    internal GameObject gazedObject;
    public float gazeMaxDistance = 300;

    void Update()
    {
        // Uses a raycast from the Main Camera to determine which object is gazed upon.
        Vector3 fwd = gameObject.transform.TransformDirection(Vector3.forward);
        Ray ray = new Ray(Camera.main.transform.position, fwd);
        RaycastHit hit;
        Debug.DrawRay(Camera.main.transform.position, fwd);

        if (Physics.Raycast(ray, out hit, gazeMaxDistance) && hit.collider != null)
        {
            if (gazedObject == null)
            {
                gazedObject = hit.transform.gameObject;

                // Set the gazedTarget in the Behaviours class
                Behaviours.instance.gazedTarget = gazedObject;
            }
        }
        else
        {
            ResetGaze();
        }
    }

    // Turn the gaze off, reset the gazeObject in the Behaviours class.
    public void ResetGaze()
    {
        if (gazedObject != null)
        {
            Behaviours.instance.gazedTarget = null;
            gazedObject = null;
        }
    }
}