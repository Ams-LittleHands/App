using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    const float FADE_TIME = 0.06f;

    Animator m_animator = null;

    struct AnimationTrack
    {
        public float start;
        public float end;

        public AnimationTrack(float start, float end)
        {
            this.start = start;
            this.end = end;
        }

        public void Normalized(float count)
        {
            this.start /= count;
            this.end /= count;
        }
    }

    struct TrackInfo
    {
        public int index;
        public float fadeTime;

        public TrackInfo(int index, float fadeTime)
        {
            this.index = index;
            this.fadeTime = fadeTime;
        }

        public TrackInfo(int index)
        {
            this.index = index;
            this.fadeTime = FADE_TIME;
        }
    }

    Dictionary<string, int> m_wordToTrackId = new Dictionary<string, int>
    {
        { "xin chào",       1 },
        { "cười",           2 },
        { "các bạn",        3 },
        { "muốn",           4 },
        { "đi",             5 },
        { "công viên",      6 },
        //{ "có",             7 },
        //{ "không",          7 },
        { "có_không",       7 },
        { "chúng tôi",      8 },
        { "bao giờ",        9 },
        { "buổi chiều",     10 },
        { "cảm ơn",         11 },
        { "rất vui được gặp bạn",     12 },

        { "demo1",     13 },
        { "demo2",     14 },
        { "demo3",     15 },
    };

    const int TOTAL_FRAMES = 1454;

    AnimationTrack[] m_tracks = {
        // idle
        new AnimationTrack(56, 59),

        // remain
        new AnimationTrack(0, 55),
        new AnimationTrack(60, 77),
        new AnimationTrack(82, 108),
        new AnimationTrack(113, 160),
        new AnimationTrack(165, 208),
        new AnimationTrack(213, 271),
        new AnimationTrack(276, 320),
        new AnimationTrack(325, 275),
        new AnimationTrack(380, 419),
        new AnimationTrack(424, 455),
        new AnimationTrack(460, 530),
        new AnimationTrack(535, 605),

        new AnimationTrack(700, 1120),
        new AnimationTrack(1125, 1295),
        new AnimationTrack(1300, 1453),
    };

    AnimationTrack m_curTrack;
    int m_curTrackIndex = 0;

    Queue<TrackInfo> m_serialAnimTrackId = new Queue<TrackInfo>();
    float m_curTrackFadeTime = 0;

    string m_rootPath = null;

    public AudioSource m_audioSource = null;

    long m_lastTimeSetTrack = 0;

    int m_playCount = 0;

    bool m_isPlayingAnim = true;

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();

        for (int i = 0; i < m_tracks.Length; i++)
        {
            m_tracks[i].Normalized(TOTAL_FRAMES);
        }

        m_curTrack = m_tracks[0];

        m_audioSource = GameObject.Find("AudioSource").GetComponent<AudioSource>();

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

        StopAnim();
        StartCoroutine(_StartAnim());
    }

    IEnumerator _StartAnim()
    {
        yield return new WaitForSeconds(0.5f);

        ResumeAnim();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_isPlayingAnim)
        {
            return;
        }

        var info = m_animator.GetCurrentAnimatorStateInfo(0);

        var now = Environment.TickCount;
        //if ((now - m_lastTimeSetTrack) / 1000.0f >= info.length * m_curTrackFadeTime)
        {
            var curTime = info.normalizedTime;
            if (curTime >= m_curTrack.end)
            {
                if (m_serialAnimTrackId.Count > 0)
                {
                    var trackId = m_serialAnimTrackId.Dequeue();
                    SetTrack(trackId.index, trackId.fadeTime);
                    Debug.Log("SetTrack: " + trackId.index);

                    if (m_serialAnimTrackId.Count == 0)
                    {
                        Debug.Log("=======================> end serial");
                    }
                }
                else
                {
                    m_animator.CrossFade("none", 0.00f, 0, m_curTrack.start, 0.0f);
                }
            }
        }

        //if (Input.GetKeyUp(KeyCode.Alpha1))
        //{
        //    PlaySerial("các bạn có muốn đi công viên không");
        //}

        //if (Input.GetKeyUp(KeyCode.Alpha2))
        //{
        //    PlaySerial("buổi chiều");
        //}

        //if (Input.GetKeyUp(KeyCode.P))
        //{
        //    StartCoroutine(RequestTextToSpeech("các bạn có muốn đi công viên không"));
        //}

        //if (Input.touchCount > 0)
        //{
        //    Touch touch = Input.GetTouch(0);

        //    switch (touch.phase)
        //    {
        //        case TouchPhase.Began:
        //            if (m_playCount == 0)
        //            {
        //                PlaySerial("các bạn có muốn đi công viên không");
        //            }
        //            if (m_playCount == 1)
        //            {
        //                PlaySerial("buổi chiều");
        //            }
        //            break;

        //        case TouchPhase.Moved:
        //            break;

        //        case TouchPhase.Ended:
        //            m_playCount++;
        //            break;
        //    }
        //}

        //for (int i = 0; i < m_tracks.Length; i++)
        //{
        //    if (Input.GetKeyUp(KeyCode.Alpha0 + i))
        //    {
        //        SetTrack(i);
        //    }
        //}
    }

    void SetTrack(int index, float fadeTime = FADE_TIME)
    {
        if (index == m_curTrackIndex)
        {
            return;
        }

        m_lastTimeSetTrack = Environment.TickCount;
        m_curTrack = m_tracks[index];
        m_curTrackIndex = index;
        m_curTrackFadeTime = fadeTime;
        m_animator.CrossFade("none", 0, 0, m_curTrack.start, 0.0f);
    }

    public void PlaySerialLocal(string str)
    {
        str = str.ToLower();
        Debug.Log("PlaySerial: " + str);

        var wordsList = new List<string>(str.Split(' '));

        var idxCo = wordsList.IndexOf("có");
        var idxKo = wordsList.IndexOf("không");
        if (idxCo >= 0 && idxKo >= 0 && idxKo > idxCo)
        {
            wordsList.RemoveAt(idxCo);
            wordsList.RemoveAt(idxKo - 1);
            m_serialAnimTrackId.Enqueue(new TrackInfo(7));
        }

        var words = wordsList.ToArray();

        for (int i = 0; i < words.Length; i++)
        {
            var word0 = words[i];
            int value = -1;

            if (m_wordToTrackId.TryGetValue(word0, out value))
            {
                m_serialAnimTrackId.Enqueue(new TrackInfo(value));
                continue;
            }

            if (i < words.Length - 4)
            {
                if (word0.Equals("rất")
                    && words[i + 1].Equals("vui")
                    && words[i + 2].Equals("được")
                    && words[i + 3].Equals("gặp")
                    && words[i + 4].Equals("bạn")
                    )
                {
                    m_serialAnimTrackId.Enqueue(new TrackInfo(12));
                    continue;
                }
            }

            if (i != words.Length - 1)
            {

                var word1 = words[i + 1];
                var compoundWord = word0 + " " + word1;
                if (m_wordToTrackId.TryGetValue(compoundWord, out value))
                {
                    m_serialAnimTrackId.Enqueue(new TrackInfo(value));
                    continue;
                }
            }
        }

        m_serialAnimTrackId.Enqueue(new TrackInfo(0));

        var arr = m_serialAnimTrackId.ToArray();
        string log = "";
        for (int i = 0; i < arr.Length; i++)
        {
            log += arr[i].index + ", ";
        }
        Debug.Log(log);
    }

    IEnumerator RequestTextToSignKey(string str)
    {
        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty, Encoding.Unicode);
        queryString.Add("input_str", str);

        //var str1 = HttpUtility.UrlEncode("{\"text\":\"" + str + "\"}");

        Debug.Log("=======================> RequestTextToSignKey: "
            + "http://222.252.4.92:3010/sign_lang_translate/?" + queryString.ToString());

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        UnityWebRequest www = UnityWebRequest.Post("http://222.252.4.92:3010/sign_lang_translate/?" + queryString.ToString(), formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var json1 = JObject.Parse(www.downloadHandler.text);
            var keys = (JArray)json1.GetValue("translation");

            Debug.Log("RequestTextToSignKey done: " + json1);

            for (int i = 0; i < keys.Count; i++)
            {
                var value = keys[i].ToString();
                int keyId;
                if (m_wordToTrackId.TryGetValue(value, out keyId))
                {
                    m_serialAnimTrackId.Enqueue(new TrackInfo(keyId));
                }
            }

            m_serialAnimTrackId.Enqueue(new TrackInfo(0));

            var arr = m_serialAnimTrackId.ToArray();
            string log = "";
            for (int i = 0; i < arr.Length; i++)
            {
                log += arr[i].index + ", ";
            }
            Debug.Log(log);
        }
    }

    public void PlaySerial(string str)
    {
        StartCoroutine(RequestTextToSignKey(str));
    }

    public IEnumerator RequestTextToSpeech(string str, Action onSpeechBeginPlay = null)
    {
        NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty, Encoding.Unicode);
        queryString.Add("text", str);

        //var str1 = HttpUtility.UrlEncode("{\"text\":\"" + str + "\"}");

        Debug.Log("=======================> RequestTextToSpeech: "
            + "http://222.252.4.92:9091/transformTextToSpeech/?" + queryString.ToString());

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        UnityWebRequest www = UnityWebRequest.Post("http://api-detect-app.aiotlab-annotation.com/transformTextToSpeech/?" + queryString.ToString(), formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("RequestTextToSpeech done!!!");
            //var data = www.downloadHandler.text;
            //Debug.Log("=======================> " + www.downloadHandler.data);

            var filePath = m_rootPath + "audio.mp3";
            File.WriteAllBytes(filePath, www.downloadHandler.data);
            //var json1 = JObject.Parse(data);

            //Debug.Log(json1.GetValue("message").ToString());

            //dynamic json = JObject.Parse(json1.GetValue("message").ToString());
            //string audioLink = json.result.audio_link;
            yield return PlayAudioFromUrl("file://" + filePath, onSpeechBeginPlay);
        }
    }

    IEnumerator PlayAudioFromUrl(string url, Action onSpeechBeginPlay)
    {
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            if (onSpeechBeginPlay != null) onSpeechBeginPlay();
            m_audioSource.PlayOneShot(DownloadHandlerAudioClip.GetContent(www));
        }
    }

    IEnumerator DownloadThenAudioFromUrl(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("DownloadAudio done!!!");
            var filePath = m_rootPath + "audio.mp3";
            File.WriteAllBytes(filePath, www.downloadHandler.data);

            UnityWebRequest www1 = UnityWebRequestMultimedia.GetAudioClip(string.Format("file://{0}", filePath), AudioType.MPEG);
            yield return www1.SendWebRequest();

            m_audioSource.PlayOneShot(DownloadHandlerAudioClip.GetContent(www1));
        }
    }

    public void StopAnim()
    {
        m_animator.enabled = false;

        m_isPlayingAnim = false;
    }

    public void ResumeAnim()
    {
        m_animator.enabled = true;

        //SetTrack(0);

        m_isPlayingAnim = true;
    }

}
