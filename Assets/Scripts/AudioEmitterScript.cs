using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEmitterScript : MonoBehaviour {

    [SerializeField] private GameObject _audioPrefab;

    /// <summary>
    /// Creates an audio instance.
    /// </summary>
    /// <param name="position">Position of the sound.</param>
    /// <param name="volume">The volume, this defines the importance and scale of the sound.</param>
    public void EmitAudio(Vector3 position, float volume)
        {
        AudioInstanceScript inst = Instantiate(_audioPrefab).GetComponent<AudioInstanceScript>();
        inst.transform.position = position;
        inst.Volume = volume;
        }
}
