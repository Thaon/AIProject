using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerdGenerator : MonoBehaviour {

    #region member variables

    public bool m_useRandomSeed = true;
    public GameObject m_prefab;
    public GameObject m_threat;
    public GameObject m_penGO;
    public int m_sheepNumber;
    [Range(0,1)]
    public float m_variationRange;
    public float m_bounds;

    private float m_points;
    private List<GameObject> m_mySheep;

    #endregion

    void Start ()
    {
	}

    public void AddPoints(float points)
    {
        m_points += points;
        m_threat.GetComponent<DogAgent>().AddReward(points);
    }

    public void GenerateHerd()
    {
        GameObject level = GameObject.FindWithTag("LevelArea");
        m_bounds = level.GetComponent<SphereCollider>().radius * level.transform.localScale.x;

        if (m_useRandomSeed)
            Random.InitState(42);

        m_points = 0;

        //remove sheep from previous iteration if any
        if (m_mySheep != null)
        {
            foreach (GameObject shp in m_mySheep)
                DestroyImmediate(shp);

            m_mySheep.Clear();
        }


        m_mySheep = new List<GameObject>();

        for (int i = 0; i < m_sheepNumber; i++)
        {
            Vector2 ranPos = Random.insideUnitCircle;
            Vector3 ranRot = Random.rotation.eulerAngles;
            ranRot.x = 0;
            ranRot.z = 0;

            GameObject go = Instantiate(m_prefab, transform.position + new Vector3(ranPos.x, 0, ranPos.y) * m_bounds, Quaternion.Euler(ranRot), this.transform);
            go.GetComponent<Sheep>().m_variation = Random.Range(0, m_variationRange);
            go.GetComponent<Sheep>().RandomiseSheep();
            go.GetComponent<Sheep>().m_fleeFrom = m_threat;
            go.GetComponent<Sheep>().m_herd = this;
            m_mySheep.Add(go);
        }
    }

    public float GetHerdRadius()
    {
        float maxDis = 0;

        foreach (GameObject shp in m_mySheep)
        {
            float dis = Vector3.Distance(GetCenterOfHerd(), shp.transform.position);
            if (dis > maxDis)
                maxDis = dis;
        }

        return maxDis;
    }

    public int GetHerdNumber()
    {
        return m_mySheep.Count;
    }

    public Vector3 GetCenterOfHerd()
    {
        Vector3 pos = Vector3.zero;

        foreach (Sheep neighbour in GetSheep())
        {
            pos += neighbour.transform.position;
        }

        pos.y = 0;
        pos = m_mySheep.Count > 0 ? pos / m_mySheep.Count : Vector3.zero;

        return pos;
    }

    public Sheep[] GetSheep()
    {
        List<Sheep> sheep = new List<Sheep>();

        //clean m_mySheep
        List<int> defectiveIndeces = new List<int>();

        if (m_mySheep == null)
            return new Sheep[0];

        for (int i = 0; i < m_mySheep.Count; i++)
        {
            if (m_mySheep[i] == null)
                defectiveIndeces.Add(i);
        }

        //grab the actual sheep
        foreach (int i in defectiveIndeces)
            m_mySheep.RemoveAt(i);

        foreach (GameObject go in m_mySheep)
        {
            if (go != null)
                sheep.Add(go.GetComponent<Sheep>());
        }
        return sheep.ToArray();
    }

    public void RemoveSheep(GameObject shp)
    {
        m_mySheep.Remove(shp);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GetCenterOfHerd(), 2);
    }
}
