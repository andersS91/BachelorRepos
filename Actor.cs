using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor : MonoBehaviour {
    private int travel = 0;
    private NavMeshAgent agent;
    private Vector3 destination;
    private bool calculated = false;
    private bool grown;

    // Use this for initialization
    void Awake () {
        agent = GetComponent<NavMeshAgent>();
        travel = -1;
    }
    
    private void Update()
    {
        agent.SetDestination(destination);
    }
    
    public int GetTravel()
    {
        return travel;
    }

    public void SetTravel(int travel)
    {
        this.travel = travel;
    }

    public bool GetCalculated()
    {
        return calculated;
    }

    public void SetCalculated(bool calculated) {
        this.calculated = calculated;
    }

    public void SetDestination(int id, Vector3 destination)
    {
        SetTravel(id);
        this.destination = destination;
    }
    
    public Vector3 GetDestination()
    {
        return agent.destination;
    }
}
