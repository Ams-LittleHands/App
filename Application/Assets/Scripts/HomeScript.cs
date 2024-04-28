using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScript : MonoBehaviour
{
    public Button mainButton;

    void Start()
    {
        // Gán sự kiện cho nút ghi âm
        mainButton.onClick.AddListener(LoadMainScene);
    }

    // Start is called before the first frame update
    void LoadMainScene() {
        SceneManager.LoadScene(1);
    }
}
