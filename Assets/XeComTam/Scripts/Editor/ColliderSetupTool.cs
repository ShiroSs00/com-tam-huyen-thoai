using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool tự động thêm collider phù hợp cho tất cả prefab trong project.
/// Truy cập qua menu: Tools → Cơm Tấm → Setup Colliders
/// </summary>
public static class ColliderSetupTool
{
    // ── Đường dẫn prefab theo từng nhóm ──────────────────────────────────────
    private const string BASE = "Assets/Prefabs";

    private static readonly string[] BuildingFolders = { BASE + "/Buildings" };
    private static readonly string[] FloorFolders = { BASE + "/Floor" };
    private static readonly string[] LampFolders = { BASE + "/Lamps" };
    private static readonly string[] TrafficFolders = { BASE + "/traffic signs" };
    private static readonly string[] LetterFolders = { BASE + "/Letters" };
    private static readonly string[] PropFolders = { BASE + "/Props" };
    private static readonly string[] XeFolders = { BASE + "/XeComTam" };

    // Props dạng trụ (cần CapsuleCollider thay vì Box)
    private static readonly HashSet<string> CylinderPropNames = new HashSet<string>
    {
        "power_poles", "Bus stop pole", "platt pole", "Hydrant",
        "parking meter", "streetprop", "Traffic_pot", "phone booth",
        "Lamp_1", "Lamp_2", "Lamp_3", "Lamp_4", "Lamp_5", "Lamp_6", "Lamp_7",
        "street lamp 1", "street lamp 2"
    };

    // ── Menu entries ──────────────────────────────────────────────────────────
    [MenuItem("Tools/Cơm Tấm/1. Setup ALL Colliders")]
    public static void SetupAll()
    {
        SetupBuildings();
        SetupFloor();
        SetupLamps();
        SetupTrafficSigns();
        SetupLetters();
        SetupProps();
        SetupXeComTam();
        AssetDatabase.SaveAssets();
        Debug.Log("[ColliderSetup] Hoàn tất! Đã xử lý tất cả prefab.");
    }

    [MenuItem("Tools/Cơm Tấm/2. Setup Buildings Colliders")]
    public static void SetupBuildings()
        => ProcessFolders(BuildingFolders, AddBoxCollider, "Buildings");

    [MenuItem("Tools/Cơm Tấm/3. Setup Floor Colliders")]
    public static void SetupFloor()
        => ProcessFolders(FloorFolders, AddMeshColliderNonConvex, "Floor");

    [MenuItem("Tools/Cơm Tấm/4. Setup Lamps Colliders")]
    public static void SetupLamps()
        => ProcessFolders(LampFolders, AddCapsuleCollider, "Lamps");

    [MenuItem("Tools/Cơm Tấm/5. Setup Traffic Signs Colliders")]
    public static void SetupTrafficSigns()
        => ProcessFolders(TrafficFolders, AddBoxCollider, "Traffic Signs");

    [MenuItem("Tools/Cơm Tấm/6. Setup Letters Colliders")]
    public static void SetupLetters()
        => ProcessFolders(LetterFolders, AddBoxCollider, "Letters");

    [MenuItem("Tools/Cơm Tấm/7. Setup Props Colliders")]
    public static void SetupProps()
    {
        ProcessFolders(PropFolders, (go) =>
        {
            bool isCylinder = false;
            foreach (string k in CylinderPropNames)
                if (go.name.Contains(k)) { isCylinder = true; break; }

            if (isCylinder) AddCapsuleCollider(go);
            else AddBoxCollider(go);
        }, "Props");
    }

    [MenuItem("Tools/Cơm Tấm/8. Setup XeComTam + Interactable")]
    public static void SetupXeComTam()
    {
        // Tên các vật thể KHÔNG phải interactable (bộ khung xe)
        var nonInteractableNames = new HashSet<string> { "XeComTam", "XeCơmTấm", "XeCơmTam" };

        ProcessFolders(XeFolders, (go) =>
        {
            bool isInteractable = true;
            foreach (string n in nonInteractableNames)
                if (go.name.Contains(n)) { isInteractable = false; break; }

            // MeshCollider convex cho tất cả XeComTam objects
            AddMeshColliderConvex(go);

            if (isInteractable)
            {
                // Đánh tag Interactable
                EnsureTag("Interactable");
                go.tag = "Interactable";

                // Thêm InteractableObject nếu chưa có
                if (go.GetComponent<InteractableObject>() == null)
                {
                    var io = go.AddComponent<InteractableObject>();
                    // Gán tên dựa theo tên file
                    var so = new SerializedObject(io);
                    so.FindProperty("interactName").stringValue = PrettifyName(go.name);
                    so.FindProperty("interactHint").stringValue = "Nhấn [E] để xem";
                    so.ApplyModifiedProperties();
                }
            }
        }, "XeComTam");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void ProcessFolders(string[] folders, System.Action<GameObject> action, string label)
    {
        int count = 0;
        foreach (string folder in folders)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = PrefabUtility.LoadPrefabContents(path);
                if (prefab == null) continue;

                action(prefab);
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                PrefabUtility.UnloadPrefabContents(prefab);
                count++;
            }
        }
        Debug.Log($"[ColliderSetup] {label}: đã xử lý {count} prefabs.");
    }

    private static void AddBoxCollider(GameObject go)
    {
        if (go.GetComponent<Collider>() != null) return; // Đã có → bỏ qua
        go.AddComponent<BoxCollider>();
    }

    private static void AddCapsuleCollider(GameObject go)
    {
        if (go.GetComponent<Collider>() != null) return;
        var cap = go.AddComponent<CapsuleCollider>();
        cap.direction = 1; // Y-axis (dọc)
    }

    private static void AddMeshColliderNonConvex(GameObject go)
    {
        if (go.GetComponent<Collider>() != null) return;
        var mc = go.AddComponent<MeshCollider>();
        mc.convex = false;
    }

    private static void AddMeshColliderConvex(GameObject go)
    {
        // Neu root da co collider → bo qua
        if (go.GetComponent<Collider>() != null) return;

        // Thu them MeshCollider vao tung child co MeshFilter
        bool added = false;
        foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            if (mf.GetComponent<Collider>() != null) continue;
            var mc = mf.gameObject.AddComponent<MeshCollider>();
            mc.convex = true;
            added = true;
        }

        // Fallback: neu khong co MeshFilter nao → dung BoxCollider bao quanh Renderer.bounds
        if (!added)
        {
            AddBoxColliderFromBounds(go);
        }
    }

    /// <summary>Them BoxCollider o root, size theo tong bounds cua tat ca Renderer con.</summary>
    private static void AddBoxColliderFromBounds(GameObject go)
    {
        if (go.GetComponent<Collider>() != null) return;

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        var box = go.AddComponent<BoxCollider>();
        // Chuyen bounds tu world sang local space cua root
        box.center = go.transform.InverseTransformPoint(combined.center);
        box.size   = Vector3.Scale(combined.size,
                       new Vector3(1f / go.transform.lossyScale.x,
                                   1f / go.transform.lossyScale.y,
                                   1f / go.transform.lossyScale.z));
    }

    private static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[ColliderSetup] Đã tạo tag '{tag}'.");
    }

    private static string PrettifyName(string name)
    {
        // Ví dụ: "BếpGa" → "Bếp Ga", "NồiCơm" → "Nồi Cơm"
        var result = System.Text.RegularExpressions.Regex.Replace(name, @"(\p{Lu})", " $1").Trim();
        return result;
    }
}
