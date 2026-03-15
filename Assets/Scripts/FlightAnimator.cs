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

    // --- NEW: The Double-Click Lock! ---
    private bool isFlying = false;

    void Awake()
    {
        if (instance == null) { instance = this; } else { Destroy(gameObject); }
    }

    // --- PUBLIC TRIGGER ---
    public void PlayFlightSequence(PlayerUIManager playerUI, LevelConfig config, Action onSequenceComplete)
    {
        // If we are already flying, ignore any extra button clicks!
        if (isFlying) return;

        StartCoroutine(AnimatePlayerJourney(playerUI, config, onSequenceComplete));
    }

    // --- THE ANIMATION LOGIC ---
    private IEnumerator AnimatePlayerJourney(PlayerUIManager playerUI, LevelConfig config, Action onSequenceComplete)
    {
        isFlying = true; // Lock the doors, we are taking off!

        if (airplanePrefab == null)
        {
            isFlying = false;
            onSequenceComplete?.Invoke();
            yield break;
        }

        TimelineColumn timeline = playerUI.dropZoneA.GetComponent<TimelineColumn>();
        List<DraggableFlight> tickets = timeline.GetSortedTickets();

        Transform startCity = GetCityTransform(config.characterA.startingCity);
        if (startCity == null)
        {
            isFlying = false;
            onSequenceComplete?.Invoke();
            yield break;
        }

        GameObject plane = Instantiate(airplanePrefab, startCity.position, Quaternion.identity, startCity.parent);
        plane.transform.SetAsLastSibling();

        Vector3 baseScale = plane.transform.localScale;
        Vector3 currentPos = startCity.position;

        if (MapManager.instance != null)
        {
            if (MapManager.instance.avatarA != null) MapManager.instance.avatarA.gameObject.SetActive(false);
            if (MapManager.instance.avatarB != null) MapManager.instance.avatarB.gameObject.SetActive(false);
        }

        foreach (DraggableFlight ticket in tickets)
        {
            Transform destinationCity = GetCityTransform(ticket.flightData.destination);
            if (destinationCity == null) continue;

            Vector3 targetPos = destinationCity.position;

            float t = 0f;
            while (t < 1f)
            {
                // --- NEW: Safety check! If the plane got destroyed, stop the code immediately. ---
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

                // The Upside-Down Fix + Scale Bounce
                float flipY = (direction.x < 0) ? -1f : 1f;
                float scaleMultiplier = 1f + (Mathf.Sin(normalizedT * Mathf.PI) * maxScaleBoost);
                plane.transform.localScale = new Vector3(baseScale.x, baseScale.y * flipY, baseScale.z) * scaleMultiplier;

                yield return null;
            }

            // --- NEW: Safety check before resetting! ---
            if (plane == null) yield break;

            plane.transform.position = targetPos;
            plane.transform.rotation = Quaternion.identity;
            plane.transform.localScale = baseScale;
            currentPos = targetPos;

            yield return new WaitForSeconds(0.3f);
        }

        // Clean up and unlock!
        if (plane != null) Destroy(plane);
        isFlying = false;
        onSequenceComplete?.Invoke();
    }

    private Transform GetCityTransform(string cityName)
    {
        GameObject cityObj = GameObject.Find(cityName);
        if (cityObj != null) return cityObj.transform;
        return null;
    }
}