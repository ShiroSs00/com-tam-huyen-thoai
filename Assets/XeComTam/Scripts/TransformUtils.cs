using UnityEngine;

public static class TransformUtils
{
    /// <summary>
    /// SetParent but guarantees the object keeps its EXACT world scale (lossyScale)
    /// regardless of how skewed or scaled the new parent is.
    /// </summary>
    public static void SetParentKeepWorldScale(Transform child, Transform targetParent)
    {
        // 1. Luu tru world scale hien tai (kich thuoc that su ngoai doi)
        Vector3 originalWorldScale = child.lossyScale;

        // 2. Thuc hien SetParent binh thuong (true hay false khong quan trong vi
        // unity doi khi loi khi parent bi non-uniform scale)
        child.SetParent(targetParent, true);

        // 3. Tinh toan lai localScale sao cho sau khi nhan voi targetParent.lossyScale 
        // thi ket qua van bang originalWorldScale.
        // localScale = originalWorldScale / parent.lossyScale
        // Vi Unity lossyScale la float 3 ngan, phep chia co the bi chia cho 0 nen kiem tra truoc.
        Vector3 parentLossyScale = targetParent.lossyScale;
        Vector3 newLocalScale = new Vector3(
            parentLossyScale.x != 0 ? originalWorldScale.x / parentLossyScale.x : originalWorldScale.x,
            parentLossyScale.y != 0 ? originalWorldScale.y / parentLossyScale.y : originalWorldScale.y,
            parentLossyScale.z != 0 ? originalWorldScale.z / parentLossyScale.z : originalWorldScale.z
        );

        child.localScale = newLocalScale;
    }

    /// <summary>
    /// Giup bat tat ca MeshRenderer tren thang con. 
    /// Rat quan trong cho FBX imported models co root la Empty GameObject bi an.
    /// Hoac SkinnedMeshRenderer co bounds = 0
    /// </summary>
    public static void ForceEnableRenderers(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(true); // true de lay ca nhung cai bi an
        foreach (var r in renderers)
        {
            r.gameObject.SetActive(true); // BAT BUOC: Phai bat GameObject chua Renderer len
            r.enabled = true;             // Bat component Renderer len
            
            
            if (r is SkinnedMeshRenderer smr)
            {
                smr.updateWhenOffscreen = true;
                smr.localBounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)); // ep buoc bounds de bao tri render
            }
            else if (r is MeshRenderer)
            {
                MeshFilter mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    if (mf.sharedMesh.bounds.size.magnitude < 0.001f)
                    {
                        // Trick ghi de the tich the hien ra view thay vi bao tri tich=0 khien Camera Unity xoa mo hinh.
                        mf.sharedMesh.bounds = new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f));
                    }
                }
            }
        }
    }
}
