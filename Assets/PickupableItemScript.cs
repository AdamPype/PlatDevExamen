using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableItemScript : MonoBehaviour {

    public enum PickupItemState {
        Normal,
        PickedUp,
        Destroyed
        }
    [HideInInspector] public PickupItemState State = PickupItemState.Normal;

    [SerializeField] private bool _destructable = false;
    [SerializeField] private float _damageSpeedThreshold;
    public int Health = 0;
    [SerializeField] private float _damageTime;
    [SerializeField] private float _damageGrow;

    [HideInInspector] public Renderer Rend;
    [HideInInspector] public float Height;
    [HideInInspector] public float DamageTimer = 0;
    private Rigidbody _rb;
    
    private Color _startCol;
    private Vector3 _startScale;



    // Use this for initialization
    void Start () {
        _rb = GetComponent<Rigidbody>();
        Rend = GetComponent<Renderer>();
        _startCol = Rend.material.color;
        _startScale = transform.localScale;
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

            //do damage indication and destruction
            if (_destructable)
                {
                if (DamageTimer > 0)
                    {
                    transform.localScale = Vector3.Lerp(_startScale, _startScale * (1 + _damageGrow), DamageTimer/_damageTime);
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
        }

    private void OnCollisionEnter(Collision collision)
        {
        //on collision with enough force
        if (State == PickupItemState.Normal)
            {
            if (_destructable && _rb.velocity.magnitude > _damageSpeedThreshold && DamageTimer <= 0)
                {
                //do damage
                DamageTimer = _damageTime;
                Health--;
                Debug.Log(_rb.velocity.magnitude);
                }
            }
        }
    }
