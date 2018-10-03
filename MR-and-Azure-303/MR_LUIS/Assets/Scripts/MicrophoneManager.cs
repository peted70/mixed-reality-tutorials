using UnityEngine;
using UnityEngine.Windows.Speech;

public class MicrophoneManager : MonoBehaviour
{
    public static MicrophoneManager instance; //help to access instance of this object
    private DictationRecognizer dictationRecognizer;  //Component converting speech to text
    public TextMesh dictationText; //a UI object used to debug dictation result

    private void Awake()
    {
        // allows this class instance to behave like a singleton
        instance = this;
    }

    void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            StartCapturingAudio();
            Debug.Log("Mic Detected");
        }
    }

    /// <summary>
    /// Start microphone capture, by providing the microphone as a continual audio source (looping),
    /// then initialise the DictationRecognizer, which will capture spoken words
    /// </summary>
    public void StartCapturingAudio()
    {
        dictationRecognizer = new DictationRecognizer
        {
            InitialSilenceTimeoutSeconds = 20,
            AutoSilenceTimeoutSeconds = 5
        };
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;
        dictationRecognizer.Start();
        Debug.Log("Capturing Audio...");
    }

    /// <summary>
    /// This handler is called every time the Dictation detects a pause in the speech. 
    /// </summary>
    private void DictationRecognizer_DictationResult(string dictationCaptured, ConfidenceLevel confidence)
    {
        StartCoroutine(LuisManager.instance.SubmitRequestToLuis(dictationCaptured));
        Debug.Log("Dictation: " + dictationCaptured);
        dictationText.text = dictationCaptured;
    }

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        Debug.Log("Dictation exception: " + error);
    }
}
