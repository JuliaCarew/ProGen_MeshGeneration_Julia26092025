using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    #region Variables

    [Header("Mesh Settings")]
    [SerializeField] int width = 30;
    [SerializeField] int height = 30;

    [Header("Wave Animation")]
    [SerializeField] float amplitude = 0.5f; // height
    [SerializeField] float frequency = 1f; // speed
    [SerializeField] float wavelength = 5f; // distance between waves

    [Header("Realistic Wave Settings")]
    [SerializeField] int waveCount = 4; // number of wave directions
    [SerializeField] float waveSpeed = 2f; 
    [SerializeField] Vector2 windDirection = new Vector2(1f, 0.5f); 
    [SerializeField] float waveHeight = 0.8f; // maximum wave height

    // mesh & vertices
    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;

    [Header("DEBUG")]
    public bool debug = false;

    #endregion

    void Start() => GenerateMesh();

    void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "Animated Water Plane";

        Vector3[] verts = new Vector3[(width + 1) * (height + 1)];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        for (int i = 0, pos = 0; i < height + 1; i++)
        {
            for (int j = 0; j < width + 1; j++)
            {
                verts[pos] = new Vector3(j, 0, i);

                uvs[pos] = new Vector2((float)j / width, (float)i / height);
                normals[pos] = Vector3.up;

                pos++;
            }
        }

        int[] tris = new int[width * height * 6];

        int tri = 0;
        int vert = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                tris[tri + 0] = vert + 0;
                tris[tri + 1] = vert + width + 1;
                tris[tri + 2] = vert + 1;
                tris[tri + 3] = vert + 1;
                tris[tri + 4] = vert + width + 1;
                tris[tri + 5] = vert + width + 2;

                tri += 6;
                vert++;

            }
            vert++;
        }

        // re-assign mesh data
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        // store base vertices for animation
        baseVertices = mesh.vertices;
        animatedVertices = new Vector3[baseVertices.Length];

        if (debug)
            Debug.Log($"Generated animated water mesh with {baseVertices.Length} vertices");
    }

    void Update() => AnimateWater();

    void AnimateWater()
    {
        if (mesh == null || baseVertices == null) return;

        float waveTime = Time.time * waveSpeed;

        // animate vertices
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 originalVertex = baseVertices[i];

            // reset position to base
            Vector3 newPos = originalVertex;
            newPos.y = 0;

            // apply anim for miltiple waves
            for (int w = 0; w < waveCount; w++)
            {
                // animate in different directions
                float directionAngle = (w / (float)waveCount) * 2f * Mathf.PI + windDirection.x;
                Vector2 waveDir = new Vector2(Mathf.Cos(directionAngle), Mathf.Sin(directionAngle));

                // make different wave amplitude and frequency for every wave
                float waveAmp = waveHeight / (1f + w * 0.8f);
                float waveFreq = frequency * (1f + w * 0.2f);

                // generate wave
                float distanceAlongWave = Vector2.Dot(waveDir, new Vector2(originalVertex.x, originalVertex.z));
                float wavePos = distanceAlongWave / wavelength + waveTime * waveFreq;

                // get more realistic shape
                float verticalOffset = Mathf.Sin(wavePos) * waveAmp;
                
                // add height
                newPos.y += verticalOffset;
            }
            
            // noise for more natural variation
            float noiseScale = 0.1f;
            float noise = Mathf.PerlinNoise(originalVertex.x * noiseScale + waveTime * 0.1f, originalVertex.z * noiseScale + waveTime * 0.1f);
            newPos.y += (noise - 0.5f) * amplitude * 0.3f;
            
            // store new animated position
            animatedVertices[i] = newPos;
        }

        // update mesh
        mesh.vertices = animatedVertices;
        mesh.RecalculateNormals();
    }

    void OnDrawGizmos()
    {
        if (debug)
        {
            var mf = GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null) return;

            Mesh debugMesh = mf.sharedMesh;

            Vector3[] verts = debugMesh.vertices;

            for (int i = 0; i < verts.Length; i++)
            {
                // convert local vertex position to world position
                Vector3 worldPos = transform.TransformPoint(verts[i]);
                Gizmos.DrawSphere(worldPos, 0.05f);
            }
        }
    }
}
