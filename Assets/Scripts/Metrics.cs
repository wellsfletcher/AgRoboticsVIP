using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PruningMetric;

namespace PruningMetric {

    public class Metric {
        public Metric() {

        }

        public virtual void Calculate() {

        }

        public virtual bool VisualizationEnabled() {
            return true;
        }

        public virtual void Visualize() {

        }

        public virtual void Hide() {

        }

        public virtual string name() {
            return "Metric";
        }

        public virtual string description() {
            return "Metric for pruning tree.";
        }

        public virtual float baseline() {
            return 0f;
        }

        public virtual float optimal() {
            return 0f;
        }

        public virtual float current() {
            return 0f;
        }

        // public float baseline
        // public float optimmal
        // public float current
    }

    public class SphereMetric : Metric {
        private TreeImporter treeImporter;
        private Sphere sphere;
        // private GameObject prefab;

        public SphereMetric(TreeImporter treeImporter, GameObject spherePrefab) {
            this.treeImporter = treeImporter;
            // this.prefab = spherePrefab;
            this.sphere = new Sphere();
            this.sphere.Instantiate(spherePrefab);
            Hide();
        }

        public override void Calculate() {
            Sphere fittedSphere = treeImporter.FitSphere();
            sphere.Copy(fittedSphere);
            cachedCurrent = treeImporter.GetPercentEnclosed(sphere);
        }

        public override void Visualize() {
            sphere.gameObject.SetActive(true);
        }

        public override void Hide() {
            sphere.gameObject.SetActive(false);
        }

        public override string name() {
            return "Bowl Concavity";
        }

        public override string description() {
            return "The percentage of cylinders that are enclosed by a sphere fitted to the tree.";
        }

        public override float baseline() {
            return 37.990f / 100f;
        }

        public override float optimal() {
            return 0f;
        }

        float cachedCurrent = 1f;
        public override float current() { // percent covered by bowl
            return cachedCurrent;
        }

        // public float baseline
        // public float optimmal
        // public float current
    }

    public class AverageLightExposureMetric : Metric {
        private TreeImporter treeImporter;
        private SunManager sunManager;

        public AverageLightExposureMetric(TreeImporter treeImporter, SunManager sunManager) {
            this.treeImporter = treeImporter;
            this.sunManager = sunManager;
            Hide();
        }

        public override void Calculate() {
            sunManager.Restart();
        }

        public override bool VisualizationEnabled() {
            return false;
        }

        public override void Visualize() {
            // show raycasts
        }

        public override void Hide() {
            // hide raycasts
        }

        public override string name() {
            return "Average Light Exposure";
        }

        public override string description() {
            return "Average percent raycast hits on leaves across all timestamps.";
        }

        public override float baseline() {
            return 81.364f / 100f;
        }

        public override float optimal() {
            return 1f;
        }

        public override float current() {
            return sunManager.CalculateAverageLightExposure();
        }

        // public float baseline
        // public float optimmal
        // public float current
    }

    public class ConvexHullMetric : Metric {
        private TreeImporter treeImporter;
        private Sphere sphere;
        // private GameObject prefab;

        public ConvexHullMetric(TreeImporter treeImporter, GameObject spherePrefab) {
            this.treeImporter = treeImporter;
            // this.prefab = spherePrefab;
            this.sphere = new Sphere();
            this.sphere.Instantiate(spherePrefab);
            Hide();
        }

        public override void Calculate() {
            Sphere fittedSphere = treeImporter.FitSphere();
            sphere.Copy(fittedSphere);
            cachedCurrent = treeImporter.GetPercentEnclosed(sphere);
        }

        public override void Visualize() {
            sphere.gameObject.SetActive(true);
        }

        public override void Hide() {
            sphere.gameObject.SetActive(false);
        }

        public override string name() {
            return "Density";
        }

        public override string description() {
            return "How much of tree's convex hull is occupied by cylinders.";
        }

        public override float baseline() {
            return 0f;
        }

        public override float optimal() {
            return 0f;
        }

        float cachedCurrent = 1f;
        public override float current() {
            return cachedCurrent;
        }
    }

    public class HollisticMetric : Metric {
        private Metric[] metrics;

        public HollisticMetric(Metric[] metrics) {
            this.metrics = metrics;
            Hide();
        }

        public override void Calculate() {
            float[] weights = { 0.5f, 0.5f };
            float result = 0;
            for (int k = 0; k < metrics.Length; k++) {
                Metric metric = metrics[k];
                float weight = 0;
                if (k < weights.Length) {
                    weight = weights[k];
                }
                //- metric.Calculate(); // this is asynchronous
                result += metric.current() * weight;
            }
            cachedCurrent = result;
        }

        public override bool VisualizationEnabled() {
            return false;
        }

        public override void Visualize() {
            // show raycasts
        }

        public override void Hide() {
            // hide raycasts
        }

        public override string name() {
            return "Holistic Score";
        }

        public override string description() {
            return "Weighted average of all metrics.";
        }

        public override float baseline() {
            return 59.677f / 100f;
        }

        public override float optimal() {
            return 1f;
        }

        float cachedCurrent = 1f;
        public override float current() {
            return cachedCurrent;
        }

        // public float baseline
        // public float optimmal
        // public float current
    }
}

public class Metrics : MonoBehaviour
{
    public GameObject spherePrefab;

    private TreeImporter treeImporter;
    private SunManager sunManager;
    public SphereMetric sphereMetric;
    public AverageLightExposureMetric averageLightMetric;
    public ConvexHullMetric convexHullMetric;
    public HollisticMetric hollisticMetric;
    public List<Metric> metrics = new List<Metric>();

    // Start is called before the first frame update
    void Start()
    {
        treeImporter = gameObject.GetComponent<TreeImporter>();
        sunManager = gameObject.GetComponent<SunManager>();
        sphereMetric = new SphereMetric(treeImporter, spherePrefab);
        averageLightMetric = new AverageLightExposureMetric(treeImporter, sunManager);
        // convexHullMetric = new ConvexHullMetric(treeImporter, spherePrefab);

        Metric[] tempMetrics = { averageLightMetric, sphereMetric };
        hollisticMetric = new HollisticMetric(tempMetrics);

        metrics.Add(averageLightMetric);
        metrics.Add(sphereMetric);
        metrics.Add(hollisticMetric);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            sphereMetric.Calculate();
            sphereMetric.Visualize();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            sphereMetric.Hide();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            hollisticMetric.Calculate();
        }
    }
}
