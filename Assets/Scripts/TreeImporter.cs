using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System;
using System.Linq;

public class Sphere {
    public Vector3 point;
    public float radius;
    public GameObject gameObject;

    public Sphere() : this(Vector3.zero, 1f) {

    }

    public Sphere(Vector3 point, float radius) {
        this.point = point;
        this.radius = radius;
    }

    public Transform transform {
        get {
            return this.gameObject.transform;
        }
    }
    public float diamater {
        get {
            return radius * 2;
        }
    }

    public GameObject Instantiate(GameObject prefab) {
        gameObject = Object.Instantiate(prefab, point, Quaternion.identity);
        gameObject.transform.localScale = Vector3.one * diamater; // 2f
        return gameObject;
    }

    public void Recalculate() {
        if (gameObject == null) return;
        transform.position = point;
        gameObject.transform.localScale = Vector3.one * diamater; // 2f
    }

    public void Copy(Sphere sphere) {
        this.point = sphere.point;
        this.radius = sphere.radius;
        Recalculate();
    }
}

public class TreeImporter : MonoBehaviour
{
    public class Cylinder {
        public GameObject gameObject;
        // public GameObject parent;
        float radius;
        float length;
        public int id;
        public int parentId;
        public int branchOrder;
        public bool isWaterSprout;
        public HashSet<GameObject> leaves;
        public List<float> parameters;
        public bool wasPruned;

        public Cylinder(GameObject cylinder, TreeImporter treeImporter, float radius, float length, int id, int parentId, int branchOrder, bool isWaterSprout, List<float> parameters, bool isPruned) {
            this.gameObject = cylinder;
            this.radius = radius;
            this.length = length;
            this.id = id;
            this.parentId = parentId;
            this.branchOrder = branchOrder;
            this.isWaterSprout = isWaterSprout;
            this.parameters = parameters;

            if (isWaterSprout) {
                cylinder.GetComponent<MeshRenderer>().material = treeImporter.waterSproutMat;
            }

            // cylinder.GetComponent<MeshRenderer>().material.color = color();
            Color branchColor = color();
            Color colorLerp = Color.Lerp(Color.black, branchColor, 0.5f);
            cylinder.GetComponent<MeshRenderer>().material.color = colorLerp;

            this.wasPruned = isPruned;
        }

        // look up C# computed properties if you're confused about how this works
        public Vector3 position {
            get {
                return this.gameObject.transform.position;
            }
        }
        public Quaternion rotation {
            get {
                return this.gameObject.transform.rotation;
            }
        }
        public Transform transform {
            get {
                return this.gameObject.transform;
            }
        }
        public float diamater {
            get {
                return radius * 2;
            }
        }
        public float volume {
            get {
                // V = π r^2 h
                return Mathf.PI * radius * radius * length;
            }
        }
        public bool isPruned {
            get {
                return !gameObject.activeSelf;
            }
        }
        public string csv {
            get {
                string result = "";
                int pruned = this.isPruned ? 1 : 0;
                if (parameters.Count > 19) { // this stuff is gross and exists to support legacy cylinder files
                    parameters[19] = pruned; // update isPruned column
                }
                foreach (float parameter in parameters) {
                    result += parameter + ",";
                }
                if (parameters.Count <= 19) {
                    result += pruned + ",";
                }
                // remove trailing comma
                result = result.TrimEnd(',');
                return result;
            }
        }

        // this should be a computed property
        public Color color() {
            if (isWaterSprout) {
                return Color.white;
            }

            return GetRainbowColor(branchOrder % 6, 6);
        }

        public void AddLeaf(GameObject leaf) {
            leaves.Add(leaf);
        }

        public HashSet<GameObject> GetLeaves() {
            return leaves;
        }

        public void SetHidden(bool hidden) {
            gameObject.GetComponent<Collider>().enabled = hidden;
            gameObject.SetActive(hidden);
            //- Debug.Log("queued");
            //- quantifier.QueueTreeUpdate();
        }
    }

    public class Leaf {
        public GameObject gameObject;
        public Cylinder cylinder;
        Vector3 baseScale;
        public Leaf(GameObject leaf, Cylinder cylinder) {
            this.gameObject = leaf;
            this.cylinder = cylinder;
            this.baseScale = leaf.transform.localScale;
            leaf.GetComponent<MeshRenderer>().material.color = cylinder.color();
        }

        public void SetPercentScale(float percent) {
            // this.gameObject.transform.localScale = Vector3.one * scale * percent;
            this.gameObject.transform.localScale = baseScale * percent;
        }

        /*
        public void SetHidden(bool hidden) {
            gameObject.GetComponent<Collider>().enabled = hidden;
            gameObject.SetActive(hidden);
            // this needs to queue a tree update
            //- Debug.Log("queued");
            //- quantifier.QueueTreeUpdate();
        }
        */

        public void SetRendering(bool hidden) {
            // gameObject.GetComponent<Collider>().enabled = hidden;
            // gameObject.SetActive(hidden);
            gameObject.GetComponent<MeshRenderer>().enabled = hidden;
        }
    }

    public class Tree {
        /*
        List<GameObject> cylinders;
        List<GameObject> leaves;
        Dictionary<GameObject, Cylinder> go2cylinder;
        Dictionary<GameObject, Leaf> go2leaf;

        public Tree(List<GameObject> cylinders, List<GameObject> leaves, Dictionary<GameObject, Cylinder> go2cylinder, Dictionary<GameObject, Leaf> go2leaf) {
            this.cylinders = cylinders;
            this.leaves = leaves;
            this.go2cylinder = go2cylinder;
            this.go2leaf = go2leaf;
        }
        */
        List<Cylinder> cylinders;
        List<Leaf> leaves;

        public Tree(List<Cylinder> cylinders, List<Leaf> leaves) {
            this.cylinders = cylinders;
            this.leaves = leaves;
        }

        public Tree(string csv) {
            Import(csv);
        }

        public void SetLeafPercentScale(float percent) {
            foreach (Leaf leaf in leaves) {
                leaf.SetPercentScale(percent);
            }
        }

        public void Import(string csv) {

        }

        public string Export() {
            string lines = "";

            foreach (Cylinder cylinder in cylinders) {
                lines += cylinder.csv;
                // lines += cylinder.isPruned ? 1 : 0;
                lines += "\n";
            }

            return lines.Trim();
        }

        public void ExportToClipboard() {
            string csv = Export();
            GUIUtility.systemCopyBuffer = csv;
        }

        /*
        public void HideLeaves(bool hidden) {
            foreach (GameObject leaf in leaves) {
                leaf.SetHidden(hidden);
            }
        }

        bool areLeavesHidden = false;
        public void ToggleLeaves() {
            areLeavesHidden = !areLeavesHidden;
            HideLeaves(areLeavesHidden);
        }
        
        public void Unhide() {
            foreach (GameObject cylinder in cylinders) {
                SetHidden(cylinder, true);
            }
        }
        */

        public void Unhide() {
            foreach (Cylinder cylinder in cylinders) {
                cylinder.SetHidden(true);
            }
        }

        public void ReloadPruning() {
            foreach (Cylinder cylinder in cylinders) {
                cylinder.SetHidden(!cylinder.wasPruned);
            }
        }

        public void SetAllVisibleLeaves(bool hidden) {
            foreach (Leaf leaf in leaves) {
                leaf.SetRendering(hidden);
            }
        }

        public void SetPercentVisibleLeaves(float percent) {
            SetAllVisibleLeaves(true);

            foreach (Leaf leaf in leaves) {
                // get a random number
                float roll = Random.Range(0f, 1f);
                if (roll > percent) {
                    leaf.SetRendering(false);
                }
            }
        }
    }

    // public string path;
    // public Object csv;
    public GameObject cylinderPrefab;
    public GameObject leafPrefab;
    public GameObject treeDestination;
    public Material waterSproutMat;
    private Vector3 treeDestinationPos;
    private LightQuantifier quantifier;

    private GameObject empty;
    public Tree tree;
    public Dictionary<GameObject, HashSet<GameObject>> childMap = new Dictionary<GameObject, HashSet<GameObject>>();
    public Dictionary<GameObject, Cylinder> go2cylinder = new Dictionary<GameObject, Cylinder>(); // data
    public Dictionary<GameObject, Leaf> go2leaf = new Dictionary<GameObject, Leaf>(); // data

    // Start is called before the first frame update
    void Start() {
        quantifier = gameObject.GetComponent<LightQuantifier>();
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
            // UnhideTree();
            tree.Unhide();
        }
        if (Input.GetKeyDown(KeyCode.Y)) { // || OVRInput.GetDown(OVRInput.Button.Two)) {
            tree.ReloadPruning();
        }
        if (Input.GetKeyDown(KeyCode.L)) { // || OVRInput.GetDown(OVRInput.Button.Two)) {
            ToggleLeaves();
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) {
            tree.ExportToClipboard();
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

    /*
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
            childMap.transform.position += Vector3.up * k * 8;

            k++;
        }
    }
    */

    List<Cylinder> cylinders2 = new List<Cylinder>(); // this should be the main list that gets used
    List<GameObject> cylinders = new List<GameObject>(); // this should become deprecated
    List<Leaf> leaves2 = new List<Leaf>();
    List<GameObject> leaves = new List<GameObject>();
    List<int> parentIds = new List<int>();
    List<int> branchOrders = new List<int>();

    /*
    private string MakeCSVLine(List<float> parameters) {
        string result = "";
        foreach (float parameter in parameters) {
            result += parameter + ",";
        }
        return result;
    }
    */

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

            int parentId = (int)parameters[8];
            int branchOrder = (int)parameters[15];

            bool isWaterSprout = (parameters[17] == 0 ? false : true); // column R
            // float waterSproutScore = parameters[18];
            bool isPruned = false;
            if (parameters.Count > 19) {
                isPruned = (parameters[19] == 0 ? false : true);
            }

            Vector3 position = new Vector3(posX, posY, posZ);
            Vector3 dir = new Vector3(axisX, axisY, axisZ);
            // Quaternion rotation = Quaternion.LookRotation(axis, dir);
            Quaternion rotation = Quaternion.LookRotation(dir) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            Vector3 scale = new Vector3(diameter, length * 0.5f * 2, diameter);

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
            Cylinder datum = new Cylinder(cylinder, this, radius, length, id, parentId, branchOrder, isWaterSprout, parameters, isPruned);
            go2cylinder[cylinder] = datum;
            cylinders2.Add(datum);




            // leaves generation from here
            int density = 1; // 10
            for (int i = 0; i < density; i++) {
                float a = Random.Range(-0.1f, 0.1f);
                float b = Random.Range(-0.1f, 0.1f);
                float c = Random.Range(-0.1f, 0.1f);
                float d = Random.Range(-0.1f, 0.1f);

                GameObject leaf = Object.Instantiate(leafPrefab, position, rotation); // true refers to defining to world space coordinate
                //leaf.transform.SetParent(cylinder.transform,false);
                leaf.transform.parent = null;
                leaf.transform.parent = cylinder.transform;


                float distance = length * ((float)i / density);
                //Vector3 scale_new = new Vector3(diameter*20, diameter *20, diameter*20);
                //leaf.transform.localScale = scale_new;


                Vector3 newPosition = position + rotation * Vector3.up * distance;
                leaf.transform.position = newPosition;
                Vector3 randomPosition = newPosition;
                randomPosition.x += d;
                randomPosition.y += d;
                randomPosition.z += d;
                leaf.transform.position = randomPosition;

                float deviatioin = Vector3.Distance(position, randomPosition) / (float)0.173205 / 2;
                float fromCenter = Vector3.Distance(new Vector3(0.55f, -1.582687f, -0.3488998f), randomPosition) / 10 - (float)62.4;
                //Mathf.Pow(branchOrder,(float)1.0)*
                float probablity = (fromCenter / 3) * 200;
                //leaf.transform.localScale = new Vector3 (probablity,probablity/(float)6.25,probablity);
                float deviationSize = (Gaussian(d) + 10) / 10;
                // float grandScale = 0.005f;
                // float grandScale = 0.000025f;
                float grandScale = 0.000025f;

                float x = grandScale * probablity * deviationSize;
                float y = grandScale * probablity * deviationSize;
                float z = grandScale * probablity * deviationSize;
                x /= cylinder.transform.localScale.x;
                y /= cylinder.transform.localScale.y;
                z /= cylinder.transform.localScale.z;

                leaf.transform.localScale = new Vector3(x, y, z);
                // Debug.Log("|leaf|: grandScale = " + grandScale + ", fromCenter = " + fromCenter + ", probablity = " + probablity + ", deviationSize = " + deviationSize + ", sphereSize = " + x);
                //- leaf.GetComponent<Renderer>().material.color = Color.green;
                //- leaf.GetComponent<Renderer>().material.color = datum.color();
                //var col = leaf.GetComponent<Renderer> ().material.GetColor("_TintColor");
                //col.a = 0.5f;

                leaves.Add(leaf);

                Leaf leafDatum = new Leaf(leaf, datum);
                go2leaf[leaf] = leafDatum;
                leaves2.Add(leafDatum);
            }
            // leaves generation ends here

            if (isPruned) {
                SetHidden(cylinder, false);
            }

            id++;
        }
        // parent.transform.CenterOnChildred();
        CenterOnChildred(parent.transform);
        // parent.transform.position = Vector3.zero;
        parent.transform.position = treeDestinationPos;
        parent.transform.localScale *= 3;

