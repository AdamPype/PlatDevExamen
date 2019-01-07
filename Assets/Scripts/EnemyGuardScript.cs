using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGuardScript : Enemy {

    [SerializeField] private Transform _head;

    private IndicatorScript _indicator;
    private BasePlayerScript _player;
    private SoundManager _snd;

    [SerializeField] private float _disengageRadius;
    [SerializeField] private float _lookAngle;
    [SerializeField] private float _lookSpeed;

    private bool _playerInFOV;
    private Quaternion _startRot;
    private bool _moveHead;
    private bool _playerSpotted;

    INode _rootNode;

    // Use this for initialization
    void Start () {
        Health = 2;
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<BasePlayerScript>();
        Animator = transform.Find("Model").GetComponent<Animator>();
        _indicator = transform.Find("Indicator").GetComponent<IndicatorScript>();
        _startRot = transform.rotation;
        _snd = GetComponent<SoundManager>();

        //behaviour tree
        _rootNode =
            new SequenceNode(
                new ActionNode(Animate),
                new SelectorNode(
                    new SequenceNode(
                        new SelectorNode(
                            new ConditionNode(PlayerSpottedAndInRange),
                            new ConditionNode(HitByBarrel)
                            ),
                        new ActionNode(AimAndShoot)
                        ),
                    new ActionNode(StandGuard)
                    )
                )
            ;

        StartCoroutine("RunTree");
        }

    public override void LateUpdate()
        {
        //rotate head up and down (I'm doing this in lateupdate because otherwise it won't override the animation!)
        if (_moveHead)
            {
            _moveHead = false;
            Vector3 newRot = _head.transform.localEulerAngles;
            newRot.y = 0;
            newRot.x = _lookAngle * Mathf.Sin(Time.frameCount * _lookSpeed);
            _head.transform.localEulerAngles = newRot;
            }
        base.LateUpdate();
        }

    IEnumerator RunTree()
        {
        while (Application.isPlaying)
            {
            yield return _rootNode.Tick();
            }
        }

    //condition node: is player in view true?
    private bool PlayerSpottedAndInRange()
        {
        if (!_playerSpotted)
            {
            if (_playerInFOV)
                _playerSpotted = true;
            }

        return _playerSpotted && _player && Vector3.Distance(transform.position, _player.transform.position) < _disengageRadius;
        }

    private bool HitByBarrel()
        {
        if (Hit) _playerSpotted = true;
        return Hit;
        }

    //stands guard and looks around
    private IEnumerator<NodeResult> StandGuard()
        {
        //tell lateupdate to do up/down movement of head
        _moveHead = true;

        //reset player spotted
        _playerSpotted = false;

        //turn towards player
        transform.rotation = Quaternion.Lerp(transform.rotation, _startRot, 0.2f);

        yield return NodeResult.Succes;
        }

    //shoots at the player
    private IEnumerator<NodeResult> AimAndShoot()
        {
        //turn towards player
        Vector3 newRot = transform.eulerAngles;
        newRot.y = Quaternion.LookRotation(_player.transform.position - transform.position).eulerAngles.y;
        transform.eulerAngles = newRot;

        //shoot
        if (Time.frameCount % 30 == 0)
            {
            _player.Damage(5, transform.position, 0.2f);
            _snd.Play("Gunshot");
            }

        yield return NodeResult.Succes;
        }

    //does the animations
    private IEnumerator<NodeResult> Animate()
        {
        Animator.SetBool("Aim", _playerSpotted);
        if (_playerSpotted) _indicator.Alerted();
        yield return NodeResult.Succes;
        }


    //spot player
    public override void OnLookTriggerStay(Collider other)
        {
        if (other.CompareTag("Player"))
            {
            _playerInFOV = true;
            }
        }

    //player out of view
    public override void OnLookTriggerExit(Collider other)
        {
        if (other.CompareTag("Player"))
            {
            _playerInFOV = false;
            }
        }
    }
