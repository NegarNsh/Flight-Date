using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FlightAnimator : MonoBehaviour
{
    public static FlightAnimator instance;

    [Header("Flight Animation Settings")]
    public GameObject airplanePrefab;
    public float flightSpeed = 1.5f;
    public float maxScaleBoost = 0.8f;

    private bool isFlying = false;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    // --- PUBLIC TRIGGER ---
    public void PlayFlightSequence(PlayerUIManager playerUI, LevelConfig config, Action onSequenceComplete)
    {
        if (isFlying) return;
        StartCoroutine(AnimateBothPlayers(playerUI, config, onSequenceComplete));
    }

    // --- THE MASTER SEQUENCE ---
    private IEnumerator AnimateBothPlayers(PlayerUIManager playerUI, LevelConfig config, Action onSequenceComplete)
    {
        isFlying = true;

        // =========================================================
        // --- PLAY THE AUDIO MANAGER SOUND! ---
        if (AudioManager.instance != null)
        {
            // IMPORTANT: Make sure you name the sound "Airplane" in your AudioManager list!
            AudioManager.instance.PlaySound("Airplane");
        }
        // =========================================================

        if (MapManager.instance != null)
        {
            if (MapManager.instance.avatarA != null) MapManager.instance.avatarA.gameObject.SetActive(false);
            if (MapManager.instance.avatarB != null) MapManager.instance.avatarB.gameObject.SetActive(false);
        }

        int flightsCompleted = 0;
        List<GameObject> activePlanes = new List<GameObject>();

        TimelineColumn timelineA = playerUI.dropZoneA.GetComponent<TimelineColumn>();
        StartCoroutine(AnimateSinglePlayer(timelineA, config.characterA.startingCity, activePlanes, () => { flightsCompleted++; }));

        TimelineColumn timelineB = playerUI.dropZoneB.GetComponent<TimelineColumn>();
        StartCoroutine(AnimateSinglePlayer(timelineB, config.characterB.startingCity, activePlanes, () => { flightsCompleted++; }));

        yield return new WaitUntil(() => flightsCompleted >= 2);

        yield return new WaitForSeconds(0.5f);

        // =========================================================
        // --- STOP THE AUDIO MANAGER SOUND! ---
        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopSound("Airplane");
        }
        // =========================================================

        foreach (GameObject p in activePlanes)
        {
            if (p != null) Destroy(p);
        }

        isFlying = false;
        onSequenceComplete?.Invoke();
    }


    // --- THE REUSABLE FLIGHT RECIPE ---
    // Notice we added the "activePlanes" list to the recipe requirements!
    private IEnumerator AnimateSinglePlayer(TimelineColumn timeline, string startCityName, List<GameObject> activePlanes, Action onFinished)
    {
        if (airplanePrefab == null || timeline == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        List<DraggableFlight> tickets = timeline.GetSortedTickets();

        Transform startCity = GetCityTransform(startCityName);
        if (startCity == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        // --- NEW: Find the absolute topmost Canvas in the entire scene ---
        Canvas topCanvas = startCity.GetComponentInParent<Canvas>().rootCanvas;

        // Spawn the plane on that top canvas instead of the city's folder!
        GameObject plane = Instantiate(airplanePrefab, startCity.position, Quaternion.identity, topCanvas.transform);

        // Now when we say "go to the front", it goes to the front of the ENTIRE game!
        plane.transform.SetAsLastSibling();

        // --- NEW: Give this newly spawned plane to the Master Sequence to hold onto! ---
        activePlanes.Add(plane);

        Vector3 baseScale = plane.transform.localScale;
        Vector3 currentPos = startCity.position;

        foreach (DraggableFlight ticket in tickets)
        {
            Transform destinationCity = GetCityTransform(ticket.flightData.destination);
            if (destinationCity == null) continue;

            Vector3 targetPos = destinationCity.position;

            float t = 0f;
            while (t < 1f)
            {
                if (plane == null) yield break;

                t += Time.deltaTime * flightSpeed;
                float normalizedT = Mathf.Clamp01(t);

                plane.transform.position = Vector3.Lerp(currentPos, targetPos, normalizedT);

                Vector3 direction = (targetPos - currentPos).normalized;
                if (direction != Vector3.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    plane.transform.rotation = Quaternion.Euler(0, 0, angle);
                }

                // Upside-Down Fix + Scale Bounce
                float flipY = (direction.x < 0) ? -1f : 1f;
                float scaleMultiplier = 1f + (Mathf.Sin(normalizedT * Mathf.PI) * maxScaleBoost);
                plane.transform.localScale = new Vector3(baseScale.x, baseScale.y * flipY, baseScale.z) * scaleMultiplier;

                yield return null;
            }

            if (plane == null) yield break;

            plane.transform.position = targetPos;
            plane.transform.rotation = Quaternion.identity;
            plane.transform.localScale = baseScale;
            currentPos = targetPos;

            yield return new WaitForSeconds(0.3f);
        }

        // We DO NOT destroy the plane here anymore! We just tell the master sequence we landed.
        onFinished?.Invoke();
    }

    private Transform GetCityTransform(string cityName)
    {
        GameObject cityObj = GameObject.Find(cityName);
        if (cityObj != null) return cityObj.transform;
        return null;
    }
}