using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorScript : MonoBehaviour {

    public float IndicatorValue { get; set; }
    public bool Searching { get; set; }

    private Transform _fill;
    private Transform _mask;
    private SpriteRenderer _alert;
    private SoundManager _snd;

    private bool _prevQuestionMark;

    private void Start()
        {
        _fill = transform.Find("White");
        _mask = transform.Find("Black");
        _alert = transform.Find("Alert").GetComponent<SpriteRenderer>();
        _snd = GetComponent<SoundManager>();
        }

    private void Update()
        {
        //increase indicator value
        Vector3 newScale = _fill.localScale;
        newScale.y = IndicatorValue;
        _fill.localScale = newScale;

        //enable question mark when searching or indicator value is bigger than 0
        bool enableQuestionMark = _alert.color.a <= 0.1f && (IndicatorValue > 0 || Searching);
        _fill.gameObject.SetActive(enableQuestionMark);
        _mask.gameObject.SetActive(enableQuestionMark);

        //play search sound
        if (!_prevQuestionMark && _mask.gameObject.activeSelf)
            _snd.Play("Search");
        _prevQuestionMark = _mask.gameObject.activeSelf;

        _alert.color = Color.Lerp(_alert.color, Color.clear, 0.1f);
        }

    public void Alerted()
        {
        if (_alert.color.a < 0.1f)
            _snd.Play("Alert");
        _alert.color = Color.white;
        }
    }
