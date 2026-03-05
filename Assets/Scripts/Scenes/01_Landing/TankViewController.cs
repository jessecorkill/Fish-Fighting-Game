using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UIElements;

public class TankViewController : MonoBehaviour
{
    [Header("Boundry UI")]
    [SerializeField] private SpriteRenderer tankBG;
    [SerializeField] private SpriteRenderer leftBound;
    [SerializeField] private SpriteRenderer rightBound;
    [SerializeField] private SpriteRenderer topBound;
    [SerializeField] private SpriteRenderer botBound;

    [Header("Menus")]
    [SerializeField] private UIDocument toolbarUI;
    [SerializeField] private VisualTreeAsset swapFishTemplate;
    [SerializeField] private UIDocument metersUI;
    [SerializeField] VisualTreeAsset meterTemplate;

    [Header("Objects")]
    [SerializeField] private SpriteRenderer bettaRenderer;
    [SerializeField] private GameObject algae;
    [SerializeField] private GameObject scraper;

    private VisualElement toolbarRoot;
    private VisualElement metersRoot;
    [SerializeField] private AppRoot appRoot;
    //public int currrentTank = appRoot.CurrrentTank; //TODO - need to re-base this value in the AppRoot controller
    private double offlineTimePassed;
    private VisualElement meterContainer;
    private bool metersReady = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Debug.Log("Current Tank: " + appRoot.CurrentTank);
        //Move Tank boundries to game view borders
        SpriteScreenUtil.FitToScreen(tankBG, Camera.main, SpriteScreenUtil.FitMode.Cover, center: true);
        SpriteScreenUtil.PlaceAtEdgeCentered(leftBound, SpriteScreenUtil.Edge.Left, Camera.main, offset: 0.0f);
        SpriteScreenUtil.PlaceAtEdgeCentered(rightBound, SpriteScreenUtil.Edge.Right, Camera.main, offset: 0.0f);
        SpriteScreenUtil.PlaceAtEdgeCentered(topBound, SpriteScreenUtil.Edge.Top, Camera.main, offset: 0.0f);
        SpriteScreenUtil.PlaceAtEdgeCentered(botBound, SpriteScreenUtil.Edge.Bottom, Camera.main, offset: 0.0f);

        //Get toolbarUI for UI referrence
        if (toolbarUI == null) toolbarUI = GetComponent<UIDocument>();
        toolbarRoot = toolbarUI?.rootVisualElement;
        if (toolbarRoot == null)
        {
            Debug.LogError("toolbarRoot / rootVisualElement missing.");
            return;
        }

        //Get meterContainer for UI referrence
        metersRoot = metersUI?.rootVisualElement;
        if(metersRoot == null)
        {
            Debug.LogError("meterRoot missing");
        }
        meterContainer = metersRoot.Q<VisualElement>("meterContainer");
        

        var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment

        //Calculate how much time has passed since last login
        calcTimeOffline(sd);
        //Updates all the needy meters (hunger, dirtiness, etc) of the tanks and their fish
        updateNeedyMeters(sd); 

        MeterController.genMeters(sd, appRoot.CurrentTank, meterTemplate, meterContainer); //Is setting meters to outdated values
        metersReady = true;

        //bind click action to action menu button
        var showToolbarBtn = toolbarRoot.Q<Button>("openToolbar");
        showToolbarBtn.RegisterCallback<ClickEvent>(
            e => {            
                showToolbar();
            }
        );
        var closeToolbarBtn = toolbarRoot.Q<VisualElement>("closeTb");
        closeToolbarBtn.RegisterCallback<ClickEvent>(
            e => {
                hideToolbar();
            }
        );
        if (sd.myTanks[0].fish != null)
        {   
            //Enable fish GameObject

            //Show tank 0 and it's fish
            _ = populateFishSprite(appRoot.CurrentTank, sd);
        }
        var swapFishBtn = toolbarRoot.Q<VisualElement>("swapFish");
        swapFishBtn.RegisterCallback<ClickEvent>(e => swapFishToolClicked());
        spawnAlgae(sd);
        //populateFishSelector(sd);

