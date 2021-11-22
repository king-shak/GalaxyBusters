using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    int health;
    float[] damageDealt;
    
    GameObject Explosion;
    GameObject powerUp;

    PlayerBehavior destroyerBehavior;

    Camera cam;
    ScoreManager score;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        score = cam.GetComponent<ScoreManager>();
        damageDealt = new float[2];
        damageDealt[0] = 0;
        damageDealt[1] = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(health == 0)
        {
            Destroy(gameObject);
            Explosion = Instantiate(Resources.Load("Prefabs/Explosion"), transform.position, transform.rotation) as GameObject;
            Destroy(Explosion.gameObject, 1);
            
            //20% to spawn powerup
            int chance = Random.Range(1, 6);
            if(chance == 1)
            {
                powerUp = Resources.Load<GameObject>("Prefabs/PowerUp") as GameObject;
                powerUp.transform.position = transform.position;
                powerUp.transform.rotation = transform.rotation;
            }

            if(destroyerBehavior.IsPlayerOne())
            {
                score.DestroyedEnemy(damageDealt, 0);
            }
            else
            {
                score.DestroyedEnemy(damageDealt, 1);
            }
            
        }
    }

    public int GetHealth()
    {
        return health;
    }

    public void setHealth(int h, int lvl)
    {
        health = h * lvl;
    }

    public void decreaseHealth(PlayerBehavior damageDealer)
    {
        if(damageDealer.IsPlayerOne())
        {
            Debug.Log("Player 1 did damage.");
            damageDealt[0]++;
            destroyerBehavior = damageDealer;
            health--;
        }
        else
        {
            damageDealt[1]++;
            destroyerBehavior = damageDealer;
            health--;
        }
        Debug.Log(damageDealer.IsPlayerOne());
    }
}
