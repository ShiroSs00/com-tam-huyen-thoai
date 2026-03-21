using UnityEngine;
using UnityEditor;
using System.Text;

public class DebugPrefabFixer : ScriptableObject
{
    [MenuItem("Tools/Debug Missing Meshes")]
    public static void CheckMeshes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/NguyenLieu" });
        StringBuilder sb = new StringBuilder();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            sb.AppendLine($"--- PREFAB: {prefab.name} ---");
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            
            if (renderers.Length == 0)
            {
                sb.AppendLine("  NO RENDERERS AT ALL!");
            }
            else
            {
                foreach (var r in renderers)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    string meshName = (mf != null && mf.sharedMesh != null) ? mf.sharedMesh.name : "NULL";
                    sb.AppendLine($"  Renderer on: {r.gameObject.name} | ActiveSelf: {r.gameObject.activeSelf} | ComponentEnabled: {r.enabled} | Bounds: {r.bounds.extents} | Mesh: {meshName}");
                }
            }
        }
        Debug.Log(sb.ToString());
    }
}