        // make tree objects
        // tree = new Tree(cylinders, leaves, go2cylinder, go2leaf);
        tree = new Tree(cylinders2, leaves2);

        // build parents
        for (int k = 1; k < cylinders.Count; k++) {
            int parentId = parentIds[k] - 1;
            if (parentId < 0) continue;
            GameObject cylinder = cylinders[k];
            GameObject parentCylinder = cylinders[parentId];

            HashSet<GameObject> children;
            if (childMap.ContainsKey(parentCylinder)) {
                children = childMap[parentCylinder];
            }
            else {
                children = new HashSet<GameObject>(); 
            }

            children.Add(cylinder);
            childMap[parentCylinder] = children;

            //- cylinder.transform.parent = parentCylinder.transform;
            Vector3 lookPos = parentCylinder.transform.position - cylinder.transform.position;
            // cylinder.transform.rotation = Quaternion.LookRotation(Vector3.forward, lookPos);
            //- cylinder.transform.rotation = Quaternion.LookRotation(lookPos, Vector3.up) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
            // cylinder.transform.LookAt(parentCylinder.transform, Vector3.left);
        }

        // ColorBranches();
        BuildOrders(cylinders[0]);
        currentOrder = orders.Count - 1;

        tree.SetPercentVisibleLeaves(.1f);
    }

    private void AddJoint(GameObject a, GameObject parent, Vector3 point) {

    }

    /*
     * Returns a value between 0 and 1
     */
    private float calculateLeafScale(float t) {
        return 0;
    }

    public float GetPercentEnclosed(Sphere sphere) {
        return GetPercentEnclosed((from item in cylinders2
                          where item.gameObject.active == true
                          select item).ToList(), sphere);
    }

    public float GetPercentEnclosed(List<Cylinder> cylinders, Sphere sphere) {
        int n = cylinders2.Count; // I apologize
        float total = 0;

        foreach (Cylinder cylinder in cylinders) {
            float distance = (sphere.point - cylinder.position).magnitude;
            if (distance <= sphere.radius) {
                total++;
            }
        }

        return total / n;
    }

    public Sphere FitSphere() {
        return FitSphere((from item in cylinders2
                          where item.gameObject.active == true
                          select item).ToList());
    }

    private Vector3 GetCentroid(List<Cylinder> cylinders) {
        int n = cylinders.Count;
        Vector3 total = Vector3.zero;
        foreach (Cylinder cylinder in cylinders) {
            total += cylinder.position;
        }
        return total / n;
    }

    private float GetAverageRadius(List<Cylinder> cylinders, Vector3 origin) {
        int n = cylinders.Count;
        double total = 0;
        foreach (Cylinder cylinder in cylinders) {
            total += (origin - cylinder.position).magnitude;
        }
        return (float) total / n;
    }

    private Vector3 GetWeightedCentroid(List<Cylinder> cylinders) {
        float n = (float) GetTotalVolume(cylinders);
        Vector3 total = Vector3.zero;
        foreach (Cylinder cylinder in cylinders) {
            float weight = cylinder.volume / n;
            total += cylinder.position * weight;
        }
        return total;
    }

    private double GetTotalVolume(List<Cylinder> cylinders) {
        double total = 0;
        foreach (Cylinder cylinder in cylinders) {
            total += cylinder.volume;
        }
        return total;
    }

    private float GetWeightedAverageRadius(List<Cylinder> cylinders, Vector3 origin) {
        double n = GetTotalVolume(cylinders);
        double total = 0;
        foreach (Cylinder cylinder in cylinders) {
            double weight = cylinder.volume / n;
            total += (origin - cylinder.position).magnitude * weight;
        }
        return (float) total;
    }

    public Sphere FitSphere(List<Cylinder> cylinders) {
        // calculate the centroid and then the average radius
        float n = cylinders.Count;
        Vector3 total = Vector3.zero;
        Vector3 centroid = GetWeightedCentroid(cylinders);
        float radius = GetWeightedAverageRadius(cylinders, centroid);

        return new Sphere(centroid + Vector3.up * 0, radius);
    }

    public Sphere FitSphere(List<Vector3> points) {



        float radius = 1;
        float x = 0;
        float y = 0;
        float z = 0;
        Vector3 point = new Vector3(x, y, z);
        return new Sphere(point, radius);
    }

    public static float Gaussian(float deviatioin) {
        float v1, v2, s;
        do {
            //v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
            //v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
            v1 = deviatioin;
            v2 = deviatioin;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

        return v1 * s;
    }

    public List<GameObject> FindChildren(GameObject cylinder) {
        List<GameObject> progeny = new List<GameObject>();

        return FindChildren(cylinder, progeny);
    }

    public List<GameObject> FindChildren(GameObject cylinder, List<GameObject> progeny) {
        progeny.Add(cylinder);

        // preprocess and build a tree data structure (already did this)
        HashSet<GameObject> children;
        if (childMap.ContainsKey(cylinder)) {
            children = childMap[cylinder];
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
        if (childMap.ContainsKey(root)) {
            children = childMap[root];
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
        // quantifier.UpdateTree();
        Debug.Log("queued");
        quantifier.QueueTreeUpdate();
    }

    public void HideLeaves(bool hidden) {
        foreach (GameObject leaf in leaves) {
            SetHidden(leaf, hidden);
        }
    }

    bool areLeavesHidden = false;
    public void ToggleLeaves() {
        areLeavesHidden = !areLeavesHidden;
        HideLeaves(areLeavesHidden);
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
        Cylinder datum = go2cylinder[cylinder];
        return datum.branchOrder;
    }

    public Color GetBranchColor2(GameObject cylinder) {
        // should add null check
        Cylinder datum = go2cylinder[cylinder];

        /*if (datum.isWaterSprout) {
            return Color.white;
        }*/

        return datum.color();
    }

    public Color GetBranchColor(GameObject cylinder) {
        if (!go2leaf.ContainsKey(cylinder)) {
            return Color.black;
        }

        Leaf datum = go2leaf[cylinder];

        return datum.cylinder.color();
    }

    private List<List<float>> ParseCylinderCSV(string path) {
        string fileData = System.IO.File.ReadAllText(path);
        return ParseCylinderCSVString(fileData);
    }

    private List<List<float>> ParseCylinderCSV() {
        // string fileData = Constants.CylindersFull1a; // CylindersFull1a, CylindersFull3a, CylindersFull1aPruned
        string fileData = Constants.CylindersFull1a;
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
