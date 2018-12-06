using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIKTouchScript : MonoBehaviour {

    [SerializeField] private float _handDistance;
    [SerializeField] private float _handOffsetDistance;

    [HideInInspector] public Transform LeftHand;
    [HideInInspector] public Transform RightHand;
    [HideInInspector] public PickupableItemScript ItemTouching;
    private Transform _leftHandRest;
    private Transform _rightHandRest;

    private BasePlayerScript _player;
    private Transform _cam;

	// Use this for initialization
	void Awake () {
        LeftHand = transform.Find("Left");
        RightHand = transform.Find("Right");
        _leftHandRest = transform.Find("LeftRest");
        _rightHandRest = transform.Find("RightRest");
        _player = transform.parent.GetComponent<BasePlayerScript>();
        _cam = Camera.main.transform;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //do raycast checks and position hand IK
        RaycastHit leftHit;
        if (Physics.Raycast(transform.position + (transform.right * _handOffsetDistance), transform.forward, out leftHit, _handDistance))
            {
            LeftHand.transform.position = Vector3.Lerp(LeftHand.position, leftHit.point, 0.2f);
            }
        else
            {
            LeftHand.transform.position = Vector3.Lerp(LeftHand.position, _leftHandRest.position, 0.2f);
            }

        RaycastHit rightHit;
        if (Physics.Raycast(transform.position - (transform.right * _handOffsetDistance), transform.forward, out rightHit, _handDistance))
            {
            RightHand.transform.position = rightHit.point;
            PickupableItemScript pickup = rightHit.collider.GetComponent<PickupableItemScript>();
            if (pickup)
                {
                ItemTouching = pickup;
                }
            }
        else
            {
            RightHand.transform.position = Vector3.Lerp(RightHand.position, _rightHandRest.position, 0.2f);
            }

        _player.IsTouching = ItemTouching && (Vector3.Distance(LeftHand.position, _leftHandRest.position) > 0.03f || Vector3.Distance(RightHand.position, _rightHandRest.position) > 0.03f);
        if (!_player.IsTouching && !_player.IsHolding)
            {
            ItemTouching = null;
            }

        //rotate IK along camera
        if (!_player.IsHolding)
            {
            transform.forward = _cam.transform.forward;
            transform.Rotate(Vector3.right, 40, Space.Self);
            }
        else //rotate IK to object held
            {
            Vector3 newRot = (ItemTouching.transform.position - transform.position).normalized;
            newRot.x += 180;
            transform.localEulerAngles = newRot;
            }
        }
}
