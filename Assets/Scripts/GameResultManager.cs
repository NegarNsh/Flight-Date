using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameResultManager : MonoBehaviour
{
    // --- VARIABLES ---
    public static GameResultManager instance;

    [Header("UI Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject endGameScreen;

    [Header("UI Text")]
    public TextMeshProUGUI winTextDisplay;

    [Header("In-Game Portraits")]
    public UnityEngine.UI.Image gamePortraitA;
    public UnityEngine.UI.Image gamePortraitB;

    [Header("Win Screen Portraits")]
    public UnityEngine.UI.Image winPortraitA;
    public UnityEngine.UI.Image winPortraitB;

    [Header("Lose Screen Portraits")]
    public UnityEngine.UI.Image losePortraitA;
    public UnityEngine.UI.Image losePortraitB;

    [Header("Transition Settings")]
    [Tooltip("How many seconds it takes for the screens to fade in and out.")]
    public float fadeSpeed = 0.4f;

    [HideInInspector]
    public bool isFinalLevel = false;

    // --- UNITY SETUP ---
    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    // --- GAME LOGIC ---
    public void CheckWinCondition(PlayerUIManager playerUI, LevelConfig config)
    {

        string finalDestA = GetFinalDestination(playerUI.dropZoneA, config.characterA.startingCity);
        string finalDestB = GetFinalDestination(playerUI.dropZoneB, config.characterB.startingCity);

        if (finalDestA == finalDestB)
        {
            Debug.Log("WIN! Both players met in " + finalDestA);
            if (winTextDisplay != null) winTextDisplay.text = config.winText;

            if (isFinalLevel && endGameScreen != null)
            {
                StartCoroutine(FadeScreen(endGameScreen, true));
            }
            else if (winScreen != null)
            {
                StartCoroutine(FadeScreen(winScreen, true));
            }
        }
        else
        {
            Debug.Log("LOSE! Player A is in " + finalDestA + " but Player B is in " + finalDestB);
            if (loseScreen != null)
            {
                StartCoroutine(FadeScreen(loseScreen, true));
            }
        }
    }

    public string GetFinalDestination(Transform dropZone, string defaultCity)
    {
        TimelineColumn timeline = dropZone.GetComponent<TimelineColumn>();
        if (timeline != null)
        {
            List<DraggableFlight> sortedTickets = timeline.GetSortedTickets();
            if (sortedTickets.Count > 0)
            {
                return sortedTickets[sortedTickets.Count - 1].flightData.destination;
            }
        }
        return defaultCity;
    }

    public void SetupLevelPortraits(string nameA, string nameB)
    {
        if (gamePortraitA != null) gamePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Normal");
        if (gamePortraitB != null) gamePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Normal");

        if (winPortraitA != null) winPortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Happy");
        if (winPortraitB != null) winPortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Happy");

        if (losePortraitA != null) losePortraitA.sprite = Resources.Load<Sprite>($"Characters/{nameA}_Sad");
        if (losePortraitB != null) losePortraitB.sprite = Resources.Load<Sprite>($"Characters/{nameB}_Sad");
    }

    public void HideAllScreens()
    {
        // Smoothly fade out any screen that is currently visible!
        if (winScreen != null && winScreen.activeSelf) StartCoroutine(FadeScreen(winScreen, false));
        if (loseScreen != null && loseScreen.activeSelf) StartCoroutine(FadeScreen(loseScreen, false));
        if (endGameScreen != null && endGameScreen.activeSelf) StartCoroutine(FadeScreen(endGameScreen, false));
    }

    // --- THE VISUAL FADE MAGIC ---
    private IEnumerator FadeScreen(GameObject screen, bool fadeIn)
    {
        if (screen == null) yield break;

        // 1. Secretly add a CanvasGroup if the designer forgot to put one on the screen!
        CanvasGroup cg = screen.GetComponent<CanvasGroup>();
        if (cg == null) cg = screen.AddComponent<CanvasGroup>();

        // 2. Setup the starting values
        if (fadeIn)
        {
            cg.alpha = 0f; // Make it 100% invisible
            screen.SetActive(true); // Turn it on
        }

        // Prevent the player from clicking buttons while the screen is still ghostly/fading
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // 3. Do the smooth math!
        float elapsedTime = 0f;
        float startAlpha = cg.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        while (elapsedTime < fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeSpeed);
            yield return null;
        }

        // 4. Clean up at the exact end of the fade
        cg.alpha = targetAlpha;

        if (fadeIn)
        {
            // Now that it's fully visible, let them click the buttons!
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            // Now that it is fully invisible, turn the object off completely!
            screen.SetActive(false);
        }
    }
}