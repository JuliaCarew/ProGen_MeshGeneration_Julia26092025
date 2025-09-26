using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class AnimateMesh : MonoBehaviour
{
    // Wave variables
    public float amplitude = 0.5f; // height
    public float frequency = 1f; // speed
    public float wavelength = 1f; // distance between waves

    // mesh / vertices
    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] animatedVertices;

    void Awake()
    {
        // MeshGenerator runs first by getting the component
        var meshGen = GetComponent<MeshGenerator>();
        if (meshGen != null)
        {
            // create the mesh
            //meshGen.Start();
        }
    }

    void Start()
    {
        // get reference to generated mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;

        // get positions of base vertices
        baseVertices = mesh.vertices;
        animatedVertices = new Vector3[baseVertices.Length];

        Debug.Log($"AnimateMesh initialized with {baseVertices.Length} vertices");
    }

    // Update is called once per frame
    void Update()
    {
        if (mesh == null || baseVertices == null) return;

        float time = Time.time * frequency;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            // reference base vertices
            Vector3 verts = baseVertices[i];

            // sine wave on Y axis to animate
            verts.y = Mathf.Sin((verts.x + verts.z) / wavelength + time) * amplitude;

            // store that position as animated pos
            animatedVertices[i] = verts;
        }

        // apply animated vertices to mesh
        mesh.vertices = animatedVertices;
        mesh.RecalculateNormals();
    }
}
