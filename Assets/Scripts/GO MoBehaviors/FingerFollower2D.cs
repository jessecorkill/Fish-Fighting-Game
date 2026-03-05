using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FingerFollower2D : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("If true, mouse input will be used when no touch is present (handy in the editor).")]
    public bool fallbackToMouseInEditor = true;

    [Tooltip("Optional: Only destroy objects on these layers.")]
    public LayerMask destroyableLayers = 3; // default: everything

    [Header("Data")]
    [SerializeField] private UIDocument toolbarUI;
    [SerializeField] private UIDocument meterUI;
    [SerializeField] private TankViewController tvController;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector2 targetPos;      // where we want to go this physics step
    private bool hasPointer;        // is the user touching (or using mouse)
    private float algaeCleaned = 0f;
    private VisualElement toolbarRoot;
    private VisualElement meterRoot;
    [SerializeField] private AppRoot appRoot;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        // Ensure trigger collider for overlap hits
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        //Get toolbarRoot
        if (toolbarUI == null) toolbarUI = GetComponent<UIDocument>();
        toolbarRoot = toolbarUI?.rootVisualElement;
        if (toolbarRoot == null)
        {
            Debug.LogError("toolbarRoot / rootVisualElement missing.");
            return;
        }
        //Get meterRoot
        if (meterUI == null) meterUI = GetComponent<UIDocument>();
        meterRoot = meterUI?.rootVisualElement;
        if (meterRoot == null)
        {
            Debug.LogError("meterRoot / rootVisualElement missing.");
            return;
        }
        //Bind stop scrape funciton to button
        var stopButton = toolbarRoot.Q<Button>("stopScrape");
        stopButton.RegisterCallback<ClickEvent>(e =>
        {
            stopScrapeBtnClicked();
        });

    }

    void Update()
    {
        hasPointer = false;

        // 1) Prefer first active touch
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, GetZDepth()));
            targetPos = new Vector2(w.x, w.y);
            hasPointer = (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled);
        }
        // 2) Fallback to mouse (useful in editor/testing)
        else if (fallbackToMouseInEditor)
        {
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, GetZDepth()));
            targetPos = new Vector2(w.x, w.y);
            hasPointer = true;
        }
    }
    private void stopScrapeBtnClicked()
    {
        //Hide stop button
        var stopScrapeBtn = toolbarRoot.Q<Button>("stopScrape");
        stopScrapeBtn.AddToClassList("hide");
        //Reveal Toolbar
        toolbarRoot.Q<VisualElement>("toolbar").RemoveFromClassList("faded");
        //Reveal Meters
        meterRoot.RemoveFromClassList("faded");

        int currentTank = appRoot.CurrentTank;

        //Disable scraper
        GameObject myself = GameObject.Find("Scrapper Cursor");
        myself.SetActive(false);

        //Update tank's algae count
        if (algaeCleaned != 0)
        {
            var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
            List<TankObj> tanksInstance = sd.myTanks;
            float currentDirt = tanksInstance[currentTank].Dirtiness;
            Debug.Log("Old dirtiness: " + tanksInstance[currentTank].Dirtiness);
            tanksInstance[currentTank].Dirtiness = Mathf.Clamp(currentDirt - algaeCleaned, 0, 100);            
            Debug.Log("New dirtiness: " + tanksInstance[currentTank].Dirtiness);

            //Update SaveData
            SaveManagerBehaviour.Instance.Mutate(sd =>
            {
                sd.myTanks = tanksInstance;
            });

        }

    }
    void FixedUpdate()
    {
        if (hasPointer)
        {
            // Tight follow: go exactly to the pointer each physics step
            rb.MovePosition(targetPos);
        }
        // else: do nothing (you could also hide/disable the object if desired)
    }

    // Ensures ScreenToWorldPoint lands on the plane your 2D objects live on.
    // If you use an orthographic camera, this is typically just cam.nearClipPlane or 0.
    float GetZDepth()
    {
        // For orthographic 2D, world Z of your follower is usually constant (ex: 0).
        // We convert using the camera�s distance to that plane:
        float followerZ = transform.position.z;
        return Mathf.Abs(cam.transform.position.z - followerZ);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        // Only delete objects on allowed layers
        if (other.CompareTag("algae"))
        {
            algaeCleaned = algaeCleaned + 1;
            Destroy(other.gameObject);
        }
    }
}
