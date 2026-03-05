using System.Threading;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] LaneSystem laneSystem;
    [SerializeField] GameObject obstacle;
    [SerializeField] staminaGameController gameController;
    private float time = 0;
    private float interval = .5f;
    float offscreenRight = 0;
    public Camera mainCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        getRightBorderX();

        spawnRandomObstacle();
    }
    private void getRightBorderX(){
        if(mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        Vector3 bottomRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0));
        offscreenRight = bottomRight.x;
    }

    private void spawnRandomObstacle()
    {   
        //Get a random lane
        int randLane = Random.Range(0, laneSystem.GetTotalLanes());

        //Generate a positon
        Vector3 pos = new Vector3( offscreenRight, laneSystem.GetLaneY(randLane), 0);
        //Create object instance & spawn it in a lane off-screen to the right
        GameObject obs = Instantiate(obstacle, pos, transform.rotation);
        //Give the obstacle inertia to the left
        Rigidbody2D rb = obs.GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(-500f, 0f), ForceMode2D.Force);
    }
    private string GetGameState()
    {
        return gameController.GetState();
    }
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;

        if(time >= interval && GetGameState() != "stop")
        {
            spawnRandomObstacle();
            time -= interval; //Reset timer
        }

    }
}
