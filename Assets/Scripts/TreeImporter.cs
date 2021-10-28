using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System;

public class TreeImporter : MonoBehaviour
{
    public class Cylinder {
        public GameObject gameObject;
        // public GameObject parent;
        public int id;
        public int parentId;
        public int branchOrder;
        public bool isWaterSprout;

        public Cylinder(GameObject cylinder, TreeImporter treeImporter, int id, int parentId, int branchOrder, bool isWaterSprout) {
            this.gameObject = cylinder;
            this.id = id;
            this.parentId = parentId;
            this.branchOrder = branchOrder;
            this.isWaterSprout = isWaterSprout;

            if (isWaterSprout) {
                cylinder.GetComponent<MeshRenderer>().material = treeImporter.waterSproutMat;
            }

            // cylinder.GetComponent<MeshRenderer>().material.color = color();
            Color branchColor = color();
            Color colorLerp = Color.Lerp(Color.black, branchColor, 0.5f);
            cylinder.GetComponent<MeshRenderer>().material.color = colorLerp;
        }

        public Color color() {
            if (isWaterSprout) {
                return Color.white;
            }

            return GetRainbowColor(branchOrder % 6, 6);
        }
    }

    // public string path;
    // public Object csv;
    public GameObject cylinderPrefab;
    public GameObject treeDestination;
    public Material waterSproutMat;
    private Vector3 treeDestinationPos;
    private GameObject empty;
    public Dictionary<GameObject, HashSet<GameObject>> tree = new Dictionary<GameObject, HashSet<GameObject>>();
    public Dictionary<GameObject, Cylinder> cylinderMap = new Dictionary<GameObject, Cylinder>(); // data

    // Start is called before the first frame update
    void Start() {
        empty = Object.Instantiate(treeDestination);
        // dimensions are (1.0, 2.0, 1.0)
        Debug.Log("dims = " + cylinderPrefab.GetComponent<MeshRenderer>().bounds.size);
        treeDestinationPos = treeDestination.transform.position;
        // string path = AssetDatabase.GetAssetPath(csv);
        // string path = Application.persistentDataPath(csv);
        // string path = Application.persistentDataPath + "/Cylinders.csv";
        string path = Application.persistentDataPath + "/CylindersFull.csv";
        Debug.Log(path);
        //- List<List<float>> cylinderData = ParseCylinderCSV(path);
        List<List<float>> cylinderData = ParseCylinderCSV();
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
            PreviousBranchOrder();
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            NextBranchOrder();
        }
        if (Input.GetKeyDown(KeyCode.U)) { // || OVRInput.GetDown(OVRInput.Button.Two)) {
            UnhideTree();
        }
    }

    public void NextBranchOrder() {
        SetActiveOrder(currentOrder, true);
        currentOrder = System.Math.Min(currentOrder + 1, orders.Count - 1);
    }

    public void PreviousBranchOrder() {
        SetActiveOrders(currentOrder, false);
        currentOrder = System.Math.Max(0, currentOrder - 1);
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
    List<int> branchOrders = new List<int>();

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
            int branchOrder = (int) parameters[15];

            bool isWaterSprout = (parameters[17] == 0 ? false : true); // column R
            // float waterSproutScore = parameters[18];

            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 dir = new Vector3(axisX, axisY, axisZ);
            // Quaternion rotation = Quaternion.LookRotation(axis, dir);
            Quaternion rotation = Quaternion.LookRotation(dir) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            Vector3 scale = new Vector3(diameter, length * 0.5f, diameter);

            // GameObject cylinder = Object.Instantiate(cylinderPrefab, position, rotation, parent.transform);

            /*
            GameObject tempParent = Object.Instantiate(empty, position, rotation, parent.transform);
            Vector3 newOrigin = new Vector3(0, -1, 0);
            // GameObject cylinder = Object.Instantiate(cylinderPrefab, newOrigin, Quaternion.LookRotation(Vector3.forward), tempParent.transform);
            GameObject cylinder = Object.Instantiate(cylinderPrefab, position, rotation, tempParent.transform);
            cylinder.transform.position += newOrigin;
            cylinder.transform.parent = null;
            cylinder.transform.parent = parent.transform;
            Destroy(tempParent);
            */
            GameObject tempParent = Object.Instantiate(empty, Vector3.zero, Quaternion.Euler(0, 0, 0), parent.transform);
            Vector3 newOrigin = new Vector3(0, length / 2f, 0);
            // GameObject cylinder = Object.Instantiate(cylinderPrefab, newOrigin, Quaternion.LookRotation(Vector3.forward), tempParent.transform);
            // GameObject cylinder = Object.Instantiate(cylinderPrefab, position, rotation, tempParent.transform);
            GameObject cylinder = Object.Instantiate(cylinderPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), tempParent.transform);
            cylinder.transform.position = newOrigin;
            tempParent.transform.position = position;
            tempParent.transform.rotation = rotation;
            cylinder.transform.parent = null;
            cylinder.transform.parent = parent.transform;
            Destroy(tempParent);

            cylinder.transform.localScale = scale;

            cylinders.Add(cylinder);
            parentIds.Add(parentId);
            branchOrders.Add(branchOrder);

            GameObject parentCylinder = (parentId > 0) ? cylinders[parentId - 1] : null;
            if (parentCylinder != null) {
                /*
                // cylinder.AddComponent<Rigidbody>();
                Rigidbody body = cylinder.GetComponent<Rigidbody>();
                body.isKinematic = false;
                HingeJoint joint = cylinder.AddComponent<HingeJoint>();
                joint.autoConfigureConnectedAnchor = false;
                // joint.connectedAnchor = position;
                joint.connectedBody = parentCylinder.GetComponent<Rigidbody>();
                // joint.connectedAnchor = newOrigin;
                // joint.connectedAnchor = parentCylinder.transform.InverseTransformPoint(position);
                joint.connectedAnchor = cylinder.transform.InverseTransformPoint(position);
                joint.useLimits = true;
                */
                /*
                // cylinder.AddComponent<Rigidbody>();
                Rigidbody body = cylinder.GetComponent<Rigidbody>();
                body.isKinematic = false;
                HingeJoint joint = parentCylinder.AddComponent<HingeJoint>();
                joint.autoConfigureConnectedAnchor = false;
                // joint.connectedAnchor = position;
                joint.connectedBody = cylinder.GetComponent<Rigidbody>();
                // joint.connectedAnchor = newOrigin;
                // joint.connectedAnchor = parentCylinder.transform.InverseTransformPoint(position);
                joint.connectedAnchor = cylinder.transform.InverseTransformPoint(position);
                joint.useLimits = true;
                */
            }
           
            /*
            Cylinder datum = new Cylinder();
            datum.gameObject = cylinder;
            datum.id = id;
            datum.parentId = parentId;
            datum.branchOrder = branchOrder;
            */
            Cylinder datum = new Cylinder(cylinder, this, id, parentId, branchOrder, isWaterSprout);           
            cylinderMap[cylinder] = datum;

            id++;
        }
        // parent.transform.CenterOnChildred();
        CenterOnChildred(parent.transform);
        // parent.transform.position = Vector3.zero;
        parent.transform.position = treeDestinationPos;
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

        // ColorBranches();
        BuildOrders(cylinders[0]);
        currentOrder = orders.Count - 1;
    }

    private void AddJoint(GameObject a, GameObject parent, Vector3 point) {

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

    private void ColorBranches() {
        foreach (GameObject cylinder in cylinders) {
            cylinder.GetComponent<MeshRenderer>().material.color = GetBranchColor(cylinder);
        }
    }

    public int currentOrder = 0;
    private List<HashSet<GameObject>> orders = new List<HashSet<GameObject>>();
    private void BuildOrders(GameObject root) {
        // BuildOrders(root, 0);
        BuildOrdersCSV(root, 0);
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

    private void BuildOrdersCSV(GameObject root, int depth) {
        // iterate through all cylinders and put them in their respective branch orders

        int k = 0;
        int max = Mathf.Max(branchOrders.ToArray());
        orders = new List<HashSet<GameObject>>(max + 1);
        for (int i = 0; i <= max; i++) {
            orders.Add(new HashSet<GameObject>());
        }

        foreach (GameObject cylinder in cylinders) {
            root = cylinder;
            int branchOrder = branchOrders[k];
            depth = branchOrder;

            HashSet<GameObject> order = orders[depth];
            order.Add(cylinder);
            k++;
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

    public int GetBranchOrder(GameObject cylinder) {
        // should add null check
        Cylinder datum = cylinderMap[cylinder];
        return datum.branchOrder;
    }

    public Color GetBranchColor(GameObject cylinder) {
        // should add null check
        Cylinder datum = cylinderMap[cylinder];

        /*if (datum.isWaterSprout) {
            return Color.white;
        }*/

        return datum.color();
    }

    private List<List<float>> ParseCylinderCSV(string path) {
        string fileData = System.IO.File.ReadAllText(path);
        return ParseCylinderCSVString(fileData);
    }

    private List<List<float>> ParseCylinderCSV() {
        // Constants constants = transform.GetComponent<Constants>();
        // string fileData = System.IO.File.ReadAllText(path);
        // string fileData = Constants.CylindersFull;
        string fileData = Constants.CylindersFull1a; // CylindersFull1a, CylindersFull3a
        return ParseCylinderCSVString(fileData);
    }

    private List<List<float>> ParseCylinderCSVString(string fileData) {
        //- string fileData = System.IO.File.ReadAllText(path);
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

    /*
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
    */


    static Color GetRainbowColor(int index, int count) {
        float progress = ((float)index) / count;
        float div = (Mathf.Abs(progress % 1) * 6);
        int ascending = (int)((div % 1) * 255);
        int descending = 255 - ascending;

        // Color color = new Color(

        switch ((int)div) {
            case 0:
                return FromArgb(255, 255, ascending, 0);
            case 1:
                return FromArgb(255, descending, 255, 0);
            case 2:
                return FromArgb(255, 0, 255, ascending);
            case 3:
                return FromArgb(255, 0, descending, 255);
            case 4:
                return FromArgb(255, ascending, 0, 255);
            default: // case 5:
                return FromArgb(255, 255, 0, descending);
        }
    }

    static Color FromArgb(int a, int r, int g, int b) {
        return new Color(r / 256f, g / 256f, b / 256f, a / 256f);
    }
}
