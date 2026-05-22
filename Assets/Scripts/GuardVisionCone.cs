using UnityEngine;

public class GuardVisionCone : MonoBehaviour
{
    [SerializeField] private int resolution = 30;
    [SerializeField] private int circleResolution = 30;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material coneMaterial;
    private Mesh combinedMesh;

    private float range, angle, circleRadius;

    void Awake()
    {
        GameObject vizObj = new GameObject("VisionMesh");
        vizObj.transform.SetParent(transform);
        vizObj.transform.localPosition = Vector3.zero;
        vizObj.transform.localRotation = Quaternion.identity;
        meshFilter = vizObj.AddComponent<MeshFilter>();
        meshRenderer = vizObj.AddComponent<MeshRenderer>();

        coneMaterial = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
        coneMaterial.color = new Color(0f, 1f, 0f, 0.2f);
        meshRenderer.material = coneMaterial;

        combinedMesh = new Mesh();
        meshFilter.mesh = combinedMesh;
    }

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
        int coneVerts = resolution + 2;
        Vector3[] coneVertices = new Vector3[coneVerts];
        int[] coneTriangles = new int[resolution * 3];

        coneVertices[0] = Vector3.up * 0.05f;
        float halfAngle = angle / 2f;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            float a = Mathf.Lerp(-halfAngle, halfAngle, t) * Mathf.Deg2Rad;
            coneVertices[i + 1] = new Vector3(Mathf.Sin(a) * range, 0.05f, Mathf.Cos(a) * range);
        }

        for (int i = 0; i < resolution; i++)
        {
            coneTriangles[i * 3 + 0] = 0;
            coneTriangles[i * 3 + 1] = i + 1;
            coneTriangles[i * 3 + 2] = i + 2;
        }

        int circleVerts = circleResolution + 2;
        Vector3[] circleVertices = new Vector3[circleVerts];
        int[] circleTriangles = new int[circleResolution * 3];

        circleVertices[0] = Vector3.up * 0.05f;

        for (int i = 0; i <= circleResolution; i++)
        {
            float a = (float)i / circleResolution * 360f * Mathf.Deg2Rad;
            circleVertices[i + 1] = new Vector3(Mathf.Cos(a) * circleRadius, 0.05f, Mathf.Sin(a) * circleRadius);
        }

        for (int i = 0; i < circleResolution; i++)
        {
            circleTriangles[i * 3 + 0] = 0;
            circleTriangles[i * 3 + 1] = i + 2;
            circleTriangles[i * 3 + 2] = i + 1;
        }

        combinedMesh.Clear();

        Vector3[] allVerts = new Vector3[coneVerts + circleVerts];
        coneVertices.CopyTo(allVerts, 0);
        circleVertices.CopyTo(allVerts, coneVerts);

        int[] allTris = new int[coneTriangles.Length + circleTriangles.Length];
        coneTriangles.CopyTo(allTris, 0);
        for (int i = 0; i < circleTriangles.Length; i++)
            allTris[coneTriangles.Length + i] = circleTriangles[i] + coneVerts;

        combinedMesh.vertices = allVerts;
        combinedMesh.triangles = allTris;
        combinedMesh.RecalculateNormals();
    }
}