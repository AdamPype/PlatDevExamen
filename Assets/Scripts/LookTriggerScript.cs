using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTriggerScript : MonoBehaviour {

    [SerializeField] private Enemy _parent;

    private void OnTriggerStay(Collider other)
        {
        _parent.OnLookTriggerStay(other);
        }

    private void OnTriggerExit(Collider other)
        {
        _parent.OnLookTriggerExit(other);
        }
    }
