using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    #region 绑主菜单按钮和功能的
    [SerializeField] private Button StartButton;
    [SerializeField] private Button ExitButton;
    
    //else
    [SerializeField] private Button ReturnToMenuButton;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        StartButton.onClick.AddListener(StartGame);
        ExitButton.onClick.AddListener(Exit);

        if(ReturnToMenuButton != null)
        ReturnToMenuButton.onClick.AddListener(ReturnToMenu);
    }

    //开始
    public void StartGame()
    {
        SceneManager.LoadScene("L1");
    }

    //设置
    //public void Setting()
    //{
    //    SettingPanel.SetActive(true);
    //}

    //退出
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion

    #region 懒得开新脚本，把其他panel的按钮功能也绑了

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");

    }

  

    #endregion


}
