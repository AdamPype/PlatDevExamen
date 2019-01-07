using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour {

    [SerializeField] private GameObject _winText;
    [SerializeField] private GameObject _loseText;
    [SerializeField] private Text _loseDescription;

    [SerializeField, Space] private GameObject[] _barrels;
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject[] _enemies;

    private SoundManager _snd;

    private void Start()
        {
        _snd = GetComponent<SoundManager>();
        }

    // Update is called once per frame
    void Update () {
        if (CheckWin())
            {
            if (!_winText.activeSelf) _snd.Play("Win", false, 0);
            _winText.SetActive(true);
            }
        else if (CheckLose())
            {
            if (!_loseText.activeSelf) _snd.Play("Lose", false, 0);
            _loseText.SetActive(true);
            }
	}

    private bool CheckLose()
        {
        bool lost = true;
        foreach (GameObject barrel in _barrels)
            {
            if (barrel)
                {
                lost = false;
                }
            }
        if (!_player)
            {
            lost = true;
            _loseDescription.text = "You died.";
            }
        return lost;
        }

    private bool CheckWin()
        {
        bool win = true;
        foreach (GameObject enemy in _enemies)
            {
            if (enemy) win = false;
            }
        return win;
        }
    }
