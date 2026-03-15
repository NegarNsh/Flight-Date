using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;

// This tells Unity to completely replace the default Inspector for the AudioManager!
[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw the normal inspector so you still see the arrays and slots
        DrawDefaultInspector();

        AudioManager manager = (AudioManager)target;

        // 2. Add some space and a nice header
        GUILayout.Space(20);
        GUILayout.Label("--- Quick Audio Preview ---", EditorStyles.boldLabel);

        // 3. Automatically create a Play button for every single sound in the list!
        if (manager.sounds != null)
        {
            foreach (GameSound s in manager.sounds)
            {
                if (!string.IsNullOrEmpty(s.soundName) && s.clip != null)
                {
                    // If the designer clicks this button, trigger the preview
                    if (GUILayout.Button("▶ Play " + s.soundName))
                    {
                        PreviewSound(s);
                    }
                }
            }
        }
    }

    // A clever asynchronous function to play sounds in Edit Mode without hitting "Play"
    private async void PreviewSound(GameSound s)
    {
        if (s.clip == null) return;

        // Create an invisible, temporary speaker that doesn't save to the scene
        GameObject previewSpeaker = new GameObject("Preview_Speaker");
        previewSpeaker.hideFlags = HideFlags.HideAndDontSave;
        AudioSource source = previewSpeaker.AddComponent<AudioSource>();

        source.clip = s.clip;
        source.volume = s.volume;
        source.time = s.startTime; // Obey the designer's X time
        source.Play();

        float duration = s.clip.length - s.startTime;
        if (s.endTime > s.startTime) duration = s.endTime - s.startTime; // Obey the Y time

        // Wait for the duration to finish, then delete the preview speaker
        await Task.Delay((int)(duration * 1000));

        if (previewSpeaker != null) DestroyImmediate(previewSpeaker);
    }
}