using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioInstanceScript : MonoBehaviour {

    public float Volume { get; set; }
    [SerializeField] private float _volumeScale;

    [SerializeField] private float _appearTime;
    private float _timer;

    private void Start()
        {
        _timer = _appearTime;
        transform.localScale = Vector3.one * _volumeScale * Volume;
        }

    private void Update()
        {
        _timer -= Time.deltaTime;
        if (_timer <= 0)
            {
            Destroy(gameObject);
            }
        }

    }
