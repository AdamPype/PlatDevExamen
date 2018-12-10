using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//[RequireComponent(typeof(CharacterController))]
public class BasePlayerScript : MonoBehaviour {

    //state machine
    public enum PlayerState {
        LedgeGrabbing,
        Normal,
        Holding
        }
    private PlayerState _state = PlayerState.Normal;
    //inspector vars
    [SerializeField] private float _acceleration;
    [SerializeField] private float _runAcceleration;
    [SerializeField] private float _drag;
    [SerializeField] private float _maximumXZVelocity = (30 * 1000) / (60 * 60); //[m/s] 30km/h
    [SerializeField] private float _jumpHeight;
    //private components
    private Transform _absoluteTransform;
    private CharacterController _char;
    private Animator _anim;
    private CameraScript _cam;
    private HandIKTouchScript _handsIK;
    private AimingArchScript _aimingArch;
    private Transform _ledgeRaycast;
    //private vars
    private Vector3 _velocity = Vector3.zero; // [m/s]
    private Vector3 _inputMovement;
    private bool _jump;
    private bool _isJumping;
    private float _notGroundedTimer;

    void Start()
        {
        //attach components
        _char = GetComponent<CharacterController>();
        _absoluteTransform = Camera.main.transform;
        _anim = transform.Find("Model").GetComponent<Animator>();
        _ledgeRaycast = transform.Find("LedgeGrabRaycast");
        _cam = GetComponent<CameraScript>();
        _aimingArch = transform.Find("Thrower").GetComponent<AimingArchScript>();

        _handsIK = transform.Find("HandIK").GetComponent<HandIKTouchScript>();
        TouchIKBehaviour touchBeh = _anim.GetBehaviour<TouchIKBehaviour>();
        touchBeh.LeftHandPos = _handsIK.LeftHand;
        touchBeh.RightHandPos = _handsIK.RightHand;

        //dependency error
#if DEBUG
        Assert.IsNotNull(_char, "DEPENDENCY ERROR: CharacterController missing from PlayerScript");
#endif

        }

    private void Update()
        {
        _inputMovement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;  //.normalized;
        if (Input.GetButtonDown("Jump") && (!_isJumping || _state == PlayerState.LedgeGrabbing))
            {
            _jump = true;
            }

        PickupItems();
        }

    private void PickupItems()
        {
        switch (_state)
            {
            case PlayerState.Normal:
                _aimingArch.DrawParabola = false;
                if (_handsIK.IsTouching && Input.GetButtonDown("Pickup") && _handsIK.CanHold)
                    {
                    //pick up item
                    _handsIK.IsHolding = true;
                    _handsIK.ItemTouching.State = PickupableItemScript.PickupItemState.PickedUp;
                    _state = PlayerState.Holding;
                    }
                break;
            case PlayerState.Holding:
                //holding item
                _handsIK.ItemTouching.transform.parent = _aimingArch.transform;
                _aimingArch.DrawParabola = true;
                _handsIK.ItemTouching.transform.localPosition = Vector3.Lerp(_handsIK.ItemTouching.transform.localPosition, Vector3.zero + (Vector3.up * _handsIK.ItemTouching.Rend.bounds.extents.y), 0.2f);

                //throw
                if (Input.GetButtonDown("Pickup"))
                    {
                    _handsIK.IsHolding = false;
                    _handsIK.ItemTouching.ThrowItem(_aimingArch);
                    _state = PlayerState.Normal;
                    }
                break;
            default:
                _aimingArch.DrawParabola = false;
                break;
            }
        }

    void FixedUpdate()
        {
        TimeGrounded();
        CheckLedgeGrab();

        if (_state == PlayerState.LedgeGrabbing)
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
        //bumping your head against a roof
        if (_velocity.y > 0)
            {
            _velocity.y = 0;
            }
        }

    private void JumpLedge()
        {
        if (_jump)
            {
            _state = PlayerState.Normal;
            //jump
            _velocity.y += Mathf.Sqrt(2 * Physics.gravity.magnitude * _jumpHeight / 1.5f);
            _velocity += transform.forward * 2;
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
        if (_notGroundedTimer > 2 && _velocity.y < 0 && _state == PlayerState.Normal)
            {
            RaycastHit hit;
            if (Physics.Raycast(_ledgeRaycast.position, Vector3.down, out hit, 0.1f))
                {
                _state = PlayerState.LedgeGrabbing;
                _velocity = Vector3.zero;
                }
            }
        _cam.FreezeY = _state == PlayerState.LedgeGrabbing;
        }

    private void AnimateMovement()
        {
        Vector3 XZvel = Vector3.Scale(_velocity, new Vector3(1, 0, 1));
        Vector3 localVelXZ = transform.InverseTransformDirection(XZvel);
        _anim.SetFloat("VerticalVelocity", (localVelXZ.z * (_drag)) / _maximumXZVelocity);
        _anim.SetFloat("HorizontalVelocity", (localVelXZ.x * (_drag)) / _maximumXZVelocity);
        _anim.SetBool("Jumping", _isJumping);
        _anim.SetBool("LedgeGrabbing", _state == PlayerState.LedgeGrabbing);
        _anim.SetBool("Falling", _notGroundedTimer > 2 && _velocity.y < 0);
        _anim.SetBool("Touch", _handsIK.IsHolding || (_notGroundedTimer <= 2 && _handsIK.IsTouching));

        //run
        if (_notGroundedTimer <= 2 && _velocity != Vector3.zero && Input.GetButton("Run"))
            {
            _anim.speed = 1.2f;
            }
        else
            {
            _anim.speed = 1;
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
            _velocity -= Vector3.Project(_velocity, Physics.gravity);
            }
        }

    private void ApplyGravity()
        {
        if (!_char.isGrounded)
            {
            //apply gravity
            _velocity += Physics.gravity * Time.deltaTime;
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
            Vector3 relativeMov = relativeRot * _inputMovement;
            float acc = _acceleration;
            if (Input.GetButton("Run"))
                {
                acc = _runAcceleration;
                }
            _velocity += relativeMov * acc * Time.deltaTime;

            //_anim.transform.rotation = Quaternion.LookRotation(xzForward);
            }

        }

    private void LimitXZVelocity()
        {
        Vector3 yVel = Vector3.Scale(_velocity, Vector3.up);
        Vector3 xzVel = Vector3.Scale(_velocity, new Vector3(1, 0, 1));

        xzVel = Vector3.ClampMagnitude(xzVel, _maximumXZVelocity);

        _velocity = xzVel + yVel;
        }

    private void ApplyDragOnGround()
        {
        if (_char.isGrounded)
            {
            //drag
            _velocity = _velocity * (1 - _drag * Time.deltaTime); //same as lerp
            }
        }

    private void ApplyJump()
        {
        if (_char.isGrounded && _jump)
            {
            _velocity.y += Mathf.Sqrt(2 * Physics.gravity.magnitude * _jumpHeight);
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
        Vector3 movement = _velocity * Time.deltaTime;
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
