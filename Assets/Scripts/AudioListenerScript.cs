using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerScript : MonoBehaviour {
    
    private List<AudioInstanceScript> _soundsInRange = new List<AudioInstanceScript>();
    
	// Update is called once per frame
	void Update () {
        //make sure there are no empty objects in list
        if (_soundsInRange.Count > 0)
            {
            for (int i = 0; i < _soundsInRange.Count; i++)
                {
                if (!_soundsInRange[i])
                    {
                    _soundsInRange.RemoveAt(i);
                    }
                }
            }
        }

    private void OnTriggerEnter(Collider other)
        {
        if (other.CompareTag("AudioInstance"))
            {
            AudioInstanceScript inst = other.GetComponent<AudioInstanceScript>();
            if (!_soundsInRange.Contains(inst))
                _soundsInRange.Add(inst);
            }
        }

    /// <summary>
    /// Returns the position of the loudest sound in range, preferring the last sound emitted.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetSoundPosition()
        {
        AudioInstanceScript toReturn = null;
        foreach (AudioInstanceScript sound in _soundsInRange)
            {
            if (!toReturn)
                toReturn = sound;
            else if (toReturn && toReturn.Volume <= sound.Volume)
                {
                toReturn = sound;
                }
            }
        return toReturn.transform.position;
        }

    /// <summary>
    /// Returns whether there is a sound in range.
    /// </summary>
    /// <returns></returns>
    public bool SoundInRange()
        {
        //make sure there are no empty objects in list
        if (_soundsInRange.Count > 0)
            {
            for (int i = 0; i < _soundsInRange.Count; i++)
                {
                if (!_soundsInRange[i])
                    {
                    _soundsInRange.RemoveAt(i);
                    }
                }
            }
        return _soundsInRange.Count > 0;
        }
    }
