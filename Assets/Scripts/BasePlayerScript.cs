using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//[RequireComponent(typeof(CharacterController))]
public class BasePlayerScript : MonoBehaviour {



    [SerializeField] private float _acceleration;
    [SerializeField] private float _runAcceleration;
    [SerializeField] private float _drag;
    [SerializeField] private float _maximumXZVelocity = (30 * 1000) / (60 * 60); //[m/s] 30km/h
    [SerializeField] private float _jumpHeight;

    private Transform _absoluteTransform;
    private CharacterController _char;
    [HideInInspector] public Animator Anim;
    private Transform _ledgeRaycast;
    private CameraScript _cam;
    private HandIKTouchScript _handsIK;
    private ThrowingScript _thrower;

    [HideInInspector] public Vector3 Velocity = Vector3.zero; // [m/s]
    [HideInInspector] public Vector3 InputMovement;
    [HideInInspector] public bool IsHolding = false;
    [HideInInspector] public bool IsTouching = false;
    private bool _jump;
    private bool _isJumping;
    private bool _isLedgeGrabbing = false;
    private float _notGroundedTimer;

    void Start()
        {
        //attach components
        _char = GetComponent<CharacterController>();
        _absoluteTransform = Camera.main.transform;
        Anim = transform.Find("Model").GetComponent<Animator>();
        _ledgeRaycast = transform.Find("LedgeGrabRaycast");
        _cam = GetComponent<CameraScript>();
        _thrower = transform.Find("Thrower").GetComponent<ThrowingScript>();

        _handsIK = transform.Find("HandIK").GetComponent<HandIKTouchScript>();
        TouchIKBehaviour touchBeh = Anim.GetBehaviour<TouchIKBehaviour>();
        touchBeh.LeftHandPos = _handsIK.LeftHand;
        touchBeh.RightHandPos = _handsIK.RightHand;

        //dependency error
#if DEBUG
        Assert.IsNotNull(_char, "DEPENDENCY ERROR: CharacterController missing from PlayerScript");
#endif

        }

    private void Update()
        {
        InputMovement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;  //.normalized;
        if (Input.GetButtonDown("Jump") && (!_isJumping || _isLedgeGrabbing))
            {
            _jump = true;
            }

        PickupItems();
        }

    private void PickupItems()
        {
        if (!IsHolding && IsTouching && Input.GetButtonDown("Pickup") && _handsIK.ItemTouching.DamageTimer <= 0 && _handsIK.ItemTouching.Health > 0)
            {
            //pick up item
            IsHolding = true;
            _handsIK.ItemTouching.State = PickupableItemScript.PickupItemState.PickedUp;
            }
        else if (IsHolding)
            {
            _handsIK.ItemTouching.transform.parent = _thrower.transform;
            _thrower.DrawParabola = true;
            _handsIK.ItemTouching.transform.localPosition = Vector3.Lerp(_handsIK.ItemTouching.transform.localPosition, Vector3.zero + (Vector3.up * _handsIK.ItemTouching.Rend.bounds.extents.y), 0.2f);

            //throw
            if (Input.GetButtonDown("Pickup"))
                {
                _handsIK.ItemTouching.State = PickupableItemScript.PickupItemState.Normal;
                IsHolding = false;
                _thrower.DrawParabola = true;
                Rigidbody rb = _handsIK.ItemTouching.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.AddForce(_thrower.GetDirection() * rb.mass, ForceMode.Impulse);
                _handsIK.ItemTouching.transform.parent = null;
                }
            }
        else
            {
            _thrower.DrawParabola = false;
            }
        }

    void FixedUpdate()
        {
        TimeGrounded();
        CheckLedgeGrab();

        if (_isLedgeGrabbing)
            {
            JumpLedge();
            }
        else
            {
            ApplyGround();
            ApplyGravity();
            ApplyMovement();
            ApplyDragOnGround();
            ApplyJump();
            LimitXZVelocity();

            DoMovement();
            }
        AnimateMovement();
        }

    private void OnTriggerEnter(Collider other)
        {
        if (Velocity.y > 0)
            {
            Velocity.y = 0;
            }
        }

    private void JumpLedge()
        {
        if (_jump)
            {
            _isLedgeGrabbing = false;
            //jump
            Velocity.y += Mathf.Sqrt(2 * Physics.gravity.magnitude * _jumpHeight / 1.5f);
            Velocity += transform.forward * 2;
            _jump = false;
            _isJumping = true;
            }
        }

    private void TimeGrounded()
        {
        if (!_char.isGrounded)
            {
            _notGroundedTimer++;
            }
        else
            {
            _notGroundedTimer = 0;
            }
        }

