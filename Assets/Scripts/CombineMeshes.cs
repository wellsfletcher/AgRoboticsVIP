using UnityEngine;
using System.Collections;

// Copy meshes from children into the parent's Mesh.
// CombineInstance stores the list of meshes.  These are combined
// and assigned to the attached Mesh.

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CombineMeshes : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            Combine();
        }
    }

    private MeshFilter[] GetImmediateChildren() {
        MeshFilter[] meshFilters = new MeshFilter[transform.childCount];
        int k = 0;
        foreach (Transform child in transform) {
            MeshFilter childMesh = child.GetComponent<MeshFilter>();
            meshFilters[k] = childMesh;
            // PrimitiveType.Sphere.
            k++;
        }
        return meshFilters;
    } 

    void Combine() {
        // MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        MeshFilter[] meshFilters = GetImmediateChildren();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        Mesh thisMesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh = thisMesh;
        thisMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        thisMesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
    }
}