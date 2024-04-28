using FFmpegUnityBind2.Android;
using FFmpegUnityBind2.IOS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace FFmpegUnityBind2.Demo
{
    class MyRECCameraHandler : IFFmpegCallbacksHandler
    {
        DemoCaseRECCameraView m_myRECCamera = null;

        public MyRECCameraHandler(DemoCaseRECCameraView myRECCamera)
        {
            m_myRECCamera = myRECCamera;
        }

        public void OnCanceled(long executionId)
        {
        }

        public void OnError(long executionId, string message)
        {
        }

        public void OnFail(long executionId)
        {
        }

        public void OnLog(long executionId, string message)
        {
        }

        public void OnStart(long executionId)
        {
        }

        public void OnSuccess(long executionId)
        {
            m_myRECCamera.OnDoneCaptureCamera();
        }

        public void OnWarning(long executionId, string message)
        {
        }
    }

    class DemoCaseRECCameraView : DemoCaseRECBaseView
    {
        protected IFFmpegCallbacksHandler[] m_myHandlers { get; private set; }

        [SerializeField]
        protected Button voiceButton = null;

        [SerializeField]
        CameraView cameraView = null;

        [SerializeField]
        TextureView textureView = null;

        [SerializeField]
        GameObject m_loadingPanel = null;

        [SerializeField]
        GameObject m_displayText = null;

        [SerializeField]
        Image m_image = null;

        [SerializeField]
        TMP_Text m_statusCam = null;

        TMP_Text m_textComp = null;

        string m_rootPath = null;

        Test m_signlangScript = null;

        IEnumerator Upload()
        {
            //yield return m_signlangScript.RequestTextToSpeech("Xin Chào", () =>
            //    {
            //        m_loadingPanel.SetActive(false);
            //    });

            var fileName = "test.mp4";
            var path = m_rootPath + fileName;

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("file", File.ReadAllBytes(path), fileName, "video/mp4"));

            UnityWebRequest www = UnityWebRequest.Post("http://api-detect-app.aiotlab-annotation.com/uploadVideo/", formData);
            yield return www.SendWebRequest();

            m_signlangScript.gameObject.SetActive(true);
            m_signlangScript.ResumeAnim();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);

                m_loadingPanel.SetActive(false);
            }
            else
            {
                Debug.Log("UploadVideo done!!!");
                var data = www.downloadHandler.text;
                Debug.Log(data);
                //File.WriteAllText(Application.dataPath + "/../Temp/response.html", data);
                //m_textComp.SetText(data);

                // m_displayText.SetActive(true);
                //m_loadingPanel.SetActive(false);

                var json1 = JObject.Parse(data);

                yield return m_signlangScript.RequestTextToSpeech(json1.GetValue("message").ToString(), () =>
                {
                    m_loadingPanel.SetActive(false);
                });
            }
        }

        private void Start()
        {
            m_myHandlers = new IFFmpegCallbacksHandler[]
            {
                new ConsoleEventsHandler(),
                new MyRECCameraHandler(this)
            };

            // m_textComp = m_displayText.GetComponent<TMP_Text>();

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

            if (m_image == null)
            {
                m_image = GetComponent<UnityEngine.UI.Image>(); // Lấy đối tượng Image nếu nó chưa được gán
                
            }
            m_image.enabled = false;
        }

        protected override void OnStartCapturingButton(bool audio = true)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!ValidateAndroid())
            {
                return;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            if (!ValidateIOS())
            {
                return;
            }
#endif
            voiceButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(true);
            executeButton.gameObject.SetActive(false);
            // m_statusCam.text = "Camera: ON";

            // m_displayText.SetActive(false);

            //if (m_image != null)
            //{
            //    m_image.enabled = true;
            //}

            cameraView.Open();

            textureView.Open(cameraView.Texture);

            demoCaseSharedView.FFmpegREC.StartREC(m_rootPath + "test.mp4", RecAudioSource.Mic, m_myHandlers);

            //base.OnStartCapturingButton(false);

            // StartCoroutine(Upload());
        }

        protected override void OnStopCapturingButton(bool audio = true)
        {
            //if (m_image != null)
            //{
            //    m_image.enabled = false;
            //}

            cameraView.Close();
            textureView.Close();

            //if (textureView.image != null)
            //{
            //    textureView.image = null;
            //}
            voiceButton.gameObject.SetActive(true);
            stopButton.gameObject.SetActive(false);
            executeButton.gameObject.SetActive(true);
            // m_statusCam.text = "Camera: OFF";
            m_loadingPanel.SetActive(true);

            demoCaseSharedView.FFmpegREC.StopREC();

            demoCaseSharedView.FFmpegREC.GetComponent<Camera>().targetTexture = null;

            m_signlangScript.StopAnim();
            //m_signlangScript.gameObject.SetActive(false);

            //base.OnStopCapturingButton(audio);
        }

        protected override void OnCancelButton()
        {
            demoCaseSharedView.FFmpegREC.Cancel();

            base.OnCancelButton();

            cameraView.Close();
            textureView.Close();
        }

        public void OnDoneCaptureCamera()
        {
            StartCoroutine(Upload());
            //Fake();
        }

        IEnumerator _RetryOnStartCapturingButton(bool audio = true)
        {
            yield return new WaitForSeconds(0.2f);

            OnStartCapturingButton();
        }

#if UNITY_ANDROID && !UNITY_EDITOR

        internal void PermissionCallbacks_PermissionDeniedAndDontAskAgain(string permissionName)
        {
            PermissionCallbacks_PermissionDenied(permissionName);
        }

        internal void PermissionCallbacks_PermissionGranted(string permissionName)
        {
            Debug.Log("PermissionCallbacks_PermissionGranted");
            StartCoroutine(_RetryOnStartCapturingButton());
        }

        internal void PermissionCallbacks_PermissionDenied(string permissionName)
        {
            m_textComp.SetText("Permission Denied!");
            // m_displayText.SetActive(true);

            if (m_image != null)
            {
                m_image.enabled = true;
            }
        }

        bool ValidateAndroid()
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
            callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
            callbacks.PermissionDeniedAndDontAskAgain += PermissionCallbacks_PermissionDeniedAndDontAskAgain;

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera, callbacks);
                return false;
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone, callbacks);
                return false;
            }

            return true;
        }

#endif

#if UNITY_IOS && !UNITY_EDITOR
        bool ValidateIOS()
        {
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                Application.RequestUserAuthorization(UserAuthorization.WebCam);
                return false;
            }

            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
                return false;
            }

            return true;
        }
#endif

        IEnumerator PlayAudio(string filePath)
        {
            UnityWebRequest www1 = UnityWebRequestMultimedia.GetAudioClip(string.Format("file://{0}", filePath), AudioType.MPEG);
            yield return www1.SendWebRequest();

            m_signlangScript.m_audioSource.PlayOneShot(DownloadHandlerAudioClip.GetContent(www1));
        }

        int m_countFake = 0;

        void Fake()
        {
            //m_signlangScript.gameObject.SetActive(true);
            m_signlangScript.ResumeAnim();
            m_loadingPanel.SetActive(false);
            m_countFake++;
            if (m_countFake == 1)
            {
                StartCoroutine(PlayAudio(Application.dataPath + "/Sounds/RatVuiDuocGapBan.mp3"));
            }
            else if (m_countFake == 2)
            {
                StartCoroutine(PlayAudio(Application.dataPath + "/Sounds/ToiCoMuonMayGioDiNhi.mp3"));
            }
        }
    }
}