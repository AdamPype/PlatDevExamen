using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableItemScript : MonoBehaviour {

    //state machine
    public enum PickupItemState {
        Normal,
        PickedUp,
        Destroyed
        }
    public PickupItemState State { get; set; }

    [SerializeField] private bool _destructable = false;
    //[SerializeField] private float _damageSpeedThreshold;
    [SerializeField] private float _damageTime;
    [SerializeField] private float _damageGrow;
    [SerializeField] private float _enemyBounceoffForce;
    [SerializeField, Space] private float _deadZoneHeight = -5;

    public Renderer Rend { get; set; }

    public int Health = 0;
    public float Height { get; set; }
    public float DamageTimer { get; set; }

    private Rigidbody _rb;
    private AudioEmitterScript _audioEmitter;
    private SoundManager _snd;
    
    private Color _startCol;
    private Vector3 _startScale;
    private bool _isThrown;


    // Use this for initialization
    void Start () {
        State = PickupItemState.Normal;
        _rb = GetComponent<Rigidbody>();
        Rend = GetComponent<Renderer>();
        _startCol = Rend.material.color;
        _startScale = transform.localScale;
        _audioEmitter = GetComponent<AudioEmitterScript>();
        _snd = GetComponent<SoundManager>();
        }
	
	// Update is called once per frame
	void Update () {
        if (State == PickupItemState.Destroyed)
            {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.1f);
            transform.Rotate(Vector3.one * 5, Space.Self);
            if (transform.localScale.x <= 0.05f)
                {
                Destroy(gameObject);
                }
            }
        else
            {
            //change properties according to state
            switch (State)
                {
                case PickupItemState.Normal:
                    _rb.isKinematic = false;
                    Rend.material.color = _startCol;
                    gameObject.layer = 0;
                    break;
                case PickupItemState.PickedUp:
                    _rb.isKinematic = true;
                    Rend.material.color = Vector4.Scale(_startCol, new Vector4(1, 1, 1, 0.4f));
                    gameObject.layer = 9;
                    break;
                default:
                    break;
                }

            ApplyDamage();
            }

        DestroyWhenOffMap();
        }

    private void DestroyWhenOffMap()
        {
        if (transform.position.y < _deadZoneHeight)
            {
            Destroy(gameObject);
            }
        }

    private void ApplyDamage()
        {
        //do damage indication and destruction
        if (_destructable)
            {
            if (DamageTimer > 0)
                {
                transform.localScale = Vector3.Lerp(_startScale, _startScale * (1 + _damageGrow), DamageTimer / _damageTime);
                DamageTimer -= Time.deltaTime;
                if (DamageTimer <= 0 && Health <= 0)
                    {
                    GetComponent<Collider>().enabled = false;
                    Destroy(_rb);
                    State = PickupItemState.Destroyed;
                    }
                }
            }
        }

    private void OnCollisionEnter(Collision collision)
        {
        if (!collision.collider.isTrigger)
            {
            //on throw with enough force
            if (State == PickupItemState.Normal && _isThrown)
                {
                if (_destructable && DamageTimer <= 0)
                    {
                    //do damage
                    DamageTimer = _damageTime;
                    Health--;
                    _snd.Play("Break");
                    //Debug.Log(_rb.velocity.magnitude);
                    }
                _audioEmitter.EmitAudio(collision.contacts[0].point, _rb.velocity.magnitude);

                if (collision.gameObject.CompareTag("Enemy"))
                    {
                    Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                    enemy.Damage(1, transform.position);
                    _rb.AddForce((transform.position - collision.transform.position).normalized * _enemyBounceoffForce, ForceMode.Impulse);
                    }
                }
            }
        _snd.Play("Hit");
        _isThrown = false;
        }

    internal void ThrowItem(AimingArchScript aimingArch)
        {
        _rb.isKinematic = false;
        _rb.AddForce(aimingArch.Direction, ForceMode.VelocityChange);
        State = PickupItemState.Normal;
        transform.parent = null;
        _isThrown = true;
        _snd.Play("Throw");
        }
    }
