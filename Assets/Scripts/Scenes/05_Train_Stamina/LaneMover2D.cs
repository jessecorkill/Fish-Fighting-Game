using UnityEngine;

public class LaneMover2D : MonoBehaviour
{
    [SerializeField] LaneSystem laneSystem;      // The origin of our lane data
    [SerializeField] int startLane = 1;          // 0=left, 1=middle, 2=right
    [SerializeField] float moveSpeed = 12f;      // for smooth tween
    [SerializeField] bool snapInstantly = false; // set true if you want instant jumps

    int currentLane;

    void Start()
    {

        currentLane = Mathf.Clamp(startLane, 0, laneSystem.laneCount - 1); //Ensure we start in a valid lane.
        var p = transform.position; //??
        Debug.Log("OG postion: " + p.y);
        Debug.Log("currentLane: " + currentLane);
        transform.position = new Vector3(p.x, laneSystem.GetLaneY(startLane), p.z); //Places the player on the center lane.
        Debug.Log("Starting position: " + transform.position.y);
        
    }

    public void NudgeLane(int delta) // call with -1 or +1 from your swipe logic
    {
        int next = Mathf.Clamp(currentLane + delta, 0, laneSystem.laneCount - 1); //Ensure we only output real lanes.
        if (next != currentLane)
            currentLane = next;
    }

    void Update()
    {
        //Debug.Log("Player Lane: " + currentLane);
        float targetY = laneSystem.GetLaneY(currentLane);
        var p = transform.position;

        if (snapInstantly)
        {
            transform.position = new Vector3(p.x, targetY, p.z); //Snap the player to the target lane
        }
        else
        {
            float y = Mathf.MoveTowards(p.y, targetY, moveSpeed * Time.deltaTime); //Animate the player to the target lane
            transform.position = new Vector3(p.x, y, p.z);
        }
    }
}