        //Bind feed fish btn to function
        var feedFishBtn = toolbarRoot.Q<VisualElement>("feedBtn");
        feedFishBtn.RegisterCallback<ClickEvent>(e => feedFish(sd));

        //Bind scraper btn to function
        var scrapeBtn = toolbarRoot.Q<VisualElement>("scrapeBtn");
        scrapeBtn.RegisterCallback<ClickEvent>(e => scrapeBtnClicked());



        Debug.Log("Current XP: " + sd.myTanks[appRoot.GetCurrentTank()].fish.EvadeXP);
        Debug.Log("Current Level: "+ XPHelper.GetCurrentXPLevel(sd.myTanks[appRoot.GetCurrentTank()].fish.EvadeXP));
        Debug.Log("Min for current Level: "+ XPHelper.GetXPLevelFloor(sd.myTanks[appRoot.GetCurrentTank()].fish.EvadeXP));
        Debug.Log("XP to next Level: "+ XPHelper.GetXPToNextLevel(sd.myTanks[appRoot.GetCurrentTank()].fish.EvadeXP));
        Debug.Log("Stamina Progress: " + XPHelper.GetProgressPercentage(sd.myTanks[appRoot.GetCurrentTank()].fish.EvadeXP) + "%");
            
        //Set up sync with Save Manager
        SaveManagerBehaviour.Instance.OnChanged += Refresh; //Hook local Refresh function to OnChange event
        if (SaveManagerBehaviour.Instance.IsLoaded) //Check if save data is ready to be pulled
        {
            Refresh(SaveManagerBehaviour.Instance.Current); //Update the UI with the most up-to-date Save data
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}, Mode: {mode}");
        // Reinitialize stuff here
        Refresh(SaveManagerBehaviour.Instance.Current);
    }
    private void SwapTank()
    {
        
    }
    /*
    private void populateFishSelector(SaveData sd)
    {
        //
        var swapFishModal = toolbarRoot.Q<VisualElement>("swapFishModal");
        var targetScrollView = swapFishModal.Q<ScrollView>(); //The scroll view element
        
        int fishCount = sd.myBettas.Count(); 
        //Create loop to create a Visual Element for each fish in user's possession
        for (int i = 0; i < fishCount; i++)
        {
            var fishItem = swapFishTemplate.Instantiate();
            var fishLabel = fishItem.Q<Label>("name");
            var fishSprite = fishItem.Q<Image>("sprite");
            fishLabel.text = sd.myBettas[i].Name;
            targetScrollView.Add(fishItem);

            _ = AtlasCache.populateImageElement(sd.myBettas[i].SpriteAtlasKey, sd.myBettas[i].SpriteName, fishSprite);

            //bind click function. Pass index to indicate what fish it is that we're talking about. They appear in the scroll view in order.
            fishItem.RegisterCallback<ClickEvent>(e =>
                {
                    var ve = (VisualElement)e.currentTarget;
                    int index = targetScrollView.IndexOf(ve);
                    swapFishClicked(index);
                }
            );
        }


    }
    */
    private void scrapeBtnClicked()
    {
        //Hide toolbar 
        toolbarRoot.Q<VisualElement>("toolbar").AddToClassList("faded");
        //Hide meters
        metersRoot.AddToClassList("faded");
        //Spawn scraper sprite
        scraper.SetActive(true);
        //Reveal scraper stop button
        var stopScrapeBtn = toolbarRoot.Q<Button>("stopScrape");
        stopScrapeBtn.RemoveFromClassList("hide");
    }
    /*
    private void swapFishClicked(int fishID)
    {
        Debug.Log("swapFishClicked: " + fishID);
        var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
        List<BettaObj> bettaListInstance = sd.myBettas;
        List<TankObj> tankListInstance = sd.myTanks;

        //Check if this fish was associated with a tank already
        if (bettaListInstance[fishID].TankID != -1)
        {
            int targetTank = bettaListInstance[fishID].TankID;
            tankListInstance[targetTank].fish = null; //Clear previous tank relationship with betta

        }

        bettaListInstance[fishID].TankID = appRoot.CurrentTank; //Set this betta's tank to this tank
        tankListInstance[appRoot.CurrentTank].fish = sd.myBettas[fishID]; //Set current tank's fish to this betta

        //Update SaveData
        SaveManagerBehaviour.Instance.Mutate(sd =>
        {
            sd.myTanks = tankListInstance;
            sd.myBettas = bettaListInstance;
        });
    } */
    private void swapFishToolClicked()
    {
        var swapFishModal = toolbarRoot.Q<VisualElement>("swapFishModal");
        swapFishModal.RemoveFromClassList("hide");
        Debug.Log("Swap Fish Clicked");
    }
    private void calcTimeOffline(SaveData sd)
    {
        //Subtract last save time from current time.
        double now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        offlineTimePassed = now - sd.lastSaveUnix;
        Debug.Log("offlineTimePassed: " + offlineTimePassed);

    }
    private void feedFish(SaveData sd)
    {
        Debug.Log("Feeding Fish...");
        
        List<TankObj> tempTanklist = sd.myTanks; //Make instance copy of tanks to manipulate and overwrite after calculating changes..

        sd.myTanks[appRoot.CurrentTank].fish.Hunger = 0f; //Reset hunger

        SaveManagerBehaviour.Instance.Mutate(sd =>
        {
            sd.myTanks = tempTanklist; //Overwrite existing List with updated List
        });        
    }
    private void spawnAlgae(SaveData sd) //Spawns the algae for the currently displayed tank
    {
        // Get bounds of the area sprite
        Bounds bounds = tankBG.bounds;

        for (int i = 0; i < sd.myTanks[appRoot.CurrentTank].Dirtiness; i++)
        {
            // Pick random X and Y within bounds
            float randX = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
            float randY = UnityEngine.Random.Range(bounds.min.y, bounds.max.y);

            // New position
            Vector3 randomPos = new Vector3(randX, randY, 0f);

            // Place/instantiate sprite
            GameObject newAlgae = Instantiate(algae, randomPos, Quaternion.identity);
            newAlgae.transform.localScale = new Vector3(1f, 1f, 1f);
        }

    }
    private void updateNeedyMeters(SaveData sd) //Runs to calculate the needy meters for everything and updates the values to what they were supposed to be.
    {        
        float hungerToAdd = loadHunger();
        float algaeToAdd = loadAlgae();
        float staminaToAdd = loadStamina();
        List <TankObj> tempTanklist = sd.myTanks; //Make instance copy of tanks to manipulate and overwrite after calculating changes..

        int tankCount = sd.myTanks.Count();
        //Loop through each tank belonging to the player
        for(int i = 0; i < tankCount; i++)
        {
            //Get tank's new algae value
            float newDirtiness = 0f;
            newDirtiness = Mathf.Clamp(sd.myTanks[i].Dirtiness + algaeToAdd, 0, 100); //Add the new value to the orginal without going above 100 or below 0
            tempTanklist[i].Dirtiness = newDirtiness;
            //Get fish's new hunger value
            float newHunger = 0f;
            newHunger = Mathf.Clamp(sd.myTanks[i].fish.Hunger + hungerToAdd, 0, 100); //Add the new value to the orginal without going above 100 or below 0
            tempTanklist[i].fish.Hunger = newHunger;
            //Get fish's new stamina value
            float newStamina = 0f;
            newStamina = Mathf.Clamp(sd.myTanks[i].fish.Stamina + staminaToAdd, 0, 100); //Add the new value to the original without going over 100 or below 0
            tempTanklist[i].fish.Stamina = newStamina;

        }
        SaveManagerBehaviour.Instance.Mutate(sd =>
        {
            sd.myTanks = tempTanklist; //Overwrite existing List with updated List
        });


        //Update SaveData with current meter values
    }
    private float loadHunger() //Calculates offline hunger change
    {
        float hungerGained;
        float hungerRate = 4.2f; //4.2% for every hour (3600 secs)
        int hoursPassed = (int)Math.Round(offlineTimePassed / 3600f); //Hours passed sincle last login
        if(hoursPassed > 0) //if at least an hour has passed..
        {
            hungerGained = hungerRate * hoursPassed;
            return hungerGained;
        }
        return 0;


    }
    private float loadAlgae() //Calculates offline algae change
    {
        float algaeGained;
        float algaeRate = .3f; //.3% for every hour. 100% at 2 weeks of build up (1,209,600 seconds or 336 hours).
        int hoursPassed = (int)Math.Round(offlineTimePassed / 3600f); // Hours passed since last login
        if(hoursPassed > 0)//if at least an hour has passed..
        {
            algaeGained = algaeRate * hoursPassed;
            return algaeGained;
        }
        return 0;
        //Set tank algae level

    }
    private float loadStamina()
    {
        float staminaRate = 1.6f; //1.6% for every minute spent offline
        int minutesPassed = (int)Math.Round(offlineTimePassed / 60f);
        if(minutesPassed >= 60)
        {
            return 100; //Return the maximum amount of stamina
        }
        else
        {
            return staminaRate * minutesPassed; 
        }
    }
    private async Task populateFishSprite(int tank, SaveData sd)
    {
        Debug.Log("populateFishSprite's targetTank: " + tank);
        if (sd.myTanks[tank].fish != null)
        {
            //enable bettaFish gameobject 
            bettaRenderer.enabled = true;
            var handle = Addressables.LoadAssetAsync<SpriteAtlas>(sd.myTanks[tank].fish.SpriteAtlasKey);
            var atlas = await handle.Task;
            if (atlas == null)
            {
                Debug.LogError($"Atlas not found for key {sd.myTanks[tank].fish.SpriteAtlasKey}");
                return;
            }
            var sprites = new Sprite[atlas.spriteCount];
            atlas.GetSprites(sprites);
            var names = string.Join(", ", sprites.Select(s => s.name));

            var sprite = atlas.GetSprite(sd.myTanks[tank].fish.SpriteName);

            //var sprite = atlas.GetSprite("HM_Orange_M_SarawutPixel_0");
            if (sprite == null)
            {
                Debug.LogWarning($"SpriteName '{sd.myTanks[tank].fish.SpriteName}' not found in atlas '{sd.myTanks[tank].fish.SpriteAtlasKey}'");
                return;
            }

            bettaRenderer.sprite = sprite;
        }
        else
        {
            bettaRenderer.enabled = false;
        }

    }
    private void hideToolbar()
    {
        var toolbar = toolbarRoot.Q<VisualElement>("toolbar");
        toolbar.AddToClassList("hide");
        var toggleBtn = toolbarRoot.Q<Button>("openToolbar");
        toggleBtn.RemoveFromClassList("hide");
    }
    private void showToolbar()
    {
        Debug.Log("Show Toolbar");
        var toolbar = toolbarRoot.Q<VisualElement>("toolbar");
        toolbar.RemoveFromClassList("hide");
        var toggleBtn = toolbarRoot.Q<Button>("openToolbar");
        toggleBtn.AddToClassList("hide");
    }
    void Refresh(SaveData sd)
    {
        //Re populate current fish sprite
        _ = populateFishSprite(appRoot.CurrentTank, sd);

        //Set values of meters
        if (metersReady)
        {
            Debug.Log("metersReady");
            MeterController.updateMeters(sd, appRoot.CurrentTank, metersRoot);
        }
        if (!metersReady)
        {
            
            Debug.Log("Meters Not Ready");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
