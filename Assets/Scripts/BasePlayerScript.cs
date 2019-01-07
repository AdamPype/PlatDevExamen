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
    [SerializeField] private float _sneakAcceleration;
    [SerializeField] private float _drag;
    [SerializeField] private float _maximumXZVelocity = (30 * 1000) / (60 * 60); //[m/s] 30km/h
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _invincibleTime;
    [SerializeField] private float _deadZoneHeight = -5;
    //private components
    private Transform _absoluteTransform;
    private CharacterController _char;
    private Animator _anim;
    private CameraScript _cam;
    private HandIKTouchScript _handsIK;
    private AimingArchScript _aimingArch;
    private Transform _ledgeRaycast;
    private AudioEmitterScript _emitter;
    private SkinnedMeshRenderer[] _rends;
    private HealthUIScript _healthUI;
    private SoundManager _snd;
    //private vars
    private Vector3 _velocity = Vector3.zero; // [m/s]
    private Vector3 _inputMovement;
    private bool _jump;
    private bool _isJumping;
    private float _notGroundedTimer;
    private Vector3 _aimingArchStartPos;
    private float _health = 100;
    private float _invincibleTimer;
    private Vector3 _bouyEffectAmplitude = Vector3.zero;

    void Start()
        {
        //attach components
        _char = GetComponent<CharacterController>();
        _absoluteTransform = Camera.main.transform;
        _anim = transform.Find("Model").GetComponent<Animator>();
        _ledgeRaycast = transform.Find("LedgeGrabRaycast");
        _cam = GetComponent<CameraScript>();
        _aimingArch = transform.Find("AimingArch").GetComponent<AimingArchScript>();
        _emitter = GetComponent<AudioEmitterScript>();
        _rends = _anim.GetComponentsInChildren<SkinnedMeshRenderer>();
        _healthUI = transform.Find("HealthUI").GetComponent<HealthUIScript>();
        _snd = GetComponent<SoundManager>();

        _handsIK = transform.Find("HandIK").GetComponent<HandIKTouchScript>();
        TouchIKBehaviour touchBeh = _anim.GetBehaviour<TouchIKBehaviour>();
        touchBeh.LeftHandPos = _handsIK.LeftHand;
        touchBeh.RightHandPos = _handsIK.RightHand;

        //set tracking vars
        _aimingArchStartPos = _aimingArch.transform.localPosition;

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
        EmitFootsteps();
        Invincibility();
        AnimateDamage();
        DestroyWhenOffMap();
        }

    private void DestroyWhenOffMap()
        {
        if (transform.position.y < _deadZoneHeight)
            {
            _health = 0;
            }
        }

    private void AnimateDamage()
        {
        //bouy effect
        _anim.transform.localEulerAngles = new Vector3(
            _bouyEffectAmplitude.x * Mathf.Sin(Time.frameCount * 0.05f),
            _bouyEffectAmplitude.y * Mathf.Sin(Time.frameCount * 0.05f),
            _bouyEffectAmplitude.z * Mathf.Sin(Time.frameCount * 0.05f)
            ) * 20;
        _bouyEffectAmplitude = Vector3.Lerp(_bouyEffectAmplitude, Vector3.zero, 0.05f);

        //death
        if (_health <= 0)
            {
            _anim.transform.localScale = Vector3.Lerp(_anim.transform.localScale, Vector3.zero, 0.1f);
            if (_anim.transform.localScale.magnitude <= 0.1f)
                {
                //death
                Camera.main.transform.parent = null;
                Destroy(gameObject);
                }
            }
        }

    public void Damage(int dmg, Vector3 positionOfInfluence, float customInvincibilityTime = -1)
        {
        if (_invincibleTimer <= 0)
            {
            _health -= dmg;
            _invincibleTimer = customInvincibilityTime >= 0 ? customInvincibilityTime : _invincibleTime;
            _bouyEffectAmplitude += (positionOfInfluence - transform.position).normalized;

            //ui
            _healthUI.Damage((int)_health);
            }
        }

    private void Invincibility()
        {
        if (_invincibleTimer > 0)
            {
            _invincibleTimer -= Time.deltaTime;
            if (Time.frameCount % 2 == 0)
                {
                foreach (SkinnedMeshRenderer rend in _rends)
                    {
                    float h, s, v;
                    Color.RGBToHSV(rend.material.color, out h, out s, out v);
                    if (v > 0.5f)
                        rend.gameObject.SetActive(!rend.gameObject.activeSelf);
                    }
                }
            if (_invincibleTimer <= 0)
                {
                foreach (SkinnedMeshRenderer rend in _rends)
                    {
                    rend.gameObject.SetActive(true);
                    }
                _invincibleTimer = 0;
                }
            }
        }

    private void EmitFootsteps()
        {
        if (_inputMovement != Vector3.zero && _notGroundedTimer <= 2)
            {
            int soundfreq = 13;
            string soundName = "Step";
            float soundSize = 0.5f;
            if ((Input.GetButton("Sneak") || Input.GetAxisRaw("Sneak") != 0))
                {
                soundfreq = 20;
                soundName = "StepM";
                soundSize = 0;
                }
            else if (Input.GetButton("Run"))
                {
                soundfreq = 10;
                soundName = "StepR";
                soundSize = 1;
                }

            if (Time.frameCount % soundfreq == 0)
                {
                _snd.Play(soundName);
                _emitter.EmitAudio(transform.position, soundSize);
                }
            }
        }

    private void PickupItems()
        {
        //pivk up and throw
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
                if (_handsIK.ItemTouching) //just checking if there is an item (incase it gets destroyed by falling off)
                    {
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
                        _anim.SetTrigger("Throw");
                        }
                    }
                break;
            default:
                _aimingArch.DrawParabola = false;
                break;
            }

        //move object position lower while sneaking
        if ((Input.GetButton("Sneak") || Input.GetAxisRaw("Sneak") != 0))
            {
            _aimingArch.transform.localPosition = _aimingArchStartPos + Vector3.down * 0.2f;
            }
        else
            {
            _aimingArch.transform.localPosition = _aimingArchStartPos;
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
            ApplyGravity();
            ApplyGround();
            ApplyMovement();
            ApplyDragOnGround();
            ApplyJump();
            LimitXZVelocity();

            DoMovement();
            }
        AnimateMovement();
        }

    public void HitRoof()
        {
        if (_velocity.y > 0)
            {
            _velocity.y = 0;
            }
        }

    private void OnTriggerEnter(Collider other)
        {
        //enemy hitting you
        if (other.CompareTag("Enemy"))
            {
            Damage(20, other.transform.position);
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
            _notGroundedTimer = 5;
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
        _anim.SetBool("Sneaking", (Input.GetButton("Sneak") || Input.GetAxisRaw("Sneak") != 0) && _notGroundedTimer <= 2);

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
        //Vector3 xzForward = Vector3.Scale(_absoluteTransform.forward, new Vector3(1, 0, 1));
        Quaternion relativeRot = Quaternion.LookRotation(direction);

        return relativeRot.eulerAngles;
        }


    private void ApplyGround()
        {
        if (_notGroundedTimer < 2)
            {
            //ground velocity
            _velocity -= Vector3.Project(_velocity, Physics.gravity);
            if (!_jump && !_isJumping)
                _velocity += Physics.gravity * Time.deltaTime * 15;
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
            if ((Input.GetButton("Sneak") || Input.GetAxisRaw("Sneak") != 0))
                {
                acc = _sneakAcceleration;
                }
            else if (Input.GetButton("Run"))
                {
                acc = _runAcceleration;
                }
            _velocity += relativeMov * acc * Time.deltaTime;
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
            _notGroundedTimer = 5;
            _snd.Play("Jump");
            }
        else if (_char.isGrounded)
            {
            if (_isJumping) //on land from jump
                {
                _emitter.EmitAudio(transform.position, 3);
                _snd.Play("Land");
                }
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
