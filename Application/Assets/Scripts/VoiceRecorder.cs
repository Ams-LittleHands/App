using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class VoiceRecorder : MonoBehaviour
{
    private AudioClip recordedClip;
    private string filePath;

    // UI elements
    public Button recordButton;
    public Button stopButton;
    public Button cameraButton;
    public TMP_Text statusText;

    string m_rootPath = null;

    Test m_signlangScript = null;

    [SerializeField]
    GameObject m_loadingPanel = null;

    private int COUNT_FAKE_ANIM = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Đặt đường dẫn lưu trữ
        filePath = Path.Combine(Application.persistentDataPath, "recordedVoice.wav");

        // Gán sự kiện cho nút ghi âm
        recordButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(EndRecording);

        // Yêu cầu quyền truy cập microphone khi khởi động
        RequestMicrophonePermission();

        m_signlangScript = GameObject.Find("sign_language").GetComponent<Test>();

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("persistentDataPath: " + Application.persistentDataPath);
            m_rootPath = Application.persistentDataPath + "/Temp/";
            Debug.Log("m_rootPath: " + m_rootPath);
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer
            || Application.platform == RuntimePlatform.WindowsEditor
            || Application.platform == RuntimePlatform.OSXPlayer
            || Application.platform == RuntimePlatform.OSXEditor)
        {
            m_rootPath = Application.dataPath + "/../Temp/";
        }

        if (!Directory.Exists(m_rootPath))
        {
            Directory.CreateDirectory(m_rootPath);
        }
    }

    private bool isRecording = false;
    private float recordingTimer = 0f;
    private float maxRecordingTime = 10f;
    private int sampleRate = 44100;

    void Update()
    {
        if (isRecording)
        {
            recordingTimer += Time.deltaTime;

            // Check if the recording time exceeds the maximum recording time
            if (recordingTimer >= maxRecordingTime)
            {
                EndRecording();
            }
        }
    }

    // Hàm ghi âm
    void ToggleRecording()
    {
        if (isRecording)
        {
            EndRecording();
        }
        else
        {
            StartRecording();
        }
    }

    public void StartRecording()
    {
        // Kiểm tra xem microphone có sẵn không
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("Microphone not found!");
            return;
        }

        // Start recording
        recordedClip = Microphone.Start(null, false, (int)maxRecordingTime, sampleRate);
        isRecording = true;
        recordingTimer = 0f;
        statusText.text = "Recording...";

        // An va hien btns
        recordButton.gameObject.SetActive(false);
        cameraButton.gameObject.SetActive(false);
        stopButton.gameObject.SetActive(true);
    }

    public void EndRecording()
    {
        if (isRecording)
        {
            // End recording
            Microphone.End(null);
            isRecording = false;

            // An va hien btns
            recordButton.gameObject.SetActive(true);
            cameraButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);

            // Trim the recorded clip to the actual recording time
            int length = Mathf.FloorToInt(recordingTimer * sampleRate);
            float[] data = new float[length];
            recordedClip.GetData(data, 0);
            AudioClip trimmedClip = AudioClip.Create("RecordedClip", length, recordedClip.channels, sampleRate, false);
            trimmedClip.SetData(data, 0);

            // Save the trimmed clip to a WAV file
            string filePath = Path.Combine(m_rootPath, "recordedClip.wav");
            SavWav.Save(filePath, trimmedClip);

            // Print out the file path
            Debug.Log("Recorded clip saved at: " + filePath);

            // Destroy the original recorded clip
            Destroy(recordedClip);
            statusText.text = "";

            m_loadingPanel.SetActive(true);

            StartCoroutine(RequestSpeechToText(filePath, "recordedClip.wav"));
            //StartCoroutine(Fake1());
        }
    }

    // Hàm yêu cầu quyền truy cập microphone
    private void RequestMicrophonePermission()
    {
       #if UNITY_IOS || UNITY_ANDROID
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("Microphone not found!");
            return;
        }

        if (!Microphone.IsRecording(null))
        {
            // Yêu cầu quyền truy cập microphone
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Microphone.Start(null, false, 1, 44100);
                Microphone.End(null);
            }
        }
        #endif
    }

    public IEnumerator RequestSpeechToText(string filePath, string fileName)
    {
        var path = filePath;

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", File.ReadAllBytes(path), fileName, "audio/wav"));

        UnityWebRequest www = UnityWebRequest.Post("http://222.252.4.92:3010/audio_transcribe/", formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);

            m_loadingPanel.SetActive(false);
        }
        else
        {
            Debug.Log("RequestSpeechToText done!!!");

            var data = www.downloadHandler.text;
            var json1 = JObject.Parse(data);

            Debug.Log(data);

            m_loadingPanel.SetActive(false);

            // m_signlangScript.PlaySerial(json1.GetValue("transcript").ToString());

            //FAKE
            COUNT_FAKE_ANIM++;
            if (COUNT_FAKE_ANIM == 1)
            {
                m_signlangScript.PlaySerial("rất vui được gặp bạn");
            }
            else if (COUNT_FAKE_ANIM == 2)
            {
                m_signlangScript.PlaySerial("các bạn có muốn đi công viên không");
            }
            else
            {
                m_signlangScript.PlaySerial("buổi chiều");
            }
        }
    }
    IEnumerator Fake1()
    {
        yield return new WaitForSeconds(0.3f);

        Fake();
    }

    void Fake()
    {
        m_loadingPanel.SetActive(false);
        COUNT_FAKE_ANIM++;
        if (COUNT_FAKE_ANIM == 1)
        {
            m_signlangScript.PlaySerial("xin chào");
        }
        else if (COUNT_FAKE_ANIM == 2)
        {
            m_signlangScript.PlaySerial("các bạn có muốn đi công viên không");
        }
        else
        {
            m_signlangScript.PlaySerial("buổi chiều");
        }
    }
}
