using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFighter : MonoBehaviour
{
    public EnemyHealth health;

    private Vector3 speed;
    
    Camera cam;
    CameraBounds camBounds;
    Bounds bound;
    
    private float nextFire;
    public float fireRate;

    private float timeSinceSpawn, timeAtSpawn;

    void Start()
    {
        health = GetComponent<EnemyHealth>();
        health.setHealth(1, 1);

        speed = new Vector3(0, 3f, 0);
        speed = speed * Time.fixedDeltaTime;

        cam = Camera.main;
        camBounds = cam.GetComponent<CameraBounds>();
        bound = camBounds.bounds;

        fireRate = 150.0f;
        nextFire = 0f;

        timeSinceSpawn = 0;
        timeAtSpawn = Time.time;

    }

    void FixedUpdate()
    {
        transform.Translate(speed);
        shoot();
        if(transform.position.y < bound.min.y - 10 || transform.position.x < bound.min.x - 10)
        {
            Destroy(gameObject);
        }
        timeSinceSpawn = Time.time - timeAtSpawn;
    }
    
    //Tells Fighter to shoot projectiles.
    void shoot()
    {
        if (Time.time > nextFire)
        {
            //Debug.Log("Firing at " + Time.time);
            nextFire = Time.time + fireRate;
            Instantiate(Resources.Load("Prefabs/Bullet"), transform.position, transform.rotation);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;

        if (other.CompareTag("HeroProjectile"))
        {
            // As a HeroProjectile, other must have a ProjectileBehavior script attached.
            ProjectileBehavior damageDealer = other.GetComponent<ProjectileBehavior>();
            PlayerBehavior p = damageDealer.GetParent();
            p.DestroyedEnemy();
            Debug.Log(damageDealer.GetParent());
            health.decreaseHealth(damageDealer.GetParent());
        }
    }

}
