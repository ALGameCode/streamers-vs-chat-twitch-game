using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobController : EntityStatus
{
    private Vector3 targetPlayer;
    private GameObject player;
    private float distanceX;
    private float distanceY;
    private SpriteRenderer mobRenderer;

    private bool canMove = true;

    [Tooltip("Estado atual do mob")] 
    public EnemyStatus enemyStatus = EnemyStatus.follow;

    public string summonerName; // Chat User NickName
    public int DamageDone {get; set;}

    void Start()
    {
        InitStatus();
        GetPlayerPosition();
    }

    void Update()
    {
        FollowPlayerBehavior();
        if(life <= 0)
        {
            DeadBehavior();
        }

        if(GameManager.instance.OnEndGame)
        {
            EndGameStatus();
        }
    }

    private void GetPlayerPosition()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mobRenderer = GetComponent<SpriteRenderer>();
    }

    private void FollowPlayerBehavior()
    {
        if((player == null) || (!canMove)) return;
        float direction = player.transform.position.x - this.transform.position.x;
        if(direction >= 0) mobRenderer.flipX = true;
        else mobRenderer.flipX = false;
        //Debug.Log($"Direcao: {}");
        targetPlayer = player.transform.position;
        this.transform.position = Vector3.MoveTowards(transform.position, targetPlayer, speed * Time.deltaTime);
    }

    private void AttackBehavior()
    {
        
    }

    private void HitBehavior()
    {
        
    }

    private void DeadBehavior()
    {
        canMove = false;
        if (!string.IsNullOrEmpty(summonerName) && ChatStatus.instance.chatUsers.ContainsKey(summonerName))
        {
            ChatStatus.instance.chatUsers[summonerName].DamageDone += DamageDone;
        }
        // Animacao de morte e esperar um tempo
        Destroy(this.gameObject);
    }

    private void EndGameStatus()
    {
        canMove = false;
        if (!string.IsNullOrEmpty(summonerName) && ChatStatus.instance.chatUsers.ContainsKey(summonerName))
        {
            ChatStatus.instance.chatUsers[summonerName].DamageDone += DamageDone;
        }
        Destroy(this);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if(coll.tag.Equals("PlayerSkill"))
        {
            DecreaseLife(coll.transform.parent.gameObject.GetComponent<PlayerSkills>().damage);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if(coll.gameObject.tag.Equals("Player"))
        {
            canMove = false;

        }
        else if (coll.gameObject.tag.Equals("Mob"))
        {
            //Debug.Log($"Collision Mob");
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        if(coll.gameObject.tag.Equals("Player"))
        {
            StartCoroutine(WaitForSeconds());
        }
        else if (coll.gameObject.tag.Equals("Mob"))
        {
            //Debug.Log($"Collision Mob");
        }
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSecondsRealtime(3);
        canMove = true;
    }


}

public enum EnemyStatus {follow, attack, hit, dead}
