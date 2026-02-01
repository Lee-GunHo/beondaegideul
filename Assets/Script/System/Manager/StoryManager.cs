using UnityEngine;

public static class StoryProgress
{
    private const string KEY_HAS_SEEN_STORY = "HAS_SEEN_STORY";

    public static bool HasSeenStory()
    {
        return PlayerPrefs.GetInt(KEY_HAS_SEEN_STORY, 0) == 1;
    }

    public static void MarkStorySeen()
    {
        PlayerPrefs.SetInt(KEY_HAS_SEEN_STORY, 1);
        PlayerPrefs.Save();
    }

    public static void ResetStorySeen()
    {
        PlayerPrefs.SetInt(KEY_HAS_SEEN_STORY, 0);
        PlayerPrefs.Save();
    }
}