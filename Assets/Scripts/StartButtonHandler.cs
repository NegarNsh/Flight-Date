using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonHandler : MonoBehaviour
{
    public AudioClip clickSound1;
    public AudioClip clickSound2;

    private bool isLoading = false;

    public void OnStartClicked()
    {
        if (isLoading) return;
        isLoading = true;

        Debug.Log("Butona basýldý!");

        GameObject soundPlayer = new GameObject("TempSoundPlayer");
        DontDestroyOnLoad(soundPlayer);

        if (clickSound1 != null)
        {
            Debug.Log("clickSound1 yüklendi: " + clickSound1.name);
            AudioSource s1 = soundPlayer.AddComponent<AudioSource>();
            s1.PlayOneShot(clickSound1);
        }
        else Debug.LogError("clickSound1 BOÞ!");

        if (clickSound2 != null)
        {
            Debug.Log("clickSound2 yüklendi: " + clickSound2.name);
            AudioSource s2 = soundPlayer.AddComponent<AudioSource>();
            s2.PlayOneShot(clickSound2);
        }
        else Debug.LogError("clickSound2 BOÞ!");

        float delay = Mathf.Max(
            clickSound1 != null ? clickSound1.length : 0,
            clickSound2 != null ? clickSound2.length : 0
        );

        StartCoroutine(LoadAfterDelay(soundPlayer, delay));
    }

    System.Collections.IEnumerator LoadAfterDelay(GameObject soundPlayer, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(soundPlayer);
        SceneManager.LoadScene("GameScene");
    }
}