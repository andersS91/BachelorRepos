using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.AI;

public class Spawner : MonoBehaviour {
    //Public variables
    public int projectedActivity;
    public int popinThreshold;
    public float maxRange;
    public float minRange;
    public bool torusOfLife;
    public bool stressTest;
    public bool destroyOnContact;
    public GameObject[] actorPrefabs;
    public GameObject FPC;

    //Private variables
    private int actorsInPlay = 0;
    private int totalProb = 0;
    private int lastSpawn = 0;
    private int lastProb = 0;
    private List<Waypoint> spawns = new List<Waypoint>();
    private float temp;
    private float newProb;
    private int spawnProb;
    private bool initSpawn = true;
    private float bias = 0.4f;
    private float range = 1.8f;
    private int model = 0;

    void Start()
    {
        newProb = 0;
       // Time.timeScale = 2;
    }


    void FixedUpdate()
    {
        if (stressTest || (!destroyOnContact && projectedActivity > actorsInPlay))
        {
            Spawn();
            return;
        }
    }

    public bool TorusOfLife(GameObject actor)
    {
        if(FPC != null && Vector3.Distance(actor.transform.position, FPC.transform.position) > maxRange)
        {
            if (FPCNotInView(actor.transform.position))
            {
                Despawn(actor);
                return true;
            }
        }
        return false;
    }


    private List<Waypoint> WaypointsInRange()
    {
        List<Waypoint> inRange = new List<Waypoint>();
        Vector3 pos;
        for (int i = 0; i < spawns.Count; i++)
        {
            pos = spawns[i].transform.position;
            if (Vector3.Distance(pos, FPC.transform.position) > minRange && Vector3.Distance(pos, FPC.transform.position) < maxRange)
            {
                inRange.Add(spawns[i]);
            }
        }
        return inRange;
    }

    private void SpawnNonTorus()
    {
        if (lastProb > 0)
        {
            spawnProb = lastProb;
        }
        else
        {
            spawnProb = 0;
            for (int i = 0; i < spawns.Count; i++)
            {
                if (spawns[i].GetInstanceID() == lastSpawn)
                {
                    spawnProb = spawns[i].spawnProb;
                    break;
                }
            }
        }
        newProb = totalProb - spawnProb;
        temp = UnityEngine.Random.Range(0, newProb);
        for (int j = 0; j < spawns.Count; j++)
        {
            if (spawns[j].GetInstanceID() != lastSpawn)
            {
                temp -= spawns[j].spawnProb;
                if (temp < 0)
                {
                    lastSpawn = spawns[j].GetInstanceID();
                    lastProb = spawns[j].spawnProb;
                    Instantiate(actorPrefabs[(j % actorPrefabs.Length)], spawns[j].transform.position, spawns[j].transform.rotation).GetComponent<NavMeshAgent>().speed = bias + UnityEngine.Random.Range(0, 0.5f * bias);
                    actorsInPlay++;
                    break;
                }
            }
        }
    }

    private void SpawnTorus(List<Waypoint> inRange)
    {
        if (inRange == null || inRange.Count <= 0)
        {
            return;
        }
        newProb = 0;
        for (int i = 0; i < inRange.Count; i++)
        {
            newProb += inRange[i].spawnProb;
        }
        temp = UnityEngine.Random.Range(0, newProb);
        for (int j = 0; j < inRange.Count; j++)
        {
                temp -= inRange[j].spawnProb;
                if (temp <= 0 && FPCNotInView(inRange[j].transform.position))
                {
                    lastSpawn = inRange[j].GetInstanceID();
                    GameObject newActor = Instantiate(actorPrefabs[(model % actorPrefabs.Length)], inRange[j].transform.position, inRange[j].transform.rotation);
                    float rand = UnityEngine.Random.Range(0, range);
                    newActor.GetComponent<NavMeshAgent>().speed = bias + rand*bias*rand;
                    newActor.GetComponent<Animator>().speed = 1+rand*rand;    
                    actorsInPlay++;
                    model++;
                    model = model % actorPrefabs.Length;
                    break;
                }
        }

    }

    private bool FPCNotInView(Vector3 pos)
    {
        RaycastHit hit = new RaycastHit();
        Vector3 dir = FPC.transform.position - pos;
        for(int i = 0; i < popinThreshold; i++)
        {
            if(Physics.Raycast(pos, dir, out hit, Vector3.Distance(pos, FPC.transform.position))){
                if(hit.collider.gameObject.name.Equals(FPC.name))
                {
                    return false;
                }
                Wait(0.1f);
            }
        }
        return true;
    }

    private void Spawn()
    {
        if (torusOfLife) {
            SpawnTorus(WaypointsInRange());
        }
        else
        {
            SpawnNonTorus();
        }
    }

    public void Despawn(GameObject actor)
    {
        Destroy(actor);
        actorsInPlay--;
    }

    IEnumerable Wait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
    }

    public void AddSpawn(Waypoint spawn)
    {
        spawns.Add(spawn);
        totalProb += spawn.spawnProb;
    }
}
