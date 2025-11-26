using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
[ExecuteAlways]
public class RoadGenerator : MonoBehaviour
{
    [Header("Road Settings")]
    [Range(0.5f, 20f)] public float roadWidth = 4f;
    [Range(1, 50)] public int segmentsPerUnit = 10;
    public Material roadMaterial;
    
    [Header("Auto Update")]
    public bool autoUpdate = true;
    
    private Spline lastSpline;
    private int lastKnotCount;
    
    void Start()
    {
        GenerateRoad();
    }
    
    void OnValidate()
    {
        if (autoUpdate && !Application.isPlaying)
        {
            GenerateRoad();
        }
    }
    
    void Update()
    {
        if (autoUpdate && !Application.isPlaying)
        {
            var splineContainer = GetComponent<SplineContainer>();
            if (splineContainer != null && splineContainer.Spline != null)
            {
                if (lastSpline != splineContainer.Spline || 
                    lastKnotCount != splineContainer.Spline.Count)
                {
                    lastSpline = splineContainer.Spline;
                    lastKnotCount = splineContainer.Spline.Count;
                    GenerateRoad();
                }
            }
        }
    }
    
    [ContextMenu("Generate Road")]
    public void GenerateRoad()
    {
        var splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null || splineContainer.Spline == null)
        {
            return;
        }
        
        var spline = splineContainer.Spline;
        
        if (spline.Count < 2)
        {
            return;
        }
        
        float splineLength = spline.GetLength();
        int totalSegments = Mathf.Max(2, Mathf.CeilToInt(splineLength * segmentsPerUnit));
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        float accumulatedDistance = 0f;
        Vector3 previousPosition = Vector3.zero;
        
        // Generate vertices along spline
        for (int i = 0; i < totalSegments; i++)
        {
            float t = i / (float)(totalSegments - 1);
            
            // Get position and direction at this point
            float3 position = spline.EvaluatePosition(t);
            float3 tangent = spline.EvaluateTangent(t);
            float3 up = new float3(0, 1, 0);
            float3 right = math.normalize(math.cross(up, tangent));
            
            // Calculate actual distance traveled
            if (i > 0)
            {
                accumulatedDistance += Vector3.Distance(previousPosition, position);
            }
            previousPosition = position;
            
            float halfRoad = roadWidth * 0.5f;
            
            // Create 2 vertices (left and right edge)
            vertices.Add(position - right * halfRoad); // Left edge
            vertices.Add(position + right * halfRoad); // Right edge
            
            // UVs: 0 to 1 across road, distance along road
            uvs.Add(new Vector2(0, accumulatedDistance));
            uvs.Add(new Vector2(1, accumulatedDistance));
        }
        
        // Generate triangles
        for (int i = 0; i < totalSegments - 1; i++)
        {
            int v = i * 2;
            int vn = (i + 1) * 2;
            
            AddQuad(triangles, v, v + 1, vn, vn + 1);
        }
        
        // Create mesh
        Mesh mesh = new Mesh();
        mesh.name = "Road Mesh";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Apply to GameObject
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        if (Application.isPlaying)
        {
            meshFilter.mesh = mesh;
            if (roadMaterial != null)
                meshRenderer.material = roadMaterial;
        }
        else
        {
            meshFilter.sharedMesh = mesh;
            if (roadMaterial != null)
                meshRenderer.sharedMaterial = roadMaterial;
        }
        
        // Add collider
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null) collider = gameObject.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
    }
    
    void AddQuad(List<int> triangles, int v0, int v1, int v2, int v3)
    {
        triangles.Add(v0);
        triangles.Add(v2);
        triangles.Add(v1);
        
        triangles.Add(v1);
        triangles.Add(v2);
        triangles.Add(v3);
    }
}
