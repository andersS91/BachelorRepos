using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    //Connections
    public Waypoint[] waypoints;
    public float[] flux;
    public bool spawn = false;
    private float[] fill;
    public int spawnProb;
    public GameObject spawnObject;
    private Spawner spawner;

    //ChooseEdge
    float total = 0;
    int newDirId = 0;
    float temp = 100000;
    bool filled = true;

    // Use this for initialization
    void Awake()
    {
        spawner = spawnObject.GetComponent<Spawner>();
        if (spawn)
        {
            spawner.AddSpawn(gameObject.GetComponent<Waypoint>());
        }
        fill = new float[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            fill[i] = 0;
        }
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (flux[i] < 0)
            {
                flux[i] = 1 / (0 - flux[i]);
            }
        }
    }

    private void Start()
    {
    }

    public int EnterEdge(int id)
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].GetInstanceID() == id)
            {
                fill[i]++;
                return i;
            }
        }
        return 0;
    }

    public void DecEdge(int id)
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].GetInstanceID() == id)
            {
                if (fill[i] > 0)
                {
                    fill[i]--;
                }
                break;
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other == null)
        {
            return;
        }

        if (other.GetComponent<Actor>() != null)
        {
            Actor actor = other.GetComponent<Actor>();
            if (actor.GetTravel() == this.GetInstanceID())
            {
                return;
            }
            int id = actor.GetTravel();
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (id == waypoints[i].GetInstanceID())
                {
                    if (spawn)
                    {
                        spawner.Despawn(other.gameObject);
                    }
                    else if(spawner.TorusOfLife(other.gameObject))
                    {
                        return;
                    }
                    
                }
            }
            RouteCal(other, actor);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other == null)
        {
            return;
        }
        return;
    }

    private void RouteCal(Collider other, Actor actor)
    {
        if (other != null)
        {
            int id = actor.GetTravel();
            LeaveEdge(id);
            actor.SetDestination(this.GetInstanceID(), waypoints[EnterEdge(ChooseEdge(id))].transform.position);
        }
        return;
    }

    private int ChooseEdge(int dirId)
    {
        total = 0;
        float[] P = new float[waypoints.Length];
        newDirId = 0;
        temp = 100000;
        filled = true;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (dirId != waypoints[i].GetInstanceID())
            {
                if (flux[i] - fill[i] > 0)
                {
                    P[i] = flux[i] - fill[i];
                    total += P[i];
                    filled = false;
                }
            }
            else
            {
                P[i] = 0;
            }
        }


        if (filled)
        {
            int chosen = -1;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (flux[i] > 0 && dirId != waypoints[i].GetInstanceID() && (((float)fill[i]) / flux[i] < temp))
                {
                    chosen = i;
                    newDirId = waypoints[i].GetInstanceID();
                    temp = fill[i] / flux[i];
                }
            }
            if (chosen > -1)
            {
                fill[chosen]++;
            }

        }
        else
        {
            temp = Random.Range(0, total);
            for (int i = 0; i < waypoints.Length; i++)
            {
                temp -= P[i];
                if (temp < 0)
                {
                    fill[i]++;
                    newDirId = waypoints[i].GetInstanceID();
                    break;
                }
            }
        }

        return newDirId;
    }

    private void LeaveEdge(int id)
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i].GetInstanceID() == id)
            {
                waypoints[i].DecEdge(this.GetInstanceID());
                break;
            }
        }
    }



    public int GetId()
    {
        return this.GetInstanceID();
    }

}
