using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System;

public class TreeImporter : MonoBehaviour
{
    // public string path;
    // public Object csv;
    public GameObject cylinderPrefab;
    public GameObject treeDestination;
    public Dictionary<GameObject, HashSet<GameObject>> tree = new Dictionary<GameObject, HashSet<GameObject>>();

    // Start is called before the first frame update
    void Start() {
        // string path = AssetDatabase.GetAssetPath(csv);
        // string path = Application.persistentDataPath(csv);
        // string path = Application.persistentDataPath + "/Cylinders.csv";
        string path = Application.persistentDataPath + "/CylindersFull.csv";
        Debug.Log(path);
        List<List<float>> cylinderData = ParseCylinderCSV(path);
        PlaceCylinders(cylinderData, treeDestination);
        // PlaceMultipleCylinders(cylinderData, treeDestination);
    }

    // Update is called once per frame
    void Update() {
        // BuildParents();
        if (Input.GetMouseButtonDown(0)) {
            GameObject clicked = GetClickedOnObject();
            if (clicked != null) {
                HideChildren(clicked);
            }
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            SetActiveOrders(currentOrder, false);
            currentOrder = System.Math.Max(0, currentOrder - 1);
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            SetActiveOrder(currentOrder, true);
            currentOrder = System.Math.Min(currentOrder + 1, orders.Count - 1);
        }
        if (Input.GetKeyDown(KeyCode.U)) {
            UnhideTree();
        }
    }

