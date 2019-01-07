using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUIScript : MonoBehaviour {

    [SerializeField] private Gradient _gradient;

    private int _health = 100;
    private Vector3 _startScale;
    private SpriteRenderer _rend;

	// Use this for initialization
	void Start () {
        _startScale = transform.localScale;
        _rend = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Lerp(newScale.x, _startScale.x * ((float)_health / 100), 0.2f);
        transform.localScale = newScale;

        _rend.color = Color.Lerp(_rend.color, _gradient.Evaluate(newScale.x / _startScale.x), 0.2f);
	}

    public void Damage(int _newHealth)
        {
        if (_health > 0)
            _health = _newHealth;
        else
            _health = 0;
        }
}
