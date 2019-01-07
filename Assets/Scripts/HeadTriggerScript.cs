using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTriggerScript : MonoBehaviour {

    private BasePlayerScript _player;

    private void Start()
        {
        _player = transform.parent.GetComponent<BasePlayerScript>();
        }

    private void OnTriggerStay(Collider other)
        {
        _player.HitRoof();
        }
    }
