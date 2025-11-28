using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    /* 1. Enemy spawn prelevel -  done
     * 2. Enemy wait til game start -  done
     * 3. Initialize enemy attack state to false -  done
     * 4. Read MIDI notes from gameplay manager -  done
     * 5. Change enemy attack state to true before 0.150s of note start time, otherwise false -  done
     * 6. enemy attack state switches off for some part of the songs --> chase and dash instead of attacking ( this can be done either through reading a file attached with midi path upon selecting a song or alternate between 2 modes after every 20 seconds)
     * 7. play attack animation when in attack state and at 0.60s before note start time and end animation at after 0.60 s of note start time
     * 8. for prototype, enemy will not have attack animation, but instead it will have a cube shaped attack hitbox that appears during attacking state that will change colorrs upon timing thresholds -  done
     * 9. whenever enemy switch state - spawn a cube behind the enemy with a specific colour
     * 
    */
    // Private variables
    gameplayManager _gameplayManager;
    playerHandler _player;
    CircleCollider2D _playerRangeCircle1Collider, _playerRangeCircle2Collider;
    GameObject _nextAttackHitBox;
    GameObject[] _attackHitBoxes;
    Rigidbody2D _rb;
    Vector3 _newDashDirection, _directionToPlayer, _newEnemyPosition;
    bool _actionPatternAttacking, _attacking, _dashing, _attackHitBoxSpawned, _dashRight, _switchActionPatternFlag, _firstAttackNoteFlag;
    float _timeAfterCurrentNote, _attackStartTime, _attackEndTime, _dashDistance, _playerRangeCircle1Radius, _randomAngle;

    // Public properties
    public bool ActionPatternAttacking
    {
        get { return _actionPatternAttacking; }
        set { _actionPatternAttacking = value; }
    }
    public bool Attacking
    {
        get { return _attacking; }
        set { _attacking = value; }
    }
    public bool Dashing
    {
        get { return _dashing; }
        set { _dashing = value; }
    }

    void Awake()
    {
        // Get components/scripts
        _gameplayManager = GameObject.Find("Gameplay Manager").GetComponent<gameplayManager>();
        _player = GameObject.Find("Player").GetComponent<playerHandler>();
        _attackHitBoxes = new GameObject[] {transform.Find("Left").gameObject, transform.Find("Right").gameObject, transform.Find("Front").gameObject};
        _playerRangeCircle1Collider = GameObject.Find("Player Range (1)").GetComponent<CircleCollider2D>();
        _playerRangeCircle2Collider = GameObject.Find("Player Range (2)").GetComponent<CircleCollider2D>();
    }

    void Start()
    {
        _attacking = false;
        _dashing = false;
        _actionPatternAttacking = true;
        _attackHitBoxSpawned = false;
        _switchActionPatternFlag = true;
        _dashDistance = 4f;
        _playerRangeCircle1Radius = _playerRangeCircle1Collider.radius * transform.lossyScale.x;
    }

    // Update is called once per frame
    void Update()
    {

        _timeAfterCurrentNote = _gameplayManager.Timer - _gameplayManager.MidiParser.MidiNotes[_gameplayManager.NoteIndex].StartTime;
        if (_gameplayManager.GameStarted)
        {
            if (_switchActionPatternFlag) {
                StartCoroutine(switchActionPattern());
            }
            if (_actionPatternAttacking)
            {
                // for attacking, should I go for switching modes between beats so the enemy can dash on this beat then attack on the next beat - or dash and spawn attack at the same time - might be the first one idk
                // if 1st note during attacking pattern while enemy is not close to the player range circle (mayb create another larger circle than the player range one) - teleport enemy to the position that is behind the player and on the player range circumference
                // from this note afterwards, enemy either reposition to the nearest position on the player range circle's circumference and attack on the next beat or within the same beat (consider the 1st comment)
                if (_attacking)
                {
                    _attacking = false;
                    if (_timeAfterCurrentNote >= -0.150f)
                    {
                        if (_firstAttackNoteFlag && !_playerRangeCircle2Collider.bounds.Contains(transform.position))
                        {
                            _newEnemyPosition = _player.transform.position - (_player.MousePos - _player.transform.position).normalized * _playerRangeCircle1Radius;
                            _rb.MovePosition(new Vector2(_newEnemyPosition.x, _newEnemyPosition.y));
                        }
                        else { 
                            _newEnemyPosition = _player.transform.position + (transform.position - _player.transform.position).normalized * _playerRangeCircle1Radius;
                            _rb.MovePosition(new Vector2(_newEnemyPosition.x, _newEnemyPosition.y));
                        }
                        if (!_attackHitBoxSpawned)
                        {
                            // set attack shape active
                            _nextAttackHitBox = _attackHitBoxes[Random.Range(0, _attackHitBoxes.Length)];
                            _nextAttackHitBox.SetActive(true);
                            _attackHitBoxSpawned = true;
                        }
                        else
                        {
                            if ((_timeAfterCurrentNote >= -0.150f && _timeAfterCurrentNote < -0.060f) || _timeAfterCurrentNote > 0.60f)
                            {
                                // set attack shape colour to red // or none idk for now
                                _nextAttackHitBox.GetComponent<SpriteRenderer>().color = Color.red;
                            }
                            else if (_timeAfterCurrentNote >= -0.30f && _timeAfterCurrentNote <= 0.030f)
                            {
                                // set attack shape colour to green
                                _nextAttackHitBox.GetComponent<SpriteRenderer>().color = Color.green;
                            }
                            else if (_timeAfterCurrentNote >= -0.40f && _timeAfterCurrentNote <= 0.040f)
                            {
                                // set attack shape colour to yellow-green
                                _nextAttackHitBox.GetComponent<SpriteRenderer>().color = new Color(0.68f, 1f, 0.18f);
                            }
                            else if (_timeAfterCurrentNote >= -0.50f && _timeAfterCurrentNote <= 0.050f)
                            {
                                // set attack shape colour to yellow
                                _nextAttackHitBox.GetComponent<SpriteRenderer>().color = Color.yellow;
                            }
                            else if (_timeAfterCurrentNote >= -0.60f && _timeAfterCurrentNote <= 0.060f)
                            {
                                // set attack shape colour to orange
                                _nextAttackHitBox.GetComponent<SpriteRenderer>().color = new Color(1f, 0.65f, 0f);
                            }
                            else if (_timeAfterCurrentNote >= 0.70f)
                            {
                                // set attack shape inactive
                                _nextAttackHitBox.SetActive(false);
                                _attackHitBoxSpawned = false;
                            }
                        }
                    }
                    if (_timeAfterCurrentNote >= -0.060f)
                    {
                        // play attack animation
                        // animation duration is 0.120s which is from -0.060s to 0.060s of the note timing
                    }
                }
            }
            else
            {
                // dash fixed distance zig zag - instant - angle fixed - til touch player range circle (if possible)
                // if touch player range circle and still on dashing state, dash to random position on player range circle's circumference
                if (_dashing)
                {
                    _dashing = false;
                    if (_timeAfterCurrentNote == 0f)
                    {
                        if (_playerRangeCircle2Collider.bounds.Contains(transform.position)) { // not in range of outter circle
                            _randomAngle = Random.Range(0f, Mathf.PI * 2f);
                            _rb.MovePosition(new Vector2(_player.transform.position.x, _player.transform.position.y) + new Vector2(Mathf.Cos(_randomAngle), Mathf.Sin(_randomAngle)) * _playerRangeCircle1Radius);
                        }
                        else
                        {
                            _directionToPlayer = (_player.transform.position - transform.position).normalized;
                            if (_dashRight)
                            {
                                _newDashDirection = Quaternion.Euler(0f, 0f, 45f) * _directionToPlayer;
                            }
                            else
                            {
                                _newDashDirection = Quaternion.Euler(0f, 0f, -45f) * _directionToPlayer;
                            }
                            _rb.MovePosition(_rb.position + new Vector2(_newDashDirection.x, _newDashDirection.y) * _dashDistance);
                            _dashRight = !_dashRight;
                        }
                    }
                }
            }
        }

    }

    private void FixedUpdate()
    {
        if (_gameplayManager.GameStarted) {
            RotateEnemy();
        }
    }

    void RotateEnemy() {
        if (transform.rotation != Quaternion.LookRotation(Vector3.forward, (_player.transform.position - transform.position).normalized)) { 
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(Vector3.forward, (_player.transform.position - transform.position).normalized), 200f * Time.deltaTime);
        }
    }

    IEnumerator switchActionPattern() { // fix later: instantiate/set active child cube and change color to green on dash and red on attack

        _switchActionPatternFlag = false;
        yield return new WaitForSeconds(10f);
        if (!(_timeAfterCurrentNote > 0.060f))
        {
            yield return new WaitForSeconds(0.060f - _timeAfterCurrentNote);
        }
        _actionPatternAttacking = !_actionPatternAttacking;
        if (_actionPatternAttacking)
        {
            _firstAttackNoteFlag = true;
        }
        else {
            _dashRight = true;
        }
        _switchActionPatternFlag = true;
    }
}
