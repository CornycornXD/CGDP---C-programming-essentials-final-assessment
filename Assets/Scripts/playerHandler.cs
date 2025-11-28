using System;
using System.Collections;
using TMPro.SpriteAssetUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class playerHandler : MonoBehaviour
{
    //  Remaining shits to do for this script:
    /*  1. enemy hit detection --> damage processing - done
     *  2. reads dodge timing (using 2 child game objs) and MIDI sync --> scoring (game manager) - done
     *  3. 1 and 2 are linked for guaranteed hit if missed dodge timing - done
     *  4. state wrong direction of dodge even when input's timing is correct
     *  5. HP restore mechanism (game manager)
     *  6. Add boxes indicating accuracy thresholds for dodging (game manager) - this is inconsistent with the current design so will be skipped for now
    */

    Rigidbody2D _rb;
    gameplayManager _gameplayManager;
    Bounds _safeZoneBounds;
    Vector3 _mousePos;
    int _hp;
    float _playerSpeed, _evasionCooldown, _evasionDuration, _dodgeDistance;
    bool _invulnerability, _controlLock, _canEvade, _dodging, _enemyCollided, _hpRecoverFlag, _hpDecayFlag;
 
    public Vector3 MousePos { 
        get { return _mousePos; }
    }
    public float PlayerSpeed
    {
        get { return _playerSpeed; }
        set { _playerSpeed = value; }
    }

    public int Hp
    {
        get { return _hp; }
        set { _hp = value; }
    }

    public bool Invulnerability
    {
        get { return _invulnerability; }
        set { _invulnerability = value; }
    }

    public bool ControlLock {
        get { return _controlLock; }
        set { _controlLock = value; }
    }

    public bool EnemyCollided
    {
        get { return _enemyCollided; }
        set { _enemyCollided = value; }
    }
    private void Awake()
    {
        // rb
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        // safezone
        _safeZoneBounds = GameObject.Find("Safezone").GetComponent<BoxCollider2D>().bounds;

    }

    void Start()
    {
        // initialize variables
        _playerSpeed = 10f;
        _hp = 100;
        _invulnerability = false;
        _dodgeDistance = 2.5f;
        _evasionCooldown = 2.5f;
        _controlLock = false;
        _canEvade = true;
        _evasionDuration = 1.5f;
        _invulnerability = false;
        _dodging = false;
        _enemyCollided = false;
        _hpRecoverFlag = true;
        _hpDecayFlag = true;
    }
    void Update()
    {
        // Disable player controls when game is paused, failed, or completed - fix later
        // non-physics methods
        if (!_controlLock) {
            if ((Input.GetKeyDown("Q") && Input.GetKeyDown("E")))
            {
                Dodge("backward");
            }
            else if (Input.GetKeyDown("Q"))
            {
                Dodge("left");
            }
            else if (Input.GetKeyDown("E"))
            {
                Dodge("right");
            }
        }
        if (_canEvade == true) { 
            if (Input.GetKeyDown("space"))
            {
                Evade();
            }
        }

    }

    void FixedUpdate() {
        // physics methods
        if (!_dodging)
        {
            wasdMovement();
        }
        playerRotation();
        if (_safeZoneBounds.Contains(transform.position))
        {
            if (_hpRecoverFlag) {
                StartCoroutine(HpRecover());
            }
        }
        else
        {
            if (_hpDecayFlag) {
                StartCoroutine(HpDecay());
            }
        }
        if (_hp <= 0)
        {
            _gameplayManager.StageFailed();
        }
    }

    void wasdMovement() {
        // WASD or Arrow Keys
        Vector2 wasdMovement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        _rb.MovePosition(_rb.position + wasdMovement * _playerSpeed * Time.fixedDeltaTime);
    }

    void playerRotation() {
        _mousePos = Input.mousePosition;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
        Vector2 offset = new Vector2(_mousePos.x - screenPoint.x, _mousePos.y - screenPoint.y).normalized;
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f); 
    }

    void Dodge(string side) {
        // dodge logic
        _dodging = true;
        _mousePos = Input.mousePosition;
        Vector3 dodgeDirection = new Vector3(0f, 0f, 0f);
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.localPosition);
        Vector2 offset = new Vector2(_mousePos.x - screenPoint.x, _mousePos.y - screenPoint.y).normalized;
        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        if (side == "left") {
            // left dodge logic
            dodgeDirection = Quaternion.Euler(0f, 0f, angle - 90f) * Vector3.left;
        } else if (side == "right") {
            // right dodge logic
            dodgeDirection = Quaternion.Euler(0f, 0f, angle - 90f) * Vector3.right;

        } else if (side  == "backward") {
            // backward dodge logic
            dodgeDirection = Quaternion.Euler(0f, 0f, angle - 90f) * Vector3.down;
        }
        _rb.MovePosition(_rb.position + new Vector2(dodgeDirection.x, dodgeDirection.y) * _dodgeDistance); // can add some smooth transition later (optional)
        _dodging = false;
        // control lock
        if (_gameplayManager.NoteInputOffset <= 0.060f && _gameplayManager.NoteInputOffset >= -0.80f) {
            StartCoroutine(CoolDown((_gameplayManager.NoteStartTime + 0.060f) - _gameplayManager.Timer, "controlLockCooldown")); // this will need to be adjusted based on the adjustments made in gameplayManager
        }
        
    }

    void Evade() {
        // evade logic
        StartCoroutine(InvulnerabilityDuration(_evasionDuration));
        StartCoroutine(CoolDown(_evasionCooldown, "evasionCooldown"));
    }

    IEnumerator InvulnerabilityDuration(float time) {
        _invulnerability = true;
        yield return new WaitForSeconds(time);
        _invulnerability = false;
    }
    IEnumerator CoolDown(float time, string cooldownType)
    {
        if (cooldownType == "controlLockCooldown")
        {
            _controlLock = true;
            yield return new WaitForSeconds(time);
            _controlLock = false;
        }
        else if (cooldownType == "evasionCooldown")
        {
            _canEvade = false;
            yield return new WaitForSeconds(time);
            _canEvade = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) // fix later - you're only supposed to touch the hitbox then if >0.060f and you're still in the area you take damage --> user ontriggerenter2d instead
    {
        if (collision.gameObject.CompareTag("Enemy") && !_invulnerability && collision.gameObject.GetComponent<EnemyBehaviour>().Attacking == true)
        {
            // process damage
            _enemyCollided = true; // You can use this flag to check whether the player inputted dodge on time but wrong direction or not so add this feature later to game manager
            _hp -= 30;
        }
    }

    private IEnumerator OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            _enemyCollided = false;
        }
        yield return null;
    
    }

    IEnumerator HpRecover() {
        _hpRecoverFlag = false;
        if (_hp < 100) { 
            yield return new WaitForSeconds(0.5f);
            _hp += 1;
        }
        _hpRecoverFlag = true;
    }

    IEnumerator HpDecay() {
        _hpDecayFlag = false;
        if (_hp > 0) {
            yield return new WaitForSeconds(1f);
            _hp -= 10;
        }
        _hpDecayFlag = true;
    }
}