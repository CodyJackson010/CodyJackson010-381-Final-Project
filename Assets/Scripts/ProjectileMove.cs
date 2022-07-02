using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMove : MonoBehaviour
{
    public float speed;
    public float fireRate;
    public GameObject targetPoint;
    public string desiredTarget;
    public bool miss = false; // Should this projectile miss?
    public Transform lastTarget;
    public int type;

    public GameObject bigExploRef;

    public TrailRenderer trail;

    // Start is called before the first frame update
    void Start()
    {
        lastTarget = targetPoint.transform;
        StartCoroutine(DestroyTimer());
        if (miss)
        {
            this.transform.LookAt(targetPoint.transform); // Try not to miss target Impossible%
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(speed != 0)
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
            if (!miss)
            {
                if(targetPoint != null)
                {
                    this.transform.LookAt(targetPoint.transform);
                    float distance = Vector3.Distance(this.transform.position, lastTarget.transform.position);
                    if (distance <= 1f)
                    {
                        CombatManager.inst.targetReached = true;
                        SFXManager.inst.targetReached = true;
                        SFXManager.inst.location = this.transform.position;

                        speed = 0;
                        Destroy(gameObject);
                    }
                }
                else
                {
                    if(lastTarget != null)
                    {
                        
                    }
                }
            }
        }
        else
        {

        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == desiredTarget)
        {
            speed = 0;
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyTimer() // Destroy self automatically after X seconds
    {
        yield return new WaitForSeconds(5);

        speed = 0;
        Destroy(gameObject);
    }
}
