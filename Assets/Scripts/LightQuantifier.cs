using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightQuantifier : MonoBehaviour {

    public Transform grid;
    private HashSet<Transform> set = new HashSet<Transform>();
    private bool hideRaycast = false;
    public TreeImporter treeImporter;

    // Start is called before the first frame update
    void Start() {
        QuantifyLight(grid);
        // treeImporter = gameObject.GetComponent<TreeImporter>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.H)) {
            hideRaycast = !hideRaycast;
        }

        // only call this as needed, insteaded of on every update
        // i.e. whenever sun is moved, branch is deleted, etc.
        // QuantifyLight(grid);
        if (shouldUpdateTree) {
            UpdateTree();
            shouldUpdateTree = false;
        }
    }
    
    public void UpdateTree() {
        QuantifyLight(grid);
        Debug.Log("tree updated");
    }

    private bool shouldUpdateTree = false;
    public void QueueTreeUpdate() {
        shouldUpdateTree = true;
    }

    void QuantifyLight(Transform grid) {
        // use counter to keep track of which cylinders are hit by the raycast
        Dictionary<Transform, int> counter = new Dictionary<Transform, int>();
        // LayerMask layerMask = -1;
        LayerMask layerMask = LayerMask.GetMask("cylinder");
        RaycastHit hit;
        // do a bunch of raycasts from the grid
        Vector3 direction = grid.TransformDirection(Vector3.up);
        Vector3 offset = new Vector3(-1, 0, -1);
        float STEP = 0.025f; // 0.025f; // 0.01f; // 0.025f;
        float SCALE = 5.0f;
        for (float x = 0; x <= 2.0; x += STEP) {
            for (float z = 0; z <= 2.0; z += STEP) {
                // determine raycast position and direction
                Vector3 localStart = new Vector3(offset.x + x, offset.y, offset.z + z) * SCALE;
                Vector3 start = grid.TransformPoint(localStart); // grid.position;
                // perform the raycast
                bool collided = Physics.Raycast(start, direction, out hit, Mathf.Infinity, layerMask);
                if (!hideRaycast)
                    Debug.DrawRay(start, direction);
                if (collided) {
                    Transform cylinder = hit.transform;
                    if (!hideRaycast)
                        Debug.DrawLine(start, hit.point, Color.red);
                    if (counter.ContainsKey(cylinder)) {
                        counter[cylinder] = counter[cylinder] + 1;
                    }
                    else {
                        counter[cylinder] = 1;
                    }
                }
            }
        }

        VisualizeRaycast(counter);
    }

    void ResetColors() {
        foreach (Transform cylinder in set) {
            // cylinder.GetComponent<MeshRenderer>().material.color = new Color(0.0f, 0.0f, 0.0f);
            // cylinder.GetComponent<MeshRenderer>().material.color = treeImporter.GetBranchColor(cylinder.gameObject);
            Color branchColor = treeImporter.GetBranchColor(cylinder.gameObject);
            Color color = Color.Lerp(Color.black, branchColor, 0.5f);
            cylinder.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    public int totalHits = 0;
    void VisualizeRaycast(Dictionary<Transform, int> counter) {
        ResetColors();

        int maxCount = 0;
        totalHits = 0;
        foreach (Transform cylinder in counter.Keys) {
            int count = counter[cylinder];
            maxCount = Mathf.Max(count, maxCount);
            totalHits += count;
        }

        // float totalExposure = 0.0f;

        foreach (Transform cylinder in counter.Keys) {
            int count = counter[cylinder];
            float exposure = ((float)count) / maxCount;

            // int branchOrder = treeImporter.GetBranchOrder(cylinder.gameObject);
            Color branchColor = treeImporter.GetBranchColor(cylinder.gameObject);
            // Color color = Color.Lerp(branchColor, Color.white, exposure);
            Color color = Color.Lerp(Color.black, branchColor, 0.5f + (exposure / 2f));

            // Color color = new Color(exposure, exposure, exposure);
            // Color color = GetRainbowColor(branchOrder % 4, 4);
            cylinder.GetComponent<MeshRenderer>().material.color = color;

            if (!set.Contains(cylinder)) {
                set.Add(cylinder);
            }
        }


    }

    void getTotalExposure() {

    }
}
