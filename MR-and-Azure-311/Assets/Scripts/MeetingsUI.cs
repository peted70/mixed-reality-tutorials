using System;
using UnityEngine;
public class MeetingsUI : MonoBehaviour
{
    /// <summary>
    /// Allows this class to behave like a singleton
    /// </summary>
    public static MeetingsUI Instance;

    /// <summary>
    /// The 3D text of the scene
    /// </summary>
    private TextMesh _meetingDisplayTextMesh;

    /// <summary>
    /// Called on initialization
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Called on initialization, after Awake
    /// </summary>
    void Start()
    {
        // Creating the text mesh within the scene
        _meetingDisplayTextMesh = CreateMeetingsDisplay();
    }

    /// <summary>
    /// Set the welcome message for the user
    /// </summary>
    internal void WelcomeUser(string userName)
    {
        if (!string.IsNullOrEmpty(userName))
        {
            _meetingDisplayTextMesh.text = $"Welcome {userName}";
        }
        else
        {
            _meetingDisplayTextMesh.text = "Welcome";
        }
    }

    /// <summary>
    /// Set up the parameters for the UI text
    /// </summary>
    /// <returns>Returns the 3D text in the scene</returns>
    private TextMesh CreateMeetingsDisplay()
    {
        GameObject display = new GameObject();
        display.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
        display.transform.position = new Vector3(-3.5f, 2f, 9f);
        TextMesh textMesh = display.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleLeft;
        textMesh.alignment = TextAlignment.Left;
        textMesh.fontSize = 80;
        textMesh.text = "Welcome! \nPlease gaze at the button" +
            "\nand use the Tap Gesture to display your meetings";

        return textMesh;
    }

    /// <summary>
    /// Adds a new Meeting in the UI by chaining the existing UI text
    /// </summary>
    internal void AddMeeting(string subject, DateTime dateTime, string location)
    {
        string newText = $"\n{_meetingDisplayTextMesh.text}\n\n Meeting,\nSubject: {subject},\nToday at {dateTime},\nLocation: {location}";

        _meetingDisplayTextMesh.text = newText;
    }
}
