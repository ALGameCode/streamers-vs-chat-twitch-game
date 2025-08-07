using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    private Vector3 moveVec;
    private Player playerAction;
    private Rigidbody2D rigidbody;
    private Vector2 moveInput;

    private SpriteRenderer spriteRenderer;

    private bool canTakeDamage = true;

    void Awake()
    {
        InitPlayer();
    }

    void FixedUpdate()
    {
        PlayerMove();
    }

    private void OnEnable()
    {
        playerAction.PlayerMove.Enable();
    }

    private void OnDisable()
    {
        playerAction.PlayerMove.Disable();
    }

    private void InitPlayer()
    {
        playerAction = new Player();
        rigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if(spriteRenderer == null)
        {
            Debug.LogWarning("There are no a sprite renderer");
        }
    }

    private void PlayerDie()
    {
        //Animacao
        // ...
        GameManager.instance.EndGame(GameWinner.Chat);
    }


    private void PlayerMove()
    {
        moveInput = playerAction.PlayerMove.Move.ReadValue<Vector2>();
        transform.Translate(moveInput.x * (PlayerStatus.instance.speed * Time.deltaTime), moveInput.y * (PlayerStatus.instance.speed * Time.deltaTime), 0f);

        if(spriteRenderer != null)
        {
            if(moveInput.x > 0f)
            {
                spriteRenderer.flipX = true;
            }
            else if(moveInput.x < 0f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        if (coll.gameObject.tag.Equals("Mob"))
        {
            if(canTakeDamage)
            {
                StartCoroutine(WaitForSeconds());
                int damage = coll.gameObject.GetComponent<MobController>().damage;
                PlayerStatus.instance.DecreaseLife(damage);
                coll.gameObject.GetComponent<MobController>().DamageDone += damage;

                if(PlayerStatus.instance.life <= 0)
                {
                    ChatStatus.instance.userLastHit = coll.gameObject.GetComponent<MobController>().summonerName;
                    PlayerDie();
                }
            }
            
        }
    }

    IEnumerator WaitForSeconds()
    {
        canTakeDamage = false;
        yield return new WaitForSecondsRealtime(1);
        canTakeDamage = true;
    }

}
