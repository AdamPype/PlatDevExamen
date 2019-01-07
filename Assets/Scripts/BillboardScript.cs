using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardScript : MonoBehaviour {
    
    [SerializeField] private float _minDistance;

    // Update is called once per frame
    void Update () {
        //get rotation
        Vector3 newRot = Quaternion.LookRotation(transform.position - Camera.main.transform.position).eulerAngles;

        //limit billboarding
        if (Vector3.Distance(Camera.main.transform.position, transform.position) < _minDistance)
            {
            newRot = new Vector3(transform.eulerAngles.x, newRot.y, transform.eulerAngles.z);
            }

        //apply rotation
        transform.eulerAngles = newRot;
	}
}
