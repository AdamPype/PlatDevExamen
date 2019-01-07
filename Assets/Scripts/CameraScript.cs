using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {
    //inspector vars
    [SerializeField] private Vector2 _orbitSpeed;
    [SerializeField] private Vector2 _xClamp;
    //private components
    private Transform _xAxis;
    //properties
    public bool FreezeY { get; set; }

	// Use this for initialization
	void Start () {
        _xAxis = transform.Find("XAxis");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (!FreezeY)
            transform.Rotate(new Vector3(0, Input.GetAxisRaw("Mouse X") * _orbitSpeed.x, 0), Space.Self);

        Vector3 newRot = _xAxis.localEulerAngles;
        newRot.x += Input.GetAxisRaw("Mouse Y") * _orbitSpeed.y;
        newRot.x = BasePlayerScript.ClampAngle(newRot.x, _xClamp.x, _xClamp.y);
        _xAxis.localEulerAngles = newRot;
    }
}
