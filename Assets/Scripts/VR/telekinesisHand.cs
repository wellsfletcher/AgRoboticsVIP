using UnityEngine;

public class telekinesisHand : MonoBehaviour
{
    [SerializeField]
    protected OVRInput.Controller m_controller;
    public TreeImporter treeImporter;

    // render our hand pointer raycast
    public LineRenderer telekinesisLine;
    
    // information about the line render
    public float lineWidth = 0.1f;
    public float lineMaxLength = 100f;
   
    // boolean to determine if the line render is enabled or disabled
    public bool toggled = false;
 
    // store input from our right hand trigger
    private float HandRight = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
    
    // boolean to determine if we hit an enemy with the raycast
    public bool enemyHit = false;
    // game object to store the enemy
    private GameObject enemy;

    private Material hitMaterial;
    private Material missMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // set up our line render
        Vector3[] startLinePositions = new Vector3[2] { Vector3.zero, Vector3.zero };
        telekinesisLine.SetPositions( startLinePositions);
        telekinesisLine.enabled = false;

        hitMaterial = telekinesisLine.materials[0];
        missMaterial = telekinesisLine.materials[1];
        telekinesisLine.material = missMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        // update the value of HandRight every frame with new value from trigger
        HandRight = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);
        

        // turn on/off the line render if trigger is pulled in
        if (HandRight > 0.9)
        {
            toggled = true;
            telekinesisLine.enabled = true;
        } else
        {
            telekinesisLine.enabled = false;
            toggled = false;
            // make sure that we can't register a hit on an enemy when the line renderer is turned off
            enemyHit = false;
        }

        if (toggled)
        {
            // starts the raycast if we have pulled the trigger
            telekinesis(transform.position, transform.forward, lineMaxLength);
            
        }
        

    }

    // raycast function - draws the line renderer, detects if an enemy is hit, updates the telekinesisExplode script
    private void telekinesis(Vector3 targetPosition, Vector3 direction, float length)
    {
        // set up raycast hit
        RaycastHit hit;

        LayerMask layerMask = LayerMask.GetMask("cylinder");

        // set up raycast
        Ray telekinesisOut = new Ray(targetPosition, direction);
        
        // declares an end position variable for the line renderer
        Vector3 endPosition = targetPosition + (length * direction);
        telekinesisLine.material = missMaterial;

        // run the raycast
        if (Physics.Raycast(telekinesisOut, out hit, 30f, layerMask))
        {
            // update the line render with the new end position
            endPosition = hit.point;

            // set the enemy game object to the gameObject that the raycast hit
            enemy = hit.collider.gameObject;

            telekinesisLine.material = hitMaterial;

            // if the enemy has the telekinesisExplode script, do something
            // if (enemy.GetComponent<telekinesisExplode>())
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                enemyHit = true;
                // update boolean variable in telekinesisExplode script
                //- enemy.GetComponent<telekinesisExplode>().rightHandRay = true;
                // debugging
                treeImporter.HideChildren(enemy);
                Debug.Log("EnemyHit Value Is: " + enemyHit);
            }
            else 
            {
                enemyHit = false;
                // debugging
                Debug.Log("EnemyHit Value Is: " + enemyHit);
            }

        }
        // if the raycast stops, set enemyHit to false
        else if (enemyHit)
        {
            enemyHit = false;
            Debug.Log("EnemyHit Value Is: " + enemyHit);
        }

        // update our Line Renderer declared at top of file
        telekinesisLine.SetPosition(0, targetPosition);
        telekinesisLine.SetPosition(1, endPosition);
    }
}
