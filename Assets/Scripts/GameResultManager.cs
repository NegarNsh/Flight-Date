using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameResultManager : MonoBehaviour
{
    public static GameResultManager instance;

    [Header("UI Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject endGameScreen;

    [Header("UI Text")]
    public TextMeshProUGUI winTextDisplay;

    // --- NEW: THE MISSING PORTRAIT VARIABLES! ---
    [Header("In-Game Portraits")]
    public UnityEngine.UI.Image gamePortraitA;
    public UnityEngine.UI.Image gamePortraitB;

    [Header("Win Screen Portraits")]
    public UnityEngine.UI.Image winPortraitA;
    public UnityEngine.UI.Image winPortraitB;

    [Header("Lose Screen Portraits")]
    public UnityEngine.UI.Image losePortraitA;
    public UnityEngine.UI.Image losePortraitB;
    // --------------------------------------------

    [Header("Transition Settings")]
    public float fadeSpeed = 0.4f;

    [HideInInspector]
    public bool isFinalLevel = false;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    // --- NEW CLEAN START TRAVEL BUTTON ---
    public void StartTravelSequence(PlayerUIManager playerUI, LevelConfig config)
    {
        // Tell the new Animator to handle the visuals, and pass CheckWinCondition as the finish line!
        if (FlightAnimator.instance != null)
        {
            FlightAnimator.instance.PlayFlightSequence(playerUI, config, () =>
            {
                CheckWinCondition(playerUI, config);
            });
        }
        else
        {
            // Failsafe just in case the animator is missing
            CheckWinCondition(playerUI, config);
        }
    }

    // --- GAME LOGIC ---
    public void CheckWinCondition(PlayerUIManager playerUI, LevelConfig config)
    {
        string finalDestA = GetFinalDestination(playerUI.dropZoneA, config.characterA.startingCity);
        string finalDestB = GetFinalDestination(playerUI.dropZoneB, config.characterB.startingCity);

        if (finalDestA == finalDestB)
        {
            if (winTextDisplay != null) winTextDisplay.text = config.winText;

            if (isFinalLevel && endGameScreen != null)
                StartCoroutine(FadeScreen(endGameScreen, true));
            else if (winScreen != null)
                StartCoroutine(FadeScreen(winScreen, true));
        }
        else
        {
            if (loseScreen != null)
                StartCoroutine(FadeScreen(loseScreen, true));
        }
    }

    public string GetFinalDestination(Transform dropZone, string defaultCity)
    {
        TimelineColumn timeline = dropZone.GetComponent<TimelineColumn>();
        if (timeline != null)
        {
            List<DraggableFlight> sortedTickets = timeline.GetSortedTickets();
            if (sortedTickets.Count > 0)
                return sortedTickets[sortedTickets.Count - 1].flightData.destination;
        }
        return defaultCity;
    }

    // --- NEW: THE MISSING PORTRAIT FUNCTION! ---
    public void SetupLevelPortraits(string nameA, string nameB)
    {
        if (gamePortraitA != null) gamePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Normal");
        if (gamePortraitB != null) gamePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Normal");

        if (winPortraitA != null) winPortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Happy");
        if (winPortraitB != null) winPortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Happy");

        if (losePortraitA != null) losePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Sad");
        if (losePortraitB != null) losePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Sad");
    }
    // -------------------------------------------

    public void HideAllScreens()
    {
        if (winScreen != null && winScreen.activeSelf) StartCoroutine(FadeScreen(winScreen, false));
        if (loseScreen != null && loseScreen.activeSelf) StartCoroutine(FadeScreen(loseScreen, false));
        if (endGameScreen != null && endGameScreen.activeSelf) StartCoroutine(FadeScreen(endGameScreen, false));
    }

    private IEnumerator FadeScreen(GameObject screen, bool fadeIn)
    {
        if (screen == null) yield break;

        CanvasGroup cg = screen.GetComponent<CanvasGroup>();
        if (cg == null) cg = screen.AddComponent<CanvasGroup>();

        if (fadeIn)
        {
            cg.alpha = 0f;
            screen.SetActive(true);
        }

        cg.interactable = false;
        cg.blocksRaycasts = false;

        float elapsedTime = 0f;
        float startAlpha = cg.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeSpeed);
            yield return null;
        }

        cg.alpha = targetAlpha;

        if (fadeIn)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            screen.SetActive(false);
        }
    }
}