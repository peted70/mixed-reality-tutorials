using System.Collections.Generic;
using UnityEngine;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;

#if !UNITY_EDITOR && UNITY_WSA
using System.Net.Http;
using System.Net.Http.Headers;
using Windows.Storage;
#endif

public class Graph : MonoBehaviour
{
    /// <summary>
    /// Insert your Application Id here
    /// </summary>
    private string _appId = "222d1182-6967-4f2b-899d-0cfc465b5bad";

    /// <summary>
    /// Application scopes, determine Microsoft Graph accessibility level to user account
    /// </summary>
    private IEnumerable<string> _scopes = new List<string>() { "User.Read", "Calendars.Read" };

    /// <summary>
    /// Microsoft Graph API, user reference
    /// </summary>
    private PublicClientApplication _client;

    /// <summary>
    /// Microsoft Graph API, authentication
    /// </summary>
    private AuthenticationResult _authResult;

    /// <summary>
    /// Begin the Sign In process using Microsoft Graph Library
    /// </summary>
    internal async void SignInAsync()
    {
#if !UNITY_EDITOR && UNITY_WSA
        // Set up Grap user settings, determine if needs auth
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        string userId = localSettings.Values["UserId"] as string;
        _client = new PublicClientApplication(_appId);

        // Attempt authentication
        _authResult = await AcquireTokenAsync(_client, _scopes, userId);

        // If authentication is successfull, retrieve the meetings
        if (!string.IsNullOrEmpty(_authResult.AccessToken))
        {
            // Once Auth as been completed, find the meetings for the day
            await ListMeetingsAsync(_authResult.AccessToken);
        }
#endif
    }

    /// <summary>
    /// Attempt to retrieve the Access Token by either retrieving
    /// previously stored credentials or by prompting user to Login
    /// </summary>
    private async Task<AuthenticationResult> AcquireTokenAsync(
        IPublicClientApplication app, IEnumerable<string> scopes, string userId)
    {
        IUser user = !string.IsNullOrEmpty(userId) ? app.GetUser(userId) : null;
        string userName = user != null ? user.Name : "null";

        // Once the User name is found, display it as a welcome message
        MeetingsUI.Instance.WelcomeUser(userName);

        // Attempt to Log In the user with a pre-stored token. Only happens
        // in case the user Logged In with this app on this device previously
        try
        {
            _authResult = await app.AcquireTokenSilentAsync(scopes, user);
        }
        catch (MsalUiRequiredException)
        {
            // Pre-stored token not found, prompt the user to log-in 
            try
            {
                _authResult = await app.AcquireTokenAsync(scopes);
            }
            catch (MsalException msalex)
            {
                Debug.Log($"Error Acquiring Token: {msalex.Message}");
                return _authResult;
            }
        }

        MeetingsUI.Instance.WelcomeUser(_authResult.User.Name);

#if !UNITY_EDITOR && UNITY_WSA
        ApplicationData.Current.LocalSettings.Values["UserId"] = 
        _authResult.User.Identifier;
#endif
        return _authResult;
    }

    /// <summary>
    /// Build the endpoint to retrieve the meetings for the current day.
    /// </summary>
    /// <returns>Returns the Calendar Endpoint</returns>
    public string BuildTodayCalendarEndpoint()
    {
        DateTime startOfTheDay = DateTime.Today.AddDays(0);
        DateTime endOfTheDay = DateTime.Today.AddDays(1);
        DateTime startOfTheDayUTC = startOfTheDay.ToUniversalTime();
        DateTime endOfTheDayUTC = endOfTheDay.ToUniversalTime();

        string todayDate = startOfTheDayUTC.ToString("o");
        string tomorrowDate = endOfTheDayUTC.ToString("o");
        string todayCalendarEndpoint = string.Format(
            "https://graph.microsoft.com/v1.0/me/calendarview?startdatetime={0}&enddatetime={1}",
            todayDate,
            tomorrowDate);

        return todayCalendarEndpoint;
    }

    /// <summary>
    /// Request all the scheduled meetings for the current day.
    /// </summary>
    private async Task ListMeetingsAsync(string accessToken)
    {
#if !UNITY_EDITOR && UNITY_WSA
        var http = new HttpClient();

        http.DefaultRequestHeaders.Authorization = 
        new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await http.GetAsync(BuildTodayCalendarEndpoint());
         
        var jsonResponse = await response.Content.ReadAsStringAsync();

        Rootobject rootObject = new Rootobject();
        try
        {
            // Parse the JSON response.
            rootObject = JsonUtility.FromJson<Rootobject>(jsonResponse);

            // Sort the meeting list by starting time.
            rootObject.value.Sort((x, y) => DateTime.Compare(x.start.StartDateTime, y.start.StartDateTime));

            // Populate the UI with the meetings.
            for (int i = 0; i < rootObject.value.Count; i++)
            {
                MeetingsUI.Instance.AddMeeting(rootObject.value[i].subject,
                                            rootObject.value[i].start.StartDateTime.ToLocalTime(),
                                            rootObject.value[i].location.displayName);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error = {ex.Message}");
            return;
        }
#endif
    }
}

/// <summary>
/// The object hosting the scheduled meetings
/// </summary>
[Serializable]
public class Rootobject
{
    public List<Value> value;
}

[Serializable]
public class Value
{
    public string subject;
    public StartTime start;
    public Location location;
}

[Serializable]
public class StartTime
{
    public string dateTime;

    private DateTime? _startDateTime;
    public DateTime StartDateTime
    {
        get
        {
            if (_startDateTime != null)
                return _startDateTime.Value;
            DateTime dt;
            DateTime.TryParse(dateTime, out dt);
            _startDateTime = dt;
            return _startDateTime.Value;
        }
    }
}

[Serializable]
public class Location
{
    public string displayName;
}
