using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [Header("Зрение")]
    [SerializeField] private float viewDistance = 10f;
    [SerializeField, Range(10f, 180f)] private float viewAngle = 90f;
    [SerializeField] private float rayHeight = 0.5f;      // на какой высоте идут лучи
    [SerializeField] private LayerMask obstacleMask;       // что блокирует взгляд
    [SerializeField] private Transform target;             // кого ищем (если пусто, ищется по тегу Player)

    [Header("Визуал")]
    [SerializeField] private Material coneMaterial;
    [SerializeField] private Color normalColor = new Color(1f, 0.9f, 0.2f, 0.35f);
    [SerializeField] private Color detectedColor = new Color(1f, 0.1f, 0.1f, 0.45f);
    [SerializeField] private int rayCount = 40;
    [SerializeField] private float visualHeight = 0.05f;   // высота конуса над полом

    public bool CanSeeTarget { get; private set; }
    public Transform Target => target;

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }
        CreateVisual();
    }

    private void Update()
    {
        bool seesNow = CheckTarget();
        if (seesNow != CanSeeTarget)
        {
            CanSeeTarget = seesNow;
            Debug.Log(name + (seesNow ? ": вижу игрока!" : ": потерял игрока"));
        }
    }

    private void LateUpdate()
    {
        DrawCone();
        if (meshRenderer != null)
            meshRenderer.material.color = CanSeeTarget ? detectedColor : normalColor;
    }

    // ---------- ЛОГИКА ----------
    private bool CheckTarget()
    {
        if (target == null) return false;

        Vector3 origin = new Vector3(transform.position.x, rayHeight, transform.position.z);
        Vector3 targetPos = new Vector3(target.position.x, rayHeight, target.position.z);
        Vector3 toTarget = targetPos - origin;
        float dist = toTarget.magnitude;

        // 1. Далеко?
        if (dist > viewDistance) return false;

        // 2. Вне угла обзора?
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        if (Vector3.Angle(flatForward, toTarget) > viewAngle * 0.5f) return false;

        // 3. Стена на пути?
        if (Physics.Raycast(origin, toTarget.normalized, dist, obstacleMask)) return false;

        return true;
    }

    // ---------- ВИЗУАЛ ----------
    private void CreateVisual()
    {
        if (coneMaterial == null)
        {
            Debug.LogError("VisionCone: не назначен Cone Material на " + name);
            return;
        }

        GameObject go = new GameObject("ConeVisual");
        go.transform.SetParent(transform, false);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(coneMaterial);
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        mesh = new Mesh { name = "VisionConeMesh" };
        mf.mesh = mesh;
    }

    private void DrawCone()
    {
        if (mesh == null) return;

        Vector3 origin = new Vector3(transform.position.x, rayHeight, transform.position.z);
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = transform.InverseTransformPoint(new Vector3(origin.x, visualHeight, origin.z));

        float startAngle = -viewAngle * 0.5f;
        float step = viewAngle / rayCount;

        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 dir = Quaternion.AngleAxis(startAngle + step * i, Vector3.up) * flatForward;
            Vector3 point = origin + dir * viewDistance;

            // Если луч уткнулся в стену, конус обрезается
            if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance, obstacleMask))
                point = hit.point;

            point.y = visualHeight;
            vertices[i + 1] = transform.InverseTransformPoint(point);

            if (i < rayCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }
}
