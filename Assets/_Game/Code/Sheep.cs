using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour {

    #region member variables

    public HerdGenerator m_herd;

    [Range(0, 1)]
    public float m_variation;

    public float m_neighboursCheckRadius;
    public float m_cohesionFactor;
    public float m_alignmentFactor;
    public float m_separationFactor;

    public float m_maxDistance;
    public float m_minDistance;

    public GameObject m_fleeFrom;
    public float m_maxVel;
    public float m_scareDistance;

    private Rigidbody m_rb;
    private Sheep[] m_neighbours;
    private bool m_dead = false;

    #endregion

    void Start ()
    {
        m_rb = GetComponent<Rigidbody>();
    }
	
	void FixedUpdate ()
    {
        m_neighbours = GetNeighbours();
        //make sure we are in a 2D plane and then cap the speed
        Vector3 nextDir = Vector3.zero;

        //make sure there are sheep around
        if (m_herd.GetSheep().Length > 0)
             nextDir = CalculateNextDirection();

        Vector3 currentVel = m_rb.velocity;
        nextDir.y = 0;
        currentVel.y = 0;

        m_rb.velocity = currentVel + (nextDir * m_maxVel * Time.deltaTime);

        if (m_rb.velocity.magnitude > m_maxVel)
        {
            m_rb.velocity = m_rb.velocity.normalized * m_maxVel;
        }

        if (m_rb.velocity.magnitude > 0)
            transform.rotation = Quaternion.LookRotation(m_rb.velocity);

        if (CheckIfDead())
        {
            m_herd.RemoveSheep(this.gameObject);
            m_herd.AddPoints(-1f);
            Destroy(this.gameObject);
        }

        if (CheckIfInPen())
        {
            m_herd.RemoveSheep(this.gameObject);
            m_herd.AddPoints(1f);
            Destroy(this.gameObject);
        }
	}

    public void RandomiseSheep()
    {
        float ran = 1 - Random.Range(0, m_variation);
        m_neighboursCheckRadius *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_cohesionFactor *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_alignmentFactor *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_separationFactor *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_maxDistance *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_minDistance *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_maxVel *= ran;
        ran = 1 - Random.Range(0, m_variation);
        m_scareDistance *= ran;
    }

    private Vector3 CalculateNextDirection()
    {
        return (Flee(m_fleeFrom) + (CalculateAlignmentVector() * m_alignmentFactor) + (GetDirectionToCenterOfMass() * m_cohesionFactor) + (GetSeparationDirection() * m_separationFactor)).normalized;
    }

    private Vector3 CalculateAlignmentVector()
    {
        Vector3 dir = Vector3.zero;

        List<Sheep> neighboursPlusThis = new List<Sheep>(m_neighbours);
        neighboursPlusThis.Add(this);
        m_neighbours = neighboursPlusThis.ToArray();

        foreach (Sheep neighbour in m_neighbours)
        {
            //check if they are moving
            if (neighbour.GetVelocity().magnitude > 0.1f)
            {
                dir += neighbour.GetComponent<Sheep>().GetVelocity();
            }
        }

        dir.y = 0;
        dir = m_neighbours.Length > 0 ? (dir / m_neighbours.Length).normalized : Vector3.zero;

        Vector3 desired = dir * m_maxVel;
        Vector3 steer = desired - m_rb.velocity;

        return (steer).normalized;
    }

    private Vector3 GetDirectionToCenterOfMass()
    {
        Vector3 pos = Vector3.zero;

        foreach (Sheep neighbour in m_neighbours)
        {
            //we only calculate neighbours that are too far
            if (Vector3.Distance(neighbour.transform.position, transform.position) > m_maxDistance)
            {
                pos += neighbour.transform.position;
            }
        }

        pos.y = 0;
        pos = m_neighbours.Length > 0 ? pos / m_neighbours.Length : Vector3.zero;

        return (pos - transform.position).normalized;
    }

    private Vector3 GetSeparationDirection()
    {
        Vector3 dis = Vector3.zero;

        foreach (Sheep neighbour in m_neighbours)
        {
            //we only calculate neighbours that are too close
            if (Vector3.Distance(neighbour.transform.position, transform.position) < m_minDistance)
            {
                //scale based on distance within radius
                float mul = 1 - (Vector3.Distance(neighbour.transform.position, transform.position) / m_neighboursCheckRadius);
                Vector3 scaledDis = (neighbour.transform.position - transform.position) * mul;
                dis += scaledDis;
            }
        }

        dis.y = 0;
        dis = m_neighbours.Length > 0 ? dis / m_neighbours.Length : Vector3.zero;

        return -dis;
    }

    private Vector3 Flee(GameObject target)
    {
        if (target == null && Vector3.Distance(transform.position, target.transform.position) > m_scareDistance)
            return Vector3.zero;

        Vector3 desiredVel = (transform.position - target.transform.position).normalized * m_maxVel;
        //calculate direction and scale it according to the distance
        return (desiredVel - m_rb.velocity) * (1 - (Vector3.Distance(transform.position, target.transform.position) / m_scareDistance));
    }

    public Vector3 GetVelocity()
    {
        //we only want to move in a 2D plane
        if (m_rb == null)
            m_rb = GetComponent<Rigidbody>();

        return new Vector3(m_rb.velocity.normalized.x, 0, m_rb.velocity.normalized.z);
    }

    private Sheep[] GetNeighbours()
    {
        List<Sheep> sheepInRadius = new List<Sheep>();
        foreach(Sheep shp in m_herd.GetSheep())
        {
            if (shp != this && Vector3.Distance(transform.position, shp.transform.position) < m_neighboursCheckRadius)
                sheepInRadius.Add(shp);
        }
        return sheepInRadius.ToArray();
    }

    private bool CheckIfDead()
    {
        if (Vector3.Distance(m_herd.transform.position, transform.position) > m_herd.m_bounds)
            return true;
        else
            return false;
    }

    private bool CheckIfInPen()
    {
        if (Vector3.Distance(m_herd.m_penGO.transform.position, transform.position) < 10) //10 is half the pen's diameter
            return true;
        else
            return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_neighboursCheckRadius);
    }
}
