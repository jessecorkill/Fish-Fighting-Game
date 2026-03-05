using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements; // Required for UIDocument


public class EndUIController : MonoBehaviour
{
    private UIDocument gameOverObj;
    private VisualElement gameOverUI;
    [SerializeField] staminaGameController miniGameController;
    private AppRoot appRoot = AppRoot.Instance;
    private ProgressBar evadeXPMeter;
    private VisualElement evadeMeterContainer;
    private VisualElement bondMeterContainer;
    private VisualElement attackMeterContainer;
    private VisualElement staminaMeterContainer;

    private float time;
    private BettaObj oldBetta;
    private BettaObj newBetta;
    void Awake()
    {
        //Establish variables
        gameOverObj = GetComponent<UIDocument>();
        if(gameOverObj == null)
        {
            Debug.LogError("Game Over UIDocument not found.");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(gameOverObj != null)
        {
            gameOverUI = gameOverObj.rootVisualElement;

            //Bind Restart function to button
            var restartBtn = gameOverUI.Q<Button>("again");
            restartBtn.RegisterCallback<ClickEvent>(e => Restart());

            //Bind Return function to button
            var returnBtn = gameOverUI.Q<Button>("return");
            returnBtn.RegisterCallback<ClickEvent>(e => ReturnHome());

            //Assign XP Meters with original fish's values
            evadeXPMeter = gameOverUI.Q<ProgressBar>("EvadeXP");
            

            var sd = SaveManagerBehaviour.Instance.Current; //Save Data at this moment
            newBetta = sd.myTanks[appRoot.CurrentTank].fish;
            oldBetta = miniGameController.previousBettaValues;

            //Set meter to original percentage value
            evadeXPMeter.value = XPHelper.GetProgressPercentage(oldBetta.EvadeXP);
            
            //Bind Meter Containers
            evadeMeterContainer = gameOverUI.Q<VisualElement>("EvadeContainer");
            attackMeterContainer = gameOverUI.Q<VisualElement>("AttackContainer");
            bondMeterContainer = gameOverUI.Q<VisualElement>("BondContainer");
            staminaMeterContainer = gameOverUI.Q<VisualElement>("StaminaContainer");

            //Update Level UI Elements
            updateLevelUI(evadeMeterContainer, oldBetta.EvadeXP);
            updateLevelUI(attackMeterContainer, oldBetta.AttackXP);
            
            
            

        }
        //Set up sync with Save Manager
        SaveManagerBehaviour.Instance.OnChanged += Refresh; //Hook local Refresh function to OnChange event
        if (SaveManagerBehaviour.Instance.IsLoaded) //Check if save data is ready to be pulled
        {
            Refresh(SaveManagerBehaviour.Instance.Current); //Update the UI with the most up-to-date Save data
        }

    }
    //Sets the UI element's sub element values 
    private void updateLevelUI(VisualElement meterContainer, float xp)
    {
        float currentLevel = XPHelper.GetCurrentXPLevel(xp);
        float nextLevel = currentLevel + 1;
        //Set Current Level label
        meterContainer.Q<Label>("CurrentLvl").text = currentLevel.ToString();
        //Set Next Level label
        meterContainer.Q<Label>("NextLvl").text = nextLevel.ToString();

    }
    private IEnumerator UpdateProgressBarRoutine(ProgressBar progressBar,float initalValue, float targetValue)
    {
        float progress = initalValue;
        while (progress < targetValue)
        {
            // Smoothly increase the value
            progress += Time.deltaTime * 1; 
            progressBar.value = progress;
            yield return null;
        }
    }

    void ReturnHome()
    {
        SceneManager.LoadScene("01_Landing");
    }
    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void Refresh(SaveData sd)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Update Score

        //Update Record

        //Evade Meter Update
        StartCoroutine(UpdateProgressBarRoutine(evadeXPMeter, XPHelper.GetProgressPercentage(oldBetta.EvadeXP), XPHelper.GetProgressPercentage(newBetta.EvadeXP)));
        //Stamina Meter Update
        StartCoroutine(UpdateProgressBarRoutine(staminaMeterContainer.Q<ProgressBar>(), XPHelper.GetProgressPercentage(oldBetta.StaminaXP), XPHelper.GetProgressPercentage(newBetta.AttackXP)));
        //Bond Meter Update
        StartCoroutine(UpdateProgressBarRoutine(bondMeterContainer.Q<ProgressBar>(), XPHelper.GetProgressPercentage(oldBetta.Bond), XPHelper.GetProgressPercentage(newBetta.Bond)));

    }
}
