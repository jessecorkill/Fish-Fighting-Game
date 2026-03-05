using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.U2D;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TankToolsController : MonoBehaviour

{
    [SerializeField] private UIDocument toolbarUI;
    //Get Meter UIDocument
    //Get AppRoot GameObject
    private VisualElement toolbarRoot; 
    //private int currentTank;
    [SerializeField] private AppRoot appRoot;
    [SerializeField] private VisualTreeAsset swapFishTemplate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment 

        //Get toolbarUI for UI referrence
        if (toolbarUI == null) toolbarUI = GetComponent<UIDocument>();
        toolbarRoot = toolbarUI?.rootVisualElement;
        if (toolbarRoot == null)
        {
            Debug.LogError("toolbarRoot / rootVisualElement missing.");
            return;
        }

        //Bind swap tank button funcitonality
        var swapTankBtn = toolbarRoot.Q<VisualElement>("swapTank");
        swapTankBtn.RegisterCallback<ClickEvent>(e => toggleTankSwapUI());

        //Bind training button to chose training UI toggle
        var trainingBtn = toolbarRoot.Q<VisualElement>("trainBtn");
        trainingBtn.RegisterCallback<ClickEvent>(e => toggleTrainingOptionsUI());

        //Bind evade training button to funciton
        var evadeTrainBtn = toolbarRoot.Q<VisualElement>("trainEvade");
        evadeTrainBtn.RegisterCallback<ClickEvent>(e => evadeTrainingClicked());

        populateFishSelector(sd);
    }

        private void populateFishSelector(SaveData sd)
    {
        //
        var swapFishModal = toolbarRoot.Q<VisualElement>("swapFishModal");
        var targetScrollView = swapFishModal.Q<ScrollView>(); //The scroll view element
        
        int fishCount = sd.myBettas.Count;
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
    }
    private void toggleTrainingOptionsUI()
    {
        Debug.Log("Training Button Clicked");
        var trainingOptionModal = toolbarRoot.Q<VisualElement>("trainingModal");
        trainingOptionModal.RemoveFromClassList("hide");

    }
    private void attackTrainingClicked()
    {
        
    }
    private void evadeTrainingClicked()
    {
        SceneManager.LoadScene("05_Train_Evade");
    }
    private void toggleTankSwapUI()
    {
        Debug.Log("Swap Tank Clicked");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
