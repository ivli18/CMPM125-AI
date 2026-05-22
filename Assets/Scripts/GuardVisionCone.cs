using UnityEngine;

public class GuardVisionCone : MonoBehaviour
{
    [Header("Cone Settings")]
    public float range = 10f;
    public float angle = 60f;
    public int resolution = 30; // smoothness

    [Header("Circle Settings")]
    public float circleRadius = 2f;
    public int circleResolution = 30;

    private Mesh coneMesh;
    private Mesh circleMesh;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material coneMaterial;

    // Combined mesh
    private Mesh combinedMesh;

    void Awake()
    {
        GameObject vizObj = new GameObject("VisionMesh");
        vizObj.transform.SetParent(transform);
        vizObj.transform.localPosition = Vector3.zero;
        vizObj.transform.localRotation = Quaternion.identity;
        meshFilter = vizObj.AddComponent<MeshFilter>();
        meshRenderer = vizObj.AddComponent<MeshRenderer>();

        // Create a material with transparency
        coneMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        coneMaterial.color = new Color(0f, 1f, 0f, 0.2f); // start green, low alpha
        meshRenderer.material = coneMaterial;

        combinedMesh = new Mesh();
        meshFilter.mesh = combinedMesh;
    }

    // Call this every frame from GuardAgent
    public void UpdateVision(float coneRange, float coneAngle, float circleRadius, Color stateColor, float suspicionMeter)
    {
        this.range = coneRange;
        this.angle = coneAngle;
        this.circleRadius = circleRadius;

        float alpha = Mathf.Lerp(0.1f, 0.5f, suspicionMeter / 100f);
        coneMaterial.color = new Color(stateColor.r, stateColor.g, stateColor.b, alpha);

        BuildCombinedMesh();
    }

    private void BuildCombinedMesh()
    {
        // --- CONE MESH (triangle fan) ---
        int coneVerts = resolution + 2; // origin + arc points + close
        Vector3[] coneVertices = new Vector3[coneVerts];
        int[] coneTriangles = new int[resolution * 3];

        coneVertices[0] = Vector3.up * 0.05f; // origin slightly above floor

        float halfAngle = angle / 2f;
        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            float a = Mathf.Lerp(-halfAngle, halfAngle, t) * Mathf.Deg2Rad;
            float x = Mathf.Sin(a) * range;
            float z = Mathf.Cos(a) * range;
            coneVertices[i + 1] = new Vector3(x, 0.05f, z);
        }

        for (int i = 0; i < resolution; i++)
        {
            coneTriangles[i * 3 + 0] = 0;
            coneTriangles[i * 3 + 1] = i + 1;
            coneTriangles[i * 3 + 2] = i + 2;
        }

        // --- CIRCLE MESH (triangle fan) ---
        int circleVerts = circleResolution + 2;
        Vector3[] circleVertices = new Vector3[circleVerts];
        int[] circleTriangles = new int[circleResolution * 3];

        circleVertices[0] = Vector3.up * 0.05f;

        for (int i = 0; i <= circleResolution; i++)
        {
            float a = (float)i / circleResolution * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(a) * circleRadius;
            float z = Mathf.Sin(a) * circleRadius;
            circleVertices[i + 1] = new Vector3(x, 0.05f, z);
        }

        for (int i = 0; i < circleResolution; i++)
        {
            circleTriangles[i * 3 + 0] = 0;
            circleTriangles[i * 3 + 1] = i + 1;
            circleTriangles[i * 3 + 2] = i + 2;
        }

        // --- COMBINE INTO ONE MESH ---
        combinedMesh.Clear();

        // Offset circle vertices so indices don't clash
        Vector3[] allVerts = new Vector3[coneVerts + circleVerts];
        coneVertices.CopyTo(allVerts, 0);
        circleVertices.CopyTo(allVerts, coneVerts);

        int[] allTris = new int[coneTriangles.Length + circleTriangles.Length];
        coneTriangles.CopyTo(allTris, 0);

        // Offset circle triangle indices
        for (int i = 0; i < circleTriangles.Length; i++)
            allTris[coneTriangles.Length + i] = circleTriangles[i] + coneVerts;

        combinedMesh.vertices = allVerts;
        combinedMesh.triangles = allTris;
        combinedMesh.RecalculateNormals();
    }
}