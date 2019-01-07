using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
    {

    public virtual Animator Animator { get; set; }
    public virtual bool Hit { get; set; }

    public virtual int Health { get; set; }
    private Vector3 _bouyEffectAmplitude = Vector3.zero;

    public virtual void Update()
        {
        //bouy effect
        Animator.transform.localEulerAngles = new Vector3(
            _bouyEffectAmplitude.x * Mathf.Sin(Time.frameCount * 0.05f),
            _bouyEffectAmplitude.y * Mathf.Sin(Time.frameCount * 0.05f),
            _bouyEffectAmplitude.z * Mathf.Sin(Time.frameCount * 0.05f)
            ) * 20;
        _bouyEffectAmplitude = Vector3.Lerp(_bouyEffectAmplitude, Vector3.zero, 0.05f);

        //death
        if (Health <= 0)
            {
            Animator.transform.localScale = Vector3.Lerp(Animator.transform.localScale, Vector3.zero, 0.1f);
            if (Animator.transform.localScale.magnitude <= 0.1f) Destroy(gameObject);
            }
        }

    public virtual void LateUpdate()
        {
        //reset Hit
        if (Hit) Hit = false;
        }

    public void Damage(int dmg, Vector3 positionOfInfluence)
        {
        Health -= dmg;
        _bouyEffectAmplitude += (positionOfInfluence - transform.position).normalized * 5;
        Hit = true;
        }

    public abstract void OnLookTriggerStay(Collider other);

    public virtual void OnLookTriggerExit(Collider other) { }
    }
