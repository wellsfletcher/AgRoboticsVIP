using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System.DateTime;
using System;

public class SunManager : MonoBehaviour
{
    public class SunState {
        public string timestamp;
        public System.DateTime date;
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion lightRotation;
        public int totalHits = 0;
        public int baseTotalHits = 0;
    }

    public Transform grid;
    public Transform light;
    public GameObject treeDestination;
    private Vector3 treeDestinationPos;

    public LightQuantifier quantifier;
    public TreeImporter treeImporter;
    private List<SunState> states;
    private int stateIndex = 0;

    // Start is called before the first frame update
    void Start() {
        treeImporter = gameObject.GetComponent<TreeImporter>();

        treeDestinationPos = treeDestination.transform.position;

        string path = Application.persistentDataPath + "/SunLocations.csv";
        Debug.Log(path);
        // List<SunState> sunStates = ParseSunCSV(path);
        //- states = ParseSunCSV(path);
        states = ParseSunCSV();
        // PlaceCylinders(cylinderData, treeDestination);
    }

    // Update is called once per frame
    private bool firstRun = true;
    void Update() {
        if (!Done()) {
            GatherData();
        }
        // float HandLeft = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
        if (Input.GetKeyDown(KeyCode.Z)) {
            PreviousSunPosition();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            NextSunPosition();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            Restart();
        }

        //- CompareToBaseline(); // this queues sun update
        Debug.Log(CalculateAverageLightExposure());
    }
    
    private void UpdateSun() {
        SunState state = states[stateIndex];

        // make leaves change over time
        // make sure this updates properly
        float leafScale = CalculateLeafScaleOverTime(state.date);
        treeImporter.tree.SetLeafPercentScale(leafScale);

        // quantifier.UpdateTree();
        // may wanna update tree immediately
        quantifier.QueueTreeUpdate();
        grid.position = state.position;
        grid.rotation = state.rotation;
        state.totalHits = quantifier.totalHits; // totalHits
    
        // Debug.Log("timestamp = " + state.timestamp + ", totalHits = " + normalizeTotalHits(state.totalHits));
        // light.rotation = state.lightRotation;
        //- buildTotalHitsCSV();
    }

    /*
     * Returns a value between 0 and 1
     */
    private float CalculateLeafScaleOverTime(DateTime timestamp) {
        // DateTime winter = timestamp.AddMonths(0);
        // winter.Month = 12;
        // actually we're just gonna say winter is Jan 1st
        int month = timestamp.Month - 1;
        int day = timestamp.Day;
        float timeFromWinter = month + (timestamp.Day / 30f); // fight me
        if (month > 5) {
            timeFromWinter = 12 - (month + (timestamp.Day / 30f));
        }
        float normalizedTimeFromWinter = timeFromWinter / 6.0f;
        return normalizedTimeFromWinter;
    }

    public void PreviousSunPosition() {
        stateIndex = System.Math.Max(0, stateIndex - 1);
        UpdateSun();
    }

    public void NextSunPosition() {
        stateIndex = System.Math.Min(stateIndex + 1, states.Count - 1);
        UpdateSun();
    }

    public void CompareToBaseline() {
        UpdateSun();

        SunState state = states[stateIndex];
        int baseHits = state.baseTotalHits;
        int hits = state.totalHits;
        float nBaseHits = normalizeTotalHits(state.baseTotalHits);
        float nHits = normalizeTotalHits(state.totalHits);

        int increase = hits - baseHits;
        float percent = 100f * (nHits - nBaseHits) / nBaseHits;

        Debug.Log("baseline hits = " + baseHits + ", " +
            "hits = " + hits + ", " +
            "difference = " + increase + ", " +
            "percent increase = " + percent);

    }

    public string GetBaselineText() {
        //- UpdateSun(); // nah

        SunState state = states[stateIndex];
        int baseHits = state.baseTotalHits;
        int hits = state.totalHits;
        float nBaseHits = normalizeTotalHits(state.baseTotalHits);
        float nHits = normalizeTotalHits(state.totalHits);

        int increase = hits - baseHits;
        float percent = 100f * (nHits - nBaseHits) / nBaseHits;

        return "baseline hits = " + baseHits + ", " +
            "hits = " + hits + ", " +
            // "percent increase = " + percent;
            state.timestamp;
    }

    public float[] GetBaseline() {
        //- UpdateSun();

        SunState state = states[stateIndex];
        int baseHits = state.baseTotalHits;
        int hits = state.totalHits;
        float nBaseHits = normalizeTotalHits(state.baseTotalHits);
        float nHits = normalizeTotalHits(state.totalHits);

        int increase = hits - baseHits;
        float percent = 100f * (nHits - nBaseHits) / nBaseHits;

        float[] result = { baseHits, hits, percent };
        return result;
    }

    public void GatherData() {
        UpdateSun();
        SunState state = states[stateIndex];
        if (firstRun) {
            // user could technically mess this up
            state.baseTotalHits = state.totalHits;
        }
        else {
            state.totalHits = state.totalHits;
        }
        stateIndex++;
    }

    private bool isDone = false;
    public bool Done() {
        bool result = isDone;

        if (!result && stateIndex >= states.Count) {
            result = true;
            firstRun = false;
            stateIndex = states.Count - 1;
        }

        isDone = result;
        return result;
    }

    public void Restart() {
        stateIndex = 0;
        isDone = false;
    }

    public float normalizeTotalHits(int totalHits) {
        // requires all states to have been visited once
        int n = 0;
        /*
        foreach (SunState state in states) {
            n += state.totalHits;
        }
        */
        foreach (SunState state in states) {
            n += System.Math.Max(n, state.baseTotalHits);
        }
        return ((float)totalHits) / n;
    }

    public float CalculateAverageLightExposure() {
        int n = 0;
        foreach (SunState state in states) {
            // n += System.Math.Max(n, state.baseTotalHits);
            n = System.Math.Max(n, state.baseTotalHits);
        }
        int N = states.Count;
        float total = 0;
        foreach (SunState state in states) {
            if (state.totalHits >= 1) {
                float normalizedLightExposure = ((float)state.totalHits) / n;
                total += normalizedLightExposure;
                Debug.Log(state.totalHits);
                // be careful if it is zero
            }
            else {
                N--;
            }
        }
        N = System.Math.Max(N, 1);
        return total / N;
    }

    private void buildTotalHitsCSV() {
        string result = "";
        foreach (SunState state in states) {
            result += state.timestamp;
            result += ", ";
            result += normalizeTotalHits(state.totalHits);
            result += "\n";
        }
        // Debug.Log(result);
    }

    private List<SunState> ParseSunCSV(string path) {
        string fileData = System.IO.File.ReadAllText(path);
        return ParseSunCSVString(fileData);
    }

    private List<SunState> ParseSunCSV() {
        // Constants constants = transform.GetComponent<Constants>();
        // string fileData = System.IO.File.ReadAllText(path);
        string fileData = Constants.SunLocations;
        return ParseSunCSVString(fileData);
    }

    private List<SunState> ParseSunCSVString(string fileData) {
        //- string fileData = System.IO.File.ReadAllText(path);
        string[] lines = fileData.Split("\n"[0]);
        string[] lineData = (lines[0].Trim()).Split(","[0]);
        // List<List<float>> sunData = new List<List<float>>();
        List<SunState> sunStates = new List<SunState>();
        foreach (string line in lines) {
            string[] words = (line.Trim()).Split(","[0]);
            if (words.Length > 0) {
                // List<float> parameters = new List<float>();
                string nothing = words[0];
                string timestamp = words[1];

                float rotX = float.Parse(words[2]);
                float rotY = float.Parse(words[3]);
                float rotZ = float.Parse(words[4]);

                float posX = float.Parse(words[5]);
                float posY = float.Parse(words[6]);
                float posZ = float.Parse(words[7]);
          

                SunState state = new SunState();
                Vector3 pos = new Vector3(posX, posY, posZ);
                state.position = new Vector3(-3f, 10.93f, 0f) + treeDestinationPos;
                state.rotation = Quaternion.LookRotation(pos, Vector3.down);
                state.lightRotation = Quaternion.LookRotation(pos, Vector3.up);
                state.timestamp = timestamp;

                string parsedTimestamp = timestamp.Split("+"[0])[0].Trim();
                // Debug.Log(parsedTimestamp);
                DateTime date = DateTime.ParseExact(parsedTimestamp, "yyyy-MM-dd HH:mm:ss", null);
                state.date = date;
                // Debug.Log(date.ToString());

                // Debug.Log(words.Length);
                // sunData.Add(parameters);
                sunStates.Add(state);
            }
        }
        // var x : float;
        // float.TryParse(lineData[0], x);
        // Debug.Log(lineData.Length);
        return sunStates;
    }


}
