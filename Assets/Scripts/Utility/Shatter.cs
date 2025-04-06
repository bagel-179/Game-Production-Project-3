using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shatter : MonoBehaviour
{
    [Header("Shatter Settings")]
    [Tooltip("Number of recursive splits")]
    [SerializeField] private int CutCascades = 4;

    [Tooltip("Force applied to fragments")]
    [SerializeField] private float ExplodeForce = 30;

    [Header("Fragment Settings")]
    [Tooltip("Time before fragments are destroyed (0 = never)")]
    [SerializeField] private float FragmentLifetime = 4f;

    [Tooltip("Fade out duration before destruction")]
    [SerializeField] private float FadeDuration = 1f;

    [Header("Destruction Effects")]
    [Tooltip("Duration of shrink animation before destruction")]
    [SerializeField] private float shrinkDuration = 4f;

    [Tooltip("Minimum fragment size (stops recursive splitting)")]
    [SerializeField] private float MinFragmentSize = 0.1f;

    private bool edgeSet = false;
    private Vector3 edgeVertex = Vector3.zero;
    private Vector2 edgeUV = Vector2.zero;
    private Plane edgePlane = new Plane();

    public void EnableShattering()
    {
        Debug.Log("Shattering Enabled!");
        DestroyMesh();
    }

    private void DestroyMesh()
    {
        var originalMesh = GetComponent<MeshFilter>().mesh;
        originalMesh.RecalculateBounds();
        var parts = new List<PartMesh>();
        var subParts = new List<PartMesh>();

        var mainPart = new PartMesh()
        {
            UV = originalMesh.uv,
            Vertices = originalMesh.vertices,
            Normals = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };

        for (int i = 0; i < originalMesh.subMeshCount; i++)
            mainPart.Triangles[i] = originalMesh.GetTriangles(i);

        parts.Add(mainPart);

        for (var c = 0; c < CutCascades; c++)
        {
            for (var i = 0; i < parts.Count; i++)
            {
                // Skip splitting if fragment is too small
                if (parts[i].Bounds.size.magnitude < MinFragmentSize)
                {
                    subParts.Add(parts[i]);
                    continue;
                }

                var bounds = parts[i].Bounds;
                bounds.Expand(0.5f);

                var plane = new Plane(
                    UnityEngine.Random.onUnitSphere,
                    new Vector3(
                        UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                        UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                        UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));

                subParts.Add(GenerateMesh(parts[i], plane, true));
                subParts.Add(GenerateMesh(parts[i], plane, false));
            }
            parts = new List<PartMesh>(subParts);
            subParts.Clear();
        }

        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].MakeGameobject(this);
            var rb = parts[i].GameObject.GetComponent<Rigidbody>();
            if (rb != null && ExplodeForce > 0)
            {
                rb.AddForceAtPosition(parts[i].Bounds.center * ExplodeForce, transform.position);
            }

            // Start lifetime coroutine on THE FRAGMENT, not the original object
            if (FragmentLifetime > 0)
            {
                var fragmentScript = parts[i].GameObject.GetComponent<Shatter>();
                if (fragmentScript != null)
                {
                    fragmentScript.StartCoroutine(fragmentScript.DestroyFragmentAfterTime(
                        parts[i].GameObject, FragmentLifetime, FadeDuration, shrinkDuration));
                }
                else
                {
                    // If no Shatter component, add a temporary MonoBehaviour to run the coroutine
                    var coroutineRunner = parts[i].GameObject.AddComponent<FragmentCoroutineRunner>();
                    coroutineRunner.StartCoroutine(DestroyFragmentAfterTime(
                        parts[i].GameObject, FragmentLifetime, FadeDuration, shrinkDuration));
                }
            }
        }

        Destroy(gameObject);
    }

    private IEnumerator DestroyFragmentAfterTime(GameObject fragment, float lifetime, float fadeDuration, float shrinkDuration = 0.5f)
    {
        yield return new WaitForSeconds(lifetime);

        float shrinkTimer = 0f;
        Vector3 originalScale = fragment.transform.localScale;

        while (shrinkTimer < shrinkDuration)
        {
            if (fragment == null) yield break;

            shrinkTimer += Time.deltaTime;
            float progress = shrinkTimer / shrinkDuration;
            fragment.transform.localScale = originalScale * Mathf.Lerp(1f, 0.01f, progress);
            yield return null;
        }

        if (fadeDuration > 0)
        {
            // Handle fade out if needed
            var renderer = fragment.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                float startTime = Time.time;
                Color[] originalColors = new Color[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    originalColors[i] = renderer.materials[i].color;
                }

                while (Time.time < startTime + fadeDuration)
                {
                    float progress = (Time.time - startTime) / fadeDuration;
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        Color newColor = originalColors[i];
                        newColor.a = Mathf.Lerp(1, 0, progress);
                        renderer.materials[i].color = newColor;
                    }
                    yield return null;
                }
            }
        }

        Destroy(fragment);
    }

    private PartMesh GenerateMesh(PartMesh original, Plane plane, bool left)
    {
        var partMesh = new PartMesh() { };
        var ray1 = new Ray();
        var ray2 = new Ray();

        for (var i = 0; i < original.Triangles.Length; i++)
        {
            var triangles = original.Triangles[i];
            edgeSet = false;

            for (var j = 0; j < triangles.Length; j = j + 3)
            {
                var sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                var sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                var sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                var sideCount = (sideA ? 1 : 0) +
                                (sideB ? 1 : 0) +
                                (sideC ? 1 : 0);
                if (sideCount == 0) continue;
                if (sideCount == 3)
                {
                    partMesh.AddTriangle(i,
                                     original.Vertices[triangles[j]],
                                     original.Vertices[triangles[j + 1]],
                                     original.Vertices[triangles[j + 2]],
                                     original.Normals[triangles[j]],
                                     original.Normals[triangles[j + 1]],
                                     original.Normals[triangles[j + 2]],
                                     original.UV[triangles[j]],
                                     original.UV[triangles[j + 1]],
                                     original.UV[triangles[j + 2]]);
                    continue;
                }

                // Cut points
                var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                ray1.origin = original.Vertices[triangles[j + singleIndex]];
                var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                ray1.direction = dir1;
                plane.Raycast(ray1, out var enter1);
                var lerp1 = enter1 / dir1.magnitude;

                ray2.origin = original.Vertices[triangles[j + singleIndex]];
                var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                ray2.direction = dir2;
                plane.Raycast(ray2, out var enter2);
                var lerp2 = enter2 / dir2.magnitude;

                // First vertex = anchor
                AddEdge(i,
                    partMesh,
                    left ? plane.normal * -1f : plane.normal,
                    ray1.origin + ray1.direction.normalized * enter1,
                    ray2.origin + ray2.direction.normalized * enter2,
                    Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                    Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                if (sideCount == 1)
                {
                    partMesh.AddTriangle(i,
                        original.Vertices[triangles[j + singleIndex]],
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        original.Normals[triangles[j + singleIndex]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        original.UV[triangles[j + singleIndex]],
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    continue;
                }

                if (sideCount == 2)
                {
                    partMesh.AddTriangle(i,
                        ray1.origin + ray1.direction.normalized * enter1,
                        original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                        original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                    partMesh.AddTriangle(i,
                        ray1.origin + ray1.direction.normalized * enter1,
                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    continue;
                }
            }
        }

        partMesh.FillArrays();
        return partMesh;
    }

    private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
    {
        if (!edgeSet)
        {
            edgeSet = true;
            edgeVertex = vertex1;
            edgeUV = uv1;
        }
        else
        {
            edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

            partMesh.AddTriangle(subMesh,
                edgeVertex,
                edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2,
                edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                normal,
                normal,
                normal,
                edgeUV,
                uv1,
                uv2);
        }
    }

    public class PartMesh
    {
        private List<Vector3> _Vertices = new List<Vector3>();
        private List<Vector3> _Normals = new List<Vector3>();
        private List<List<int>> _Triangles = new List<List<int>>();
        private List<Vector2> _UVs = new List<Vector2>();
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public int[][] Triangles;
        public Vector2[] UV;
        public GameObject GameObject;
        public Bounds Bounds = new Bounds();

        public PartMesh() { }

        public void AddTriangle(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1, Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (_Triangles.Count - 1 < submesh)
                _Triangles.Add(new List<int>());

            _Triangles[submesh].Add(_Vertices.Count);
            _Vertices.Add(vert1);
            _Triangles[submesh].Add(_Vertices.Count);
            _Vertices.Add(vert2);
            _Triangles[submesh].Add(_Vertices.Count);
            _Vertices.Add(vert3);
            _Normals.Add(normal1);
            _Normals.Add(normal2);
            _Normals.Add(normal3);
            _UVs.Add(uv1);
            _UVs.Add(uv2);
            _UVs.Add(uv3);

            // Fixed bounds calculation
            Bounds.Encapsulate(vert1);
            Bounds.Encapsulate(vert2);
            Bounds.Encapsulate(vert3);
        }

        public void FillArrays()
        {
            Vertices = _Vertices.ToArray();
            Normals = _Normals.ToArray();
            UV = _UVs.ToArray();
            Triangles = new int[_Triangles.Count][];
            for (var i = 0; i < _Triangles.Count; i++)
                Triangles[i] = _Triangles[i].ToArray();
        }

        public void MakeGameobject(Shatter original)
        {
            GameObject = new GameObject(original.name + "_Fragment");
            GameObject.transform.position = original.transform.position;
            GameObject.transform.rotation = original.transform.rotation;
            GameObject.transform.localScale = original.transform.localScale;

            var mesh = new Mesh();
            mesh.name = original.GetComponent<MeshFilter>().mesh.name + "_Fragment";

            mesh.vertices = Vertices;
            mesh.normals = Normals;
            mesh.uv = UV;
            for (var i = 0; i < Triangles.Length; i++)
                mesh.SetTriangles(Triangles[i], i, true);

            mesh.RecalculateBounds();
            Bounds = mesh.bounds;

            var renderer = GameObject.AddComponent<MeshRenderer>();
            renderer.materials = original.GetComponent<MeshRenderer>().materials;

            var filter = GameObject.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            var collider = GameObject.AddComponent<MeshCollider>();
            collider.convex = true;

            GameObject.AddComponent<Rigidbody>();

            // Only add Shatter component if cascades remain
            if (original.CutCascades > 1)
            {
                var shatter = GameObject.AddComponent<Shatter>();
                shatter.CutCascades = original.CutCascades - 1;
                shatter.ExplodeForce = original.ExplodeForce;
                shatter.FragmentLifetime = original.FragmentLifetime;
                shatter.FadeDuration = original.FadeDuration;
                shatter.MinFragmentSize = original.MinFragmentSize;
            }
        }
    }

    public class FragmentCoroutineRunner : MonoBehaviour
    {
        public static IEnumerator DestroyFragmentAfterTime(GameObject fragment, float lifetime, float fadeDuration)
        {
            yield return new WaitForSeconds(lifetime);

            if (fragment == null) yield break;

            if (fadeDuration > 0)
            {
                var renderer = fragment.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    float startTime = Time.time;
                    Material[] materials = renderer.materials;
                    Color[] originalColors = new Color[materials.Length];

                    for (int i = 0; i < materials.Length; i++)
                    {
                        originalColors[i] = materials[i].color;
                    }

                    while (Time.time < startTime + fadeDuration)
                    {
                        if (fragment == null) yield break;

                        float progress = (Time.time - startTime) / fadeDuration;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i] != null)
                            {
                                Color newColor = originalColors[i];
                                newColor.a = Mathf.Lerp(1, 0, progress);
                                materials[i].color = newColor;
                            }
                        }
                        yield return null;
                    }
                }
            }

            Destroy(fragment);
        }
    }
}