using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class LuisManager : MonoBehaviour
{
    public static LuisManager instance;

    //Substitute the value of luis Endpoint with your own End Point
    string luisEndpoint;

    private void Awake()
    {
        string path = "Assets/Resources/luis.lic";

        //Read the text from directly from the test.txt file
        using (StreamReader reader = new StreamReader(path))
        {
            luisEndpoint = reader.ReadToEnd();
        }

        // allows this class instance to behave like a singleton
        instance = this;
    }

    /// <summary>
    /// Call LUIS to submit a dictation result.
    /// </summary>
    public IEnumerator SubmitRequestToLuis(string dictationResult)
    {
        WWWForm webForm = new WWWForm();

        string queryString;

        queryString = string.Concat(Uri.EscapeDataString(dictationResult));

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(luisEndpoint + queryString))
        {
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return unityWebRequest.SendWebRequest();

            long responseCode = unityWebRequest.responseCode;

            try
            {
                using (Stream stream = GenerateStreamFromString(unityWebRequest.downloadHandler.text))
                {
                    StreamReader reader = new StreamReader(stream);

                    AnalysedQuery analysedQuery = new AnalysedQuery();

                    analysedQuery = JsonUtility.FromJson<AnalysedQuery>(unityWebRequest.downloadHandler.text);

                    //analyse the elements of the response 
                    AnalyseResponseElements(analysedQuery);
                }
            }
            catch (Exception exception)
            {
                Debug.Log("Luis Request Exception Message: " + exception.Message);
            }

            yield return null;
        }
    }

    public static Stream GenerateStreamFromString(string receivedString)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(receivedString);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private void AnalyseResponseElements(AnalysedQuery aQuery)
    {
        string topIntent = aQuery.topScoringIntent.intent;

        // Create a dictionary of entities associated with their type
        Dictionary<string, string> entityDic = new Dictionary<string, string>();

        foreach (EntityData ed in aQuery.entities)
        {
            entityDic.Add(ed.type, ed.entity);
        }

        // Depending on the topmost recognised intent, read the entities name
        switch (aQuery.topScoringIntent.intent)
        {
            case "ChangeObjectColor":
                string targetForColor = null;
                string color = null;

                foreach (var pair in entityDic)
                {
                    if (pair.Key == "target")
                    {
                        targetForColor = pair.Value;
                    }
                    else if (pair.Key == "color")
                    {
                        color = pair.Value;
                    }
                }

                Behaviours.instance.ChangeTargetColor(targetForColor, color);
                break;

            case "ChangeObjectSize":
                string targetForSize = null;
                foreach (var pair in entityDic)
                {
                    if (pair.Key == "target")
                    {
                        targetForSize = pair.Value;
                    }
                }

                if (entityDic.ContainsKey("upsize") == true)
                {
                    Behaviours.instance.UpSizeTarget(targetForSize);
                }
                else if (entityDic.ContainsKey("downsize") == true)
                {
                    Behaviours.instance.DownSizeTarget(targetForSize);
                }
                break;
        }
    }

    [System.Serializable] //this class represents the LUIS response
    public class AnalysedQuery
    {
        public TopScoringIntentData topScoringIntent;
        public EntityData[] entities;
        public string query;
    }

    // This class contains the Intent LUIS determines 
    // to be the most likely
    [System.Serializable]
    public class TopScoringIntentData
    {
        public string intent;
        public float score;
    }

    // This class contains data for an Entity
    [System.Serializable]
    public class EntityData
    {
        public string entity;
        public string type;
        public int startIndex;
        public int endIndex;
        public float score;
    }
}
