using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowingScript : MonoBehaviour {
    
    [SerializeField] private float _maxLength;
    [SerializeField] private float _segmentLength;
    [SerializeField] private float _throwStrength;
    [SerializeField] private float _yness;

    private LineRenderer _line;
    private Transform _cam;

    private Vector3 _directionVelocity;

    public bool DrawParabola = false;

	// Use this for initialization
	void Start () {
        _line = GetComponent<LineRenderer>();
        _cam = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {
        //rotate direction along camera
        Vector3 newDir = _cam.forward * _throwStrength;
        newDir.y += _yness;
        _directionVelocity = Vector3.Lerp(_directionVelocity, newDir, 0.2f);

        if (DrawParabola)
            {
            if (!_line.enabled)
            _line.enabled = true;

            //draw parabola
            Vector3 prev = transform.position;
            _line.positionCount = 1;
            _line.SetPosition(0, transform.position);

            for (int i = 1; ; i++)
                {
                float t = _segmentLength * i;
                if (t > _maxLength) break;
                Vector3 pos = PlotTrajectoryAtTime(transform.position, _directionVelocity, t);
                if (Physics.Linecast(prev, pos, LayerMask.GetMask("Default"))) break;
                _line.positionCount++;
                _line.SetPosition(_line.positionCount - 1, pos);
                }
            }
        else
            {
            _line.enabled = false;
            }
        }

    private Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, float time)
        {
        return start + startVelocity * time + Physics.gravity * time * time * 0.5f;
        }

    public Vector3 GetDirection()
        {
        //rotate direction along camera
        Vector3 newDir = _cam.forward * _throwStrength;
        newDir.y += _yness;
        return Vector3.Lerp(_directionVelocity, newDir, 0.2f);
        }
    }
