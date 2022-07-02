using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public GameObject startPoint;
    public GameObject targetPoint;
    public List<GameObject> vfx = new List<GameObject>();

    private GameObject effectToSpawn;
    public bool targetReached;

    public List<GameObject> activeEffects = new List<GameObject>();
    public string explosionType;

    public Color orange = new Color(1f, 0.5f, 0f, 1f);

    public GameObject exploRef;
    public GameObject healingRef;

    public Vector3 location;

    public static SFXManager inst;
    public void Awake()
    {
        inst = this;
    }

    /// <summary>
    /// This manages *special effects* that need to be created at certain locations.
    /// </summary>

    // Start is called before the first frame update
    void Start()
    {
        effectToSpawn = vfx[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(location != null)
        {
            if (targetReached)
            {
                switch (explosionType)
                {
                    case "": // Nothing

                        break;

                    case "icon_fireball": // Explosion
                        GameObject newExplosion = Instantiate(exploRef, location, Quaternion.identity);
                        targetReached = false;

                        StartCoroutine(DeleteEffect(newExplosion));
                        break;

                    case "icon_secondwind":
                        GameObject newHeal = Instantiate(healingRef, location, Quaternion.identity);
                        targetReached = false;

                        StartCoroutine(DeleteEffect(newHeal));

                        break;

                    case "icon_curewounds":
                        GameObject newHeal1 = Instantiate(healingRef, location, Quaternion.identity);
                        targetReached = false;

                        StartCoroutine(DeleteEffect(newHeal1));

                        break;

                    case "icon_healingword":
                        GameObject newHeal2 = Instantiate(healingRef, location, Quaternion.identity);
                        targetReached = false;

                        StartCoroutine(DeleteEffect(newHeal2));

                        break;

                    default:

                        break;
                }
            }
        }

    }

    public void CreateEffectHere(string icon, Vector3 location)
    {
        float mtth = 1.5f;

        switch (icon)
        {
            case "": // Nothing

                break;

            case "icon_fireball": // Explosion
                GameObject newExplosion = Instantiate(exploRef, location, Quaternion.identity);

                StartCoroutine(DeleteEffectWhen(newExplosion, mtth));
                break;

            case "icon_secondwind":
                GameObject newHeal = Instantiate(healingRef, location, Quaternion.identity);

                StartCoroutine(DeleteEffectWhen(newHeal, mtth));

                break;

            case "icon_curewounds":
                GameObject newHeal1 = Instantiate(healingRef, location, Quaternion.identity);

                StartCoroutine(DeleteEffectWhen(newHeal1, mtth));

                break;

            case "icon_healingword":
                GameObject newHeal2 = Instantiate(healingRef, location, Quaternion.identity);

                StartCoroutine(DeleteEffectWhen(newHeal2, mtth));

                break;

            default:

                break;
        }
    }

    public void SetEffectToSpawn(int num)
    {
        effectToSpawn = vfx[num];
    }

    public void SpawnVFX(bool missTarget, string desiredTarget, string exploType = "", string desiredColor = "")
    {
        GameObject vfx;

        targetReached = false;

        if (startPoint != null && targetPoint != null)
        {
            vfx = Instantiate(effectToSpawn, startPoint.transform.position, Quaternion.identity);
            vfx.GetComponent<ProjectileMove>().targetPoint = targetPoint;
            vfx.GetComponent<ProjectileMove>().desiredTarget = desiredTarget;

            var rend = vfx.GetComponentInChildren<MeshRenderer>();
            var trail = vfx.GetComponentInChildren<TrailRenderer>();
            switch (desiredColor)
            {
                case "":

                    break;

                case "red":
                    rend.material.SetColor("_Color", orange);
                    trail.material.SetColor("_Color", orange);
                    break;

                case "cyan":
                    rend.material.SetColor("_Color", Color.cyan);
                    trail.material.SetColor("_Color", Color.cyan);
                    break;

                case "blue":
                    rend.material.SetColor("_Color", Color.blue);
                    trail.material.SetColor("_Color", Color.blue);
                    break;

                case "magenta":
                    rend.material.SetColor("_Color", Color.magenta);
                    trail.material.SetColor("_Color", Color.magenta);
                    break;

                default:

                    break;
            }

            switch (exploType)
            {
                case "icon_fireball":
                    vfx.GetComponent<ProjectileMove>().type = 1;
                    break;

                default:
                    vfx.GetComponent<ProjectileMove>().type = 0;
                    break;
            }
            explosionType = exploType;

            activeEffects.Add(vfx);

            //activeEffects[activeEffects.Length] = vfx;

            if (missTarget)
            {
                vfx.GetComponent<ProjectileMove>().miss = true;
            }
        }
        else
        {
            Debug.LogError("Failure in spawning VFX. A start and/or target point is null.");
        }
    }

    IEnumerator DeleteEffect(GameObject toDelete)
    {
        yield return new WaitForSeconds(10f);

        Destroy(toDelete);
    }

    IEnumerator DeleteEffectWhen(GameObject toDelete, float mtth)
    {
        yield return new WaitForSeconds(mtth);

        Destroy(toDelete);
    }
}