    private void CheckLedgeGrab()
        {
        if (_notGroundedTimer > 2 && Velocity.y < 0)
            {
            RaycastHit hit;
            if (Physics.Raycast(_ledgeRaycast.position, Vector3.down, out hit, 0.1f))
                {
                _isLedgeGrabbing = true;
                Velocity = Vector3.zero;
                }
            }
        _cam.FreezeY = _isLedgeGrabbing;
        }

    private void AnimateMovement()
        {
        Vector3 XZvel = Vector3.Scale(Velocity, new Vector3(1, 0, 1));
        Vector3 localVelXZ = transform.InverseTransformDirection(XZvel);
        Anim.SetFloat("VerticalVelocity", (localVelXZ.z * (_drag)) / _maximumXZVelocity);
        Anim.SetFloat("HorizontalVelocity", (localVelXZ.x * (_drag)) / _maximumXZVelocity);
        Anim.SetBool("Jumping", _isJumping);
        Anim.SetBool("LedgeGrabbing", _isLedgeGrabbing);
        Anim.SetBool("Falling", _notGroundedTimer > 2 && Velocity.y < 0);
        Anim.SetBool("Touch", IsHolding || (_notGroundedTimer <= 2 && IsTouching));

        //run
        if (_notGroundedTimer <= 2 && Velocity != Vector3.zero && Input.GetButton("Run"))
            {
            Anim.speed = 1.2f;
            }
        else
            {
            Anim.speed = 1;
            }
        }

    private Vector3 RelativeDirection(Vector3 direction)
        {
        //get relative rotation from camera
        Vector3 xzForward = Vector3.Scale(_absoluteTransform.forward, new Vector3(1, 0, 1));
        Quaternion relativeRot = Quaternion.LookRotation(direction);

        return relativeRot.eulerAngles;
        }


    private void ApplyGround()
        {
        if (_char.isGrounded)
            {
            //ground velocity
            Velocity -= Vector3.Project(Velocity, Physics.gravity);
            }
        }

    private void ApplyGravity()
        {
        if (!_char.isGrounded)
            {
            //apply gravity
            Velocity += Physics.gravity * Time.deltaTime;
            }
        }

    private void ApplyMovement()
        {
        if (_char.isGrounded)
            {
            //get relative rotation from camera
            Vector3 xzForward = Vector3.Scale(_absoluteTransform.forward, new Vector3(1, 0, 1));
            Quaternion relativeRot = Quaternion.LookRotation(xzForward);

            //move in relative direction
            Vector3 relativeMov = relativeRot * InputMovement;
            float acc = _acceleration;
            if (Input.GetButton("Run"))
                {
                acc = _runAcceleration;
                }
            Velocity += relativeMov * acc * Time.deltaTime;

            //_anim.transform.rotation = Quaternion.LookRotation(xzForward);
            }

        }

    private void LimitXZVelocity()
        {
        Vector3 yVel = Vector3.Scale(Velocity, Vector3.up);
        Vector3 xzVel = Vector3.Scale(Velocity, new Vector3(1, 0, 1));

        xzVel = Vector3.ClampMagnitude(xzVel, _maximumXZVelocity);

        Velocity = xzVel + yVel;
        }

    private void ApplyDragOnGround()
        {
        if (_char.isGrounded)
            {
            //drag
            Velocity = Velocity * (1 - _drag * Time.deltaTime); //same as lerp
            }
        }

    private void ApplyJump()
        {
        if (_char.isGrounded && _jump)
            {
            Velocity.y += Mathf.Sqrt(2 * Physics.gravity.magnitude * _jumpHeight);
            _jump = false;
            _isJumping = true;
            }
        else if (_char.isGrounded)
            {
            _isJumping = false;
            }
        }

    private void DoMovement()
        {
        //do velocity / movement on character controller
        Vector3 movement = Velocity * Time.deltaTime;
        _char.Move(movement);
        }

    public static float ClampAngle(float angle, float min, float max)
        {
        angle = Mathf.Repeat(angle, 360);
        min = Mathf.Repeat(min, 360);
        max = Mathf.Repeat(max, 360);
        bool inverse = false;
        var tmin = min;
        var tangle = angle;
        if (min > 180)
            {
            inverse = !inverse;
            tmin -= 180;
            }
        if (angle > 180)
            {
            inverse = !inverse;
            tangle -= 180;
            }
        var result = !inverse ? tangle > tmin : tangle < tmin;
        if (!result)
            angle = min;

        inverse = false;
        tangle = angle;
        var tmax = max;
        if (angle > 180)
            {
            inverse = !inverse;
            tangle -= 180;
            }
        if (max > 180)
            {
            inverse = !inverse;
            tmax -= 180;
            }

        result = !inverse ? tangle < tmax : tangle > tmax;
        if (!result)
            angle = max;
        return angle;
        }
    }
