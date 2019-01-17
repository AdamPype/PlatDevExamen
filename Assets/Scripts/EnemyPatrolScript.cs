using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolScript : Enemy {

    enum PatrolEnemyState {
        Roam,
        Search,
        Chase
        }
    private PatrolEnemyState _state = PatrolEnemyState.Roam;

    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private AudioListenerScript _listener;

    private NavMeshAgent _agent;
    private IndicatorScript _indicator;


    [SerializeField, Space] private float _searchTime;
    [SerializeField] private float _waitAtSoundTime;
    [SerializeField] private float _chaseTime;
    [SerializeField] private float _povTime;
    [SerializeField] private float _instantNoticeDistance;

    private float _searchTimer;
    private float _chaseTimer;
    private float _povTimer;
    private BasePlayerScript _player;
    private bool _playerInSightForTime;
    private bool _playerOutOfSight = true;
    private Vector3 _playerLastKnownLocation;
    private int _waypointIndex = 0;

    // Use this for initialization
    public void Start () {
        Health = 2;
        _agent = GetComponent<NavMeshAgent>();
        Animator = transform.Find("Model").GetComponent<Animator>();
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<BasePlayerScript>();
        _povTimer = _povTime;
        _searchTimer = _searchTime;
        _chaseTimer = _chaseTime;
        _indicator = transform.Find("Indicator").GetComponent<IndicatorScript>();
	}
	
	// Update is called once per frame
	public override void Update () {
        base.Update();
        //ANYSTATE
        //Go to chase when a player is in POV
        if (_playerInSightForTime)
            {
            AnyToChase();
            }

        //STATE MACHINE
        if (Health > 0)
            {
            switch (_state)
                {
                case PatrolEnemyState.Roam:
                    RoamState();
                    //go to search when sound is in range
                    if (_listener.SoundInRange())
                        {
                        RoamToSearch();
                        }
                    _agent.speed = 1;
                    break;
                case PatrolEnemyState.Search:
                    SearchState();
                    //count down and reset timer
                    _searchTimer -= Time.deltaTime;
                    if (_listener.SoundInRange())
                        {
                        _searchTimer = _searchTime;
                        }
                    //go to roam after a time
                    if (_searchTimer <= 0)
                        {
                        SearchToRoam();
                        }
                    _agent.speed = 1;
                    break;
                case PatrolEnemyState.Chase:
                    ChaseState();
                    //count down and reset timer
                    _chaseTimer -= Time.deltaTime;
                    if (_playerInSightForTime)
                        {
                        _chaseTimer = _chaseTime;
                        }
                    //go to roam after a time
                    if (_chaseTimer <= 0)
                        {
                        ChaseToRoam();
                        }
                    _agent.speed = 3.5f;
                    break;
                default:
                    break;
                }
            }

        //animation
        Animate();
        }

    private void PlayerOutOfSight()
        {
        _povTimer += Time.deltaTime;
        if (_povTimer >= _povTime)
            {
            _povTimer = _povTime;
            }
        }

    private void Animate()
        {
        //walking movement
        Animator.SetFloat("VerticalVelocity", Vector3.Scale(_agent.velocity, new Vector3(1, 0, 1)).magnitude * 0.25f);

        //navmesh jump
        Animator.SetBool("Jumping", _agent.isOnOffMeshLink);

        //indicator
        _indicator.IndicatorValue = _povTimer > 0 ? 1 - (_povTimer / _povTime) : 0;
        _indicator.Searching = _state == PatrolEnemyState.Search;
        }

    #region States
    private void RoamState()
        {
        Vector3 newRot = transform.eulerAngles;
        //roam to waypoints
        if (_povTimer == _povTime)
            {
            _agent.destination = _waypoints[_waypointIndex].transform.position;
            if (Vector3.Distance(transform.position, _agent.destination) <= 0.05f)
                {
                _waypointIndex++;
                if (_waypointIndex >= _waypoints.Length)
                    {
                    _waypointIndex = 0;
                    }
                }
            newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_agent.velocity).eulerAngles.y, 0.2f);
            }
        else //look at player in view
            {
            _agent.destination = transform.position;
            newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_player.transform.position - transform.position).eulerAngles.y, 0.2f);
            }

        //player out of sight
        if (_povTimer < _povTime && _playerOutOfSight)
            {
            newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_playerLastKnownLocation - transform.position).eulerAngles.y, 0.2f);
            PlayerOutOfSight();
            }

        transform.eulerAngles = newRot;
        }

    private void SearchState()
        {
        Vector3 newRot = transform.eulerAngles;
        //go to sound position
        if (_povTimer == _povTime)
            {
            //go to sound
            if (_listener.SoundInRange()) _agent.destination = _listener.GetSoundPosition();
            //wait at sound a while
            if (Vector3.Distance(transform.position, _agent.destination) <= 0.2f)
                {
                if (Vector3.Distance(transform.position, _agent.destination) > 0.05f)
                    {
                    _searchTimer = _waitAtSoundTime;
                    }
                }
            else newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_agent.velocity).eulerAngles.y, 0.2f);
            }
        else //look at player in view
            {
            _agent.destination = transform.position;
            newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_player.transform.position - transform.position).eulerAngles.y, 0.2f);
            }

        //player out of sight
        if (_povTimer < _povTime && _playerOutOfSight)
            {
            newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_playerLastKnownLocation - transform.position).eulerAngles.y, 0.2f);
            PlayerOutOfSight();
            }

        transform.eulerAngles = newRot;
        }

    private void ChaseState()
        {
        if (_player)
            {
            _agent.destination = _player.transform.position;
            }

        //rotate agent
        Vector3 newRot = transform.eulerAngles;
        newRot.y = Mathf.LerpAngle(newRot.y, Quaternion.LookRotation(_agent.velocity).eulerAngles.y, 0.2f);
        transform.eulerAngles = newRot;
        }
    #endregion

    #region Transitions
    private void SearchToRoam()
        {
        _state = PatrolEnemyState.Roam;
        _searchTimer = _searchTime;
        }

    private void RoamToSearch()
        {
        _state = PatrolEnemyState.Search;
        _searchTimer = _searchTime;
        }

    private void AnyToChase()
        {
        _state = PatrolEnemyState.Chase;
        _searchTimer = _searchTime;
        _indicator.Alerted();
        }

    private void ChaseToRoam()
        {
        _state = PatrolEnemyState.Roam;
        _chaseTimer = _chaseTime;
        }
    #endregion

    public override void OnLookTriggerStay(Collider other)
        {
        //player is in FOV
        if (other.CompareTag("PlayerCollider"))
            {
            RaycastHit hit;
            //there's no wall or object inbetween the player and enemy
            if (!Physics.Linecast(transform.position, other.transform.position, out hit, LayerMask.GetMask("Default")))
                {
                //the player is in POV for the right amount of time or it's in the chase state
                _povTimer -= Time.deltaTime;
                if (_povTimer <= 0 || _state == PatrolEnemyState.Chase || Vector3.Distance(transform.position, other.transform.position) < _instantNoticeDistance)
                    {
                    _playerInSightForTime = true;
                    _povTimer = _povTime;
                    }
                _playerOutOfSight = false;
                }
            else
                {
                _playerInSightForTime = false;
                _playerLastKnownLocation = _player.transform.position;
                _playerOutOfSight = true;
                //Debug.Log(hit.transform.gameObject.name + " is blocking " + gameObject.name + "'s view!");
                }
            }
        }

    public override void OnLookTriggerExit(Collider other)
        {
        if (other.CompareTag("PlayerCollider"))
            {
            _playerInSightForTime = false;
            _playerLastKnownLocation = _player.transform.position;
            _playerOutOfSight = true;
            }
        }
    }
