using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandIKTouchScript : MonoBehaviour {

    //inspector private vars
    [SerializeField] private float _handDistance;
    [SerializeField] private float _handOffsetDistance;
    //component properties
    public Transform LeftHand { get; set; }
    public Transform RightHand { get; set; }
    public PickupableItemScript ItemTouching { get; set; }
    //private components
    private Transform _leftHandRest;
    private Transform _rightHandRest;
    private Transform _cam;
    //properties
    public bool IsTouching { get; set; }
    public bool IsHolding { get; set; }
    public bool CanHold { get; set; }

	// Use this for initialization
	void Awake () {
        LeftHand = transform.Find("Left");
        RightHand = transform.Find("Right");
        _leftHandRest = transform.Find("LeftRest");
        _rightHandRest = transform.Find("RightRest");
        _cam = Camera.main.transform;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //do raycast checks and position hand IK
        RaycastHit leftHit;
        if (Physics.Raycast(transform.position + (transform.right * _handOffsetDistance), transform.forward, out leftHit, _handDistance))
            {
            LeftHand.transform.position = Vector3.Lerp(LeftHand.position, leftHit.point, 0.2f);
            PickupableItemScript pickup = leftHit.collider.GetComponent<PickupableItemScript>();
            if (pickup && !IsHolding)
                {
                ItemTouching = pickup;
                }
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
            if (pickup && !IsHolding)
                {
                ItemTouching = pickup;
                }
            }
        else
            {
            RightHand.transform.position = Vector3.Lerp(RightHand.position, _rightHandRest.position, 0.2f);
            }

        //see if the item is being touched
        IsTouching = ItemTouching && (Vector3.Distance(LeftHand.position, _leftHandRest.position) > 0.03f && Vector3.Distance(RightHand.position, _rightHandRest.position) > 0.03f);
        //make sure the item can be held
        if (ItemTouching)
            CanHold = ItemTouching.Health > 0 && ItemTouching.DamageTimer <= 0;
        //set item holding to null if nothing is being held or touched
        if (!IsTouching && !IsHolding)
            {
            ItemTouching = null;
            }

        //rotate IK along camera
        if (!IsHolding)
            {
            transform.forward = _cam.transform.forward;
            transform.Rotate(Vector3.right, 40, Space.Self);
            }
        else //rotate IK to object held
            {
            Vector3 newRot = transform.localEulerAngles;
            newRot.x = -75;
            transform.localEulerAngles = newRot;
            }
        }
}
