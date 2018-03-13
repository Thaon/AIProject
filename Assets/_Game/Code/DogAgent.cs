using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAgent : Agent {

    #region member variables

    public GameObject m_penGO;
    public GameObject m_herdGO;
    public float m_simulationRunningTime = 60f;
    public float m_deathPenalty;

    private Vector3 m_penPosition;
    private Vector3 m_herdCenterOfMass;
    private Vector3 m_previousCenterOfMass;
    private Rigidbody m_rb;
    private HerdGenerator m_herd;
    private float m_simulationCounter;
    private float m_speed = 5f;

    //agent reset stuff
    Vector3 m_startingPos;

    #endregion

    private void Start()
    {
        m_previousCenterOfMass = m_herdCenterOfMass;
        m_startingPos = transform.position;
        m_rb = GetComponent<Rigidbody>();
        m_herd = m_herdGO.GetComponent<HerdGenerator>();
        m_penPosition = m_penGO.transform.position;
        m_simulationCounter = 0;
    }

    public override List<float> CollectState()
    {

        List<float> state = new List<float>();

        //herd size, maybe to be added later
        m_herdCenterOfMass = m_herd.GetCenterOfHerd();

        //pen's direction
        state.Add(m_penPosition.x - transform.position.x);
        state.Add(m_penPosition.z - transform.position.z);

        //the dog's position
        state.Add(transform.position.x);
        state.Add(transform.position.z);

        //and velocity
        state.Add(m_rb.velocity.x);
        state.Add(m_rb.velocity.z);

        //distace from the center of the field, unnecessary
        //state.Add(m_herd.transform.position.x - transform.position.x);
        //state.Add(m_herd.transform.position.z - transform.position.z);

        //vector to the center of mass
        state.Add(m_herdCenterOfMass.x - transform.position.x);
        state.Add(m_herdCenterOfMass.z - transform.position.z);

        //velocity of the herd
        state.Add(m_herdCenterOfMass.x - m_previousCenterOfMass.x);
        state.Add(m_herdCenterOfMass.z - m_previousCenterOfMass.z);

        //radius of the herd, added as it will give precious additional info to the dog
        state.Add(m_herd.GetHerdRadius());

        //cache previous center of mass to calculate its velocity
        m_previousCenterOfMass = m_herdCenterOfMass;

        return state;
    }

    public override void InitializeAgent()
    {
    }

    public override void AgentOnDone()
    {
    }

    public override void AgentReset()
    {
        transform.position = m_startingPos;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        m_simulationCounter = 0;
        m_herd.GenerateHerd();
        reward = 0;
    }

    public override void AgentStep(float[] action)
    {
        if (brain.brainParameters.actionSpaceType == StateType.continuous)
        {
            transform.position += new Vector3(action[0], 0, action[1]).normalized / m_speed;

            //removing this reward will force the agent to look for another one (the sheep)
            //if (!done)
            //{
            //    reward += Time.deltaTime * 1/(Vector3.Distance(m_herd.GetCenterOfHerd(), m_penPosition) + 0.01f);
            //}
        }

        m_simulationCounter += Time.deltaTime;

        if (m_herd.GetHerdNumber() == 0 || m_simulationCounter >= m_simulationRunningTime)
        {
            done = true;
        }

        //dog's out of the field
        if (Vector3.Distance(m_herd.transform.position, transform.position) > m_herd.m_bounds)
        {
            done = true;
            reward = -m_deathPenalty;
        }
    }

    public void AddReward(float rew)
    {
        print(rew > 0 ? "Rewarded!" : "Bad!");
        reward += rew;
    }
}
