using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor tool: chinh scale hang loat cho cac prefab NguyenLieu.
/// Menu: Tools → Com Tam → Scale Nguyen Lieu Prefabs
/// </summary>
public static class ScaleSetupTool
{
    // Scale tuong ung cho tung prefab (theo ten file)
    private static readonly Dictionary<string, Vector3> ScaleMap = new Dictionary<string, Vector3>
    {
        { "Dia",         new Vector3(0.30f, 0.30f, 0.30f) },
        { "Com",         new Vector3(0.25f, 0.25f, 0.25f) },
        { "ComTam",      new Vector3(0.25f, 0.25f, 0.25f) },
        { "ThitSong",    new Vector3(0.20f, 0.20f, 0.20f) },
        { "ThitChin",    new Vector3(0.20f, 0.20f, 0.20f) },
        { "TrungSong",   new Vector3(0.15f, 0.15f, 0.15f) },
        { "TrungChien",  new Vector3(0.20f, 0.20f, 0.20f) },
        { "LatCaChua",   new Vector3(0.18f, 0.18f, 0.18f) },
        { "LatDuaLeo",   new Vector3(0.15f, 0.15f, 0.15f) },
    };

    private const string NGUYEN_LIEU_FOLDER = "Assets/Prefabs/NguyenLieu";

    [MenuItem("Tools/Com Tam/Scale Nguyen Lieu Prefabs")]
    public static void ScaleNguyenLieu()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { NGUYEN_LIEU_FOLDER });
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = PrefabUtility.LoadPrefabContents(path);
            if (prefab == null) continue;

            // Normalize ten: bo dau tieng Viet, space, ky tu dac biet
            string normalizedName = NormalizeName(prefab.name);

            Vector3 targetScale = Vector3.one;
            bool found = false;

            foreach (var kv in ScaleMap)
            {
                if (normalizedName.Contains(NormalizeName(kv.Key)))
                {
                    targetScale = kv.Value;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                prefab.transform.localScale = targetScale;
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                Debug.Log($"[ScaleSetup] {prefab.name} → scale {targetScale}");
                count++;
            }
            else
            {
                Debug.LogWarning($"[ScaleSetup] Khong tim thay scale cho: {prefab.name}");
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[ScaleSetup] Hoan tat! Da chinh scale {count} prefabs.");
    }

    // Chuyen tieng Viet co dau → khong dau, viet lien
    private static string NormalizeName(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        string s = input.ToLower();
        s = s.Replace("à","a").Replace("á","a").Replace("ả","a").Replace("ã","a").Replace("ạ","a");
        s = s.Replace("ă","a").Replace("ắ","a").Replace("ằ","a").Replace("ẳ","a").Replace("ẵ","a").Replace("ặ","a");
        s = s.Replace("â","a").Replace("ấ","a").Replace("ầ","a").Replace("ẩ","a").Replace("ẫ","a").Replace("ậ","a");
        s = s.Replace("è","e").Replace("é","e").Replace("ẻ","e").Replace("ẽ","e").Replace("ẹ","e");
        s = s.Replace("ê","e").Replace("ế","e").Replace("ề","e").Replace("ể","e").Replace("ễ","e").Replace("ệ","e");
        s = s.Replace("ì","i").Replace("í","i").Replace("ỉ","i").Replace("ĩ","i").Replace("ị","i");
        s = s.Replace("ò","o").Replace("ó","o").Replace("ỏ","o").Replace("õ","o").Replace("ọ","o");
        s = s.Replace("ô","o").Replace("ố","o").Replace("ồ","o").Replace("ổ","o").Replace("ỗ","o").Replace("ộ","o");
        s = s.Replace("ơ","o").Replace("ớ","o").Replace("ờ","o").Replace("ở","o").Replace("ỡ","o").Replace("ợ","o");
        s = s.Replace("ù","u").Replace("ú","u").Replace("ủ","u").Replace("ũ","u").Replace("ụ","u");
        s = s.Replace("ư","u").Replace("ứ","u").Replace("ừ","u").Replace("ử","u").Replace("ữ","u").Replace("ự","u");
        s = s.Replace("ỳ","y").Replace("ý","y").Replace("ỷ","y").Replace("ỹ","y").Replace("ỵ","y");
        s = s.Replace("đ","d");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9]", "");
        return s;
    }
}