    public GameObject GetClickedOnObject() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            return hit.transform.gameObject;
        }
        return null;
    }

    private void PlaceMultipleCylinders(List<List<float>> cylinderData, GameObject parent) {
        List<Vector3> axises = new List<Vector3>();
        // axises.Add(new Vector3(0, 0, 0));
        axises.Add(new Vector3(1, 0, 0));
        axises.Add(new Vector3(0, 1, 0));
        axises.Add(new Vector3(0, 0, 1));
        axises.Add(new Vector3(1, 1, 0));
        axises.Add(new Vector3(0, 1, 1));
        axises.Add(new Vector3(1, 0, 1));
        axises.Add(new Vector3(1, 1, 1));
        axises.Add(new Vector3(-1, 0, 0)); // 8
        axises.Add(new Vector3(0, -1, 0));
        axises.Add(new Vector3(0, 0, -1)); // ok
        axises.Add(new Vector3(-1, -1, 0));
        axises.Add(new Vector3(0, -1, -1));
        axises.Add(new Vector3(-1, 0, -1));
        axises.Add(new Vector3(-1, -1, -1));
        axises.Add(new Vector3(1, -1, 0));
        axises.Add(new Vector3(0, 1, -1));
        axises.Add(new Vector3(1, 0, -1));
        axises.Add(new Vector3(-1, 1, 0));
        axises.Add(new Vector3(0, -1, 1));
        axises.Add(new Vector3(-1, 0, 1));
        axises.Add(new Vector3(1, -1, -1));
        axises.Add(new Vector3(-1, 1, -1));
        axises.Add(new Vector3(-1, -1, 1));
        axises.Add(new Vector3(-1, 1, 1));
        axises.Add(new Vector3(1, -1, 1));
        axises.Add(new Vector3(1, 1, -1));



        int k = 0;
        foreach (Vector3 axis in axises) {
            GameObject tree = Object.Instantiate(parent);
            // PlaceCylinders(cylinderData, tree, axis);
            tree.transform.position += Vector3.up * k * 8;

            k++;
        }
    }

    List<GameObject> cylinders = new List<GameObject>();
    List<int> parentIds = new List<int>();

    private void PlaceCylinders(List<List<float>> cylinderData, GameObject parent) {
        // Vector3 axis = Vector3.up;
        Vector3 axis = new Vector3(-1, 0, 0);
        // float n = cylinderData.Count;
        // float averagePos = Vector3.zero;
        int id = 0;
        //- List<GameObject> cylinders = new List<GameObject>();
        //- List<int> parentIds = new List<int>();
        foreach (List<float> parameters in cylinderData) {
            float radius = parameters[0];
            float diameter = 2 * radius;
            float length = parameters[1];

            float posX = parameters[2];
            float posZ = parameters[3];
            float posY = parameters[4];

            float axisX = parameters[5];
            float axisZ = parameters[6];
            float axisY = parameters[7];

            int parentId = (int) parameters[8];

            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 dir = new Vector3(axisX, axisY, axisZ);
            // Quaternion rotation = Quaternion.LookRotation(axis, dir);
            Quaternion rotation = Quaternion.LookRotation(dir) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            Vector3 scale = new Vector3(diameter, length * 0.5f, diameter);

            GameObject cylinder = Object.Instantiate(cylinderPrefab, position, rotation, parent.transform);
            cylinder.transform.localScale = scale;

            cylinders.Add(cylinder);
            parentIds.Add(parentId);

            id++;
        }
        // parent.transform.CenterOnChildred();
        CenterOnChildred(parent.transform);
        parent.transform.position = Vector3.zero;
        parent.transform.localScale *= 3;

        // build parents
        for (int k = 1; k < cylinders.Count; k++) {
            int parentId = parentIds[k] - 1;
            if (parentId < 0) continue;
            GameObject cylinder = cylinders[k];
            GameObject parentCylinder = cylinders[parentId];

            HashSet<GameObject> children;
            if (tree.ContainsKey(parentCylinder)) {
                children = tree[parentCylinder];
            }
            else {
                children = new HashSet<GameObject>(); 
            }

            children.Add(cylinder);
            tree[parentCylinder] = children;

            //- cylinder.transform.parent = parentCylinder.transform;
            Vector3 lookPos = parentCylinder.transform.position - cylinder.transform.position;
            // cylinder.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookPos);
            //- cylinder.transform.rotation = Quaternion.LookRotation(lookPos, Vector3.up) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            // cylinder.transform.LookAt(parentCylinder.transform, Vector3.left);
        }

        BuildOrders(cylinders[0]);
        currentOrder = orders.Count - 1;
    }

    public List<GameObject> FindChildren(GameObject cylinder) {
        List<GameObject> progeny = new List<GameObject>();

        return FindChildren(cylinder, progeny);
    }

    public List<GameObject> FindChildren(GameObject cylinder, List<GameObject> progeny) {
        progeny.Add(cylinder);

        // preprocess and build a tree data structure (already did this)
        HashSet<GameObject> children;
        if (tree.ContainsKey(cylinder)) {
            children = tree[cylinder];
        }
        else {
            children = new HashSet<GameObject>();
        }

        foreach (GameObject child in children) {
            progeny = FindChildren(child, progeny);
        }


        return progeny;
    }

    public int currentOrder = 0;
    private List<HashSet<GameObject>> orders = new List<HashSet<GameObject>>();
    private void BuildOrders(GameObject root) {
        BuildOrders(root, 0);
    }
    private void BuildOrders(GameObject root, int depth) {
        HashSet<GameObject> order;
        if (orders.Count > depth) {
            order = orders[depth];
        }
        else {
            order = new HashSet<GameObject>();
            orders.Add(order);
        }
        HashSet<GameObject> children; // I really need a getordefault method
        if (tree.ContainsKey(root)) {
            children = tree[root];
        }
        else {
            children = new HashSet<GameObject>();
        }

        foreach (GameObject child in children) {
            order.Add(child);
            BuildOrders(child, depth + 1);
        }
    }

    public void HideChildren(GameObject cylinder) {
        List<GameObject> progeny = FindChildren(cylinder);

        foreach (GameObject child in progeny) {
            SetHidden(child, false);
        }
    }

    public void SetActiveOrders(int startOrder, bool active) {
        for (int k = startOrder; k < orders.Count; k++) {
            HashSet<GameObject> progeny = orders[k];

            foreach (GameObject child in progeny) {
                SetHidden(child, active);
            }
        }
    }
    public void SetActiveOrder(int startOrder, bool active) {
        HashSet<GameObject> progeny = orders[startOrder];

        foreach (GameObject child in progeny) {
            SetHidden(child, active);
        }
    }

    public void UnhideTree() {
        foreach (GameObject cylinder in cylinders) {
            SetHidden(cylinder, true);
        }
    }

    public void SetHidden(GameObject go, bool hidden) {
        go.GetComponent<Collider>().enabled = hidden;
        go.SetActive(hidden);
    }

    /*
    public void BuildParents() {
        // build parents
        for (int k = 1; k < cylinders.Count; k++) {
            int parentId = parentIds[k] - 1;
            if (parentId < 0) continue;
            GameObject cylinder = cylinders[k];
            GameObject parentCylinder = cylinders[parentId];
            //- cylinder.transform.parent = parentCylinder.transform;
            Vector3 lookPos = parentCylinder.transform.position - cylinder.transform.position;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, lookPos);
            Debug.DrawRay(cylinder.transform.position, lookPos);
            // Debug.DrawRay(cylinder.transform.position, rotation);
            // cylinder.transform.rotation = rotation;
            // cylinder.transform.LookAt(parentCylinder.transform, Vector3.left);
        }
    }
    */

    public static void CenterOnChildred(Transform aParent) {
        // var childs = aParent.Cast<Transform>().ToList();
        // var childs = new List<Transform>();
        // foreach (Transform child in transform)
        // childs.Add(child);
        var childs = GetAllChilds(aParent);

        var pos = Vector3.zero;
        foreach (var C in childs) {
            pos += C.position;
            C.parent = null;
        }
        pos /= childs.Count;
        aParent.position = pos;
        foreach (var C in childs)
            C.parent = aParent;
    }

    public static List<Transform> GetAllChilds(Transform Go) {
        List<Transform> list = new List<Transform>();
        for (int i = 0; i < Go.childCount; i++) {
            list.Add(Go.GetChild(i));
        }
        return list;
    }

    private List<List<float>> ParseCylinderCSV(string path) {
        string fileData = System.IO.File.ReadAllText(path);
        string[] lines = fileData.Split("\n"[0]);
        string[] lineData = (lines[0].Trim()).Split(","[0]);
        List<List<float>> cylinderData = new List<List<float>>();
        foreach (string line in lines) {
            string[] words = (line.Trim()).Split(","[0]);
            if (words.Length >= 8) {
                List<float> cylinderParameters = new List<float>();
                foreach (string word in words) {
                    float parameter = float.Parse(word);
                    cylinderParameters.Add(parameter);
                }
                // Debug.Log(cylinderParameters.Count);
                cylinderData.Add(cylinderParameters);
            }
        }
        // var x : float;
        // float.TryParse(lineData[0], x);
        // Debug.Log(lineData.Length);
        return cylinderData;
    }

    private List<List<float>> ParseCylinderEulerCSV(string path) {
        string fileData = System.IO.File.ReadAllText(path);
        string[] lines = fileData.Split("\n"[0]);
        string[] lineData = (lines[0].Trim()).Split(","[0]);
        List<List<float>> cylinderData = new List<List<float>>();
        foreach (string line in lines) {
            string[] words = (line.Trim()).Split(","[0]);
            if (words.Length == 8) {
                List<float> cylinderParameters = new List<float>();
                foreach (string word in words) {
                    float parameter = float.Parse(word);
                    cylinderParameters.Add(parameter);
                }
                // Debug.Log(cylinderParameters.Count);
                cylinderData.Add(cylinderParameters);
            }
        }
        // var x : float;
        // float.TryParse(lineData[0], x);
        // Debug.Log(lineData.Length);
        return cylinderData;
    }

    private void PlaceCylindersEuler(List<List<float>> cylinderData, GameObject parent) {
        // float n = cylinderData.Count;
        // float averagePos = Vector3.zero;
        int id = 0;
        foreach (List<float> parameters in cylinderData) {
            float radius = parameters[0];
            float diameter = 2 * radius;
            float length = parameters[1];

            float posX = parameters[2];
            float posZ = parameters[3];
            float posY = parameters[4];

            float axisX = parameters[5];
            float axisZ = parameters[6];
            float axisY = parameters[7];
            
            Vector3 position = new Vector3(posX, posY, posZ);
            Quaternion rotation = Quaternion.Euler(axisX, axisY, axisZ);
            Vector3 scale = new Vector3(diameter, length, diameter);

            GameObject cylinder = Object.Instantiate(cylinderPrefab, position, rotation, parent.transform);
            cylinder.transform.localScale = scale;

            id++;
        }
        // parent.transform.CenterOnChildred();
        CenterOnChildred(parent.transform);
        parent.transform.position = Vector3.zero;
        parent.transform.localScale *= 3;
    }

}
