using System;
using UnityEngine;

public class Map_procedural : MonoBehaviour
{
    public GameObject subject;
    public GameObject cam;
    public GameObject EmptyPlane;
    public GameObject Tree;
    public GameObject SlopeObject;
    public int threshold;
    public float perlinScale;
    private Transform player;

    private GameObject[] planes;
    private Vector3[] beginPoints;
    private Vector3 Totaldecal = Vector3.zero;

    public int xSize, zSize;
    private Vector3[] vertices;
    private Mesh mesh;
    private int length, width,hLimit,vLimit, longitude, latitude;
    private float seed;

    public float slope = -0.1f;
    public int VerticeDistance = 2;

    void Awake()
    {
        length = zSize * VerticeDistance;
        width = xSize * VerticeDistance;
        vLimit = length;
        hLimit = width*2;
        //Vertical
        latitude = 0;
        //Horizontal
        //Horizontal
        longitude = 0;

        createTreadmill(10);
        starGenerate();
        subject = spawnObject(subject);
        cam = spawnObject(cam);

        seed = UnityEngine.Random.Range(0, 1);
    }

    private void FixedUpdate()
    {
        updatePlanes();
    }

    private void Update()
    {
        FloatingOrigin();
    }

    private void createTreadmill(int n)
    {
        planes = new GameObject[n];
        beginPoints = new Vector3[n];

        for (int i = 0; i < n; i++)
        {
            planes[i] = Instantiate(EmptyPlane , transform);
            beginPoints[i] = Vector3.zero;
        }
    }

    private void starGenerate()
    {
        for (int i = 0, z = 0; i < transform.childCount; z += length)
        {
            for(int x = -hLimit; x<= hLimit; x += width, i++)
            {
                beginPoints[i] = new Vector3(x, z / VerticeDistance * slope, z);
                planes[i].transform.position = beginPoints[i];
                generatePlane(planes[i], beginPoints[i]);
                distributeObject(planes[i], Tree, beginPoints[i], 50);
                //distributeObject(planes[i], SlopeObject, beginPoints[i], 5);
            }
        }
    }

    private void updatePlanes()
    {
        Vector3 position = subject.transform.position;

        Boolean change = false;

        if (longitude - position.x < -width)
        {
            longitude += width;
            change = true;
        }
        else if (longitude - position.x > width)
        {
            longitude -= width;
            change = true;
        }
        else if (latitude - position.z < -length-5)
        {
            latitude += length;
            change = true;
        }

        if (change)
        {

            for (int i = 0; i < transform.childCount; i++)
            {

                if (longitude - beginPoints[i].x < -hLimit)
                {
                    beginPoints[i].x -= hLimit * 2.5f;

                    planes[i].transform.position = beginPoints[i];
                    generatePlane(planes[i], beginPoints[i]);
                    redistributeObject(planes[i], beginPoints[i]);
                }
                else if (longitude - beginPoints[i].x > hLimit)
                {
                    beginPoints[i].x += hLimit * 2.5f;

                    planes[i].transform.position = beginPoints[i];
                    generatePlane(planes[i], beginPoints[i]);
                    redistributeObject(planes[i], beginPoints[i]);
                }
                else if (beginPoints[i].z - latitude <= -vLimit)
                {
                    beginPoints[i].z += vLimit*2;
                    beginPoints[i].y = (beginPoints[i].z) / VerticeDistance * slope;

                    planes[i].transform.position = beginPoints[i];
                    generatePlane(planes[i], beginPoints[i]);
                    redistributeObject(planes[i], beginPoints[i]);
                }
            }

        }
    }

    private void generatePlane(GameObject plane, Vector3 beginPoint)
    {
        plane.GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Grid";

        float hight = 0;
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0, z = 0; z <= length; z+=VerticeDistance)
        {
            for (int x = (int)(-width/2); x <= (int)(width / 2); x+=VerticeDistance, i++)
            {
                vertices[i] = new Vector3(x,hight-customPerlin(x +beginPoint.x, z + beginPoint.z), z);
                uv[i] = new Vector2((float)x / xSize, (float)z / zSize);
            }
            hight += slope;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        int[] triangles = new int[xSize * zSize * 6];
        for (int ti = 0, vi = 0, y = 0; y < zSize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        plane.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    #region object

    private void distributeObject(GameObject plane, GameObject o, Vector3 beginPoint,int number)
    {
        PoissonDiscSampler sampler = new PoissonDiscSampler(width, length, 15);
        int i = 0;

        Vector3 origin = new Vector3(beginPoint.x - width / 2, beginPoint.y, beginPoint.z);

        foreach (Vector2 sample in sampler.Samples())
        {
            Instantiate(o, new Vector3(origin.x+sample.x, getYofPlane(origin.x + sample.x, origin.z + sample.y), origin.z+sample.y), randomYRotation(), plane.transform);

            i++;
            if (i == number) break;
        }
    }

    private void redistributeObject(GameObject plane, Vector3 beginPoint)
    {
        PoissonDiscSampler sampler = new PoissonDiscSampler(width, length, 15);
        int i = 0;

        Vector3 origin = new Vector3(beginPoint.x - width / 2, beginPoint.y, beginPoint.z);

        foreach (Vector2 sample in sampler.Samples())
        {
            Transform child = plane.transform.GetChild(i);

            child.position = new Vector3(origin.x + sample.x, getYofPlane(origin.x + sample.x, origin.z + sample.y), origin.z + sample.y);
            child.rotation = randomYRotation();

            i++;
            if (i >= plane.transform.childCount ) break;
        }
    }

    #endregion

    private float customPerlin(float x, float y)
    {

        return Mathf.PerlinNoise((x + Totaldecal.x + seed + hLimit*1.5f) * 0.006f, (y + Totaldecal.z + seed) * 0.006f) * perlinScale;
    }

    #region FloatingOrigin

    private void FloatingOrigin()
    {
        Vector3 decal = new Vector3();

        if (longitude > threshold * width)
        {
            decal.x = -threshold * width;

        }
        else if (longitude < -threshold * width)
        {
            decal.x = +threshold * width;
        }
        if (latitude > threshold * length)
        {
            decal.z = -threshold * length;
            decal.y = decal.z / VerticeDistance * slope;
        }

        if (decal.x != 0 || decal.y != 0 || decal.z != 0) resetEntities(decal);

        Totaldecal -= decal; 

    }

    private void resetEntities(Vector3 decal)
    {

        Vector3 pos = subject.GetComponent<Ski>().resetEntities(decal);

        latitude = ((int)decal.z / length) * length;
        longitude = ((int)decal.x / width) * width;

        for (int i = 0; i < transform.childCount; i++)
        {

            planes[i].transform.position += decal;
            beginPoints[i] = planes[i].transform.position;

        }
    }

    #endregion

    private float getYofPlane(float x, float z)
    {
        RaycastHit hit;

        Physics.Raycast(new Vector3(x, 0, z), Vector3.down,out hit);

        return hit.point.y;
    }

    private Quaternion randomYRotation()
    {
        return Quaternion.Euler(new Vector3(0, UnityEngine.Random.Range(0, 359),0));
    }

    private GameObject spawnObject(GameObject obj)
    {

        GameObject o = Instantiate(obj, new Vector3(0,0,5), Quaternion.Euler(0, 0, 0));

        return o;

    }
}
