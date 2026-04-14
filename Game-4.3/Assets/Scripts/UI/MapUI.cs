using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapUI : MonoBehaviour
{
    public static MapUI Instance { get; private set; }

    public Button level_Cao;
    public Button level_Sen;
    public Button level_Geb;

    public Image Card_Cao;
    public Image Card_Sen;
    public Image Card_Geb;

    public Button enterButton_Cao;//告别字幕
    public Button enterButton_Sen;
    public Button enterButton_Geb;
    public static int passed = 0;

    // 每个地点对应的可放归动物列表
    private List<string> caoYuanAnimals = new List<string> { "虫子", "大象" };
    private List<string> senLinAnimals = new List<string> { "猎豹" };
    private List<string> geBiAnimals = new List<string> { "骆驼" };

    public Image win;
    public Button winButton;
    public AudioSource bgm;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        HideAllPreviews();
        UnlockNextLevel();

        level_Cao.onClick.AddListener(() => FanSheng(1));
        level_Sen.onClick.AddListener(() => FanSheng(2));
        level_Geb.onClick.AddListener(() => FanSheng(3));

        enterButton_Cao.onClick.AddListener(() => JumpToNextLevel(1));
        enterButton_Sen.onClick.AddListener(() => JumpToNextLevel(2));
        enterButton_Geb.onClick.AddListener(() => JumpToNextLevel(3));

        winButton.onClick.AddListener(Win);
    }

    void HideAllPreviews()
    {
        level_Cao.gameObject.SetActive(false);
        level_Sen.gameObject.SetActive(false);
        level_Geb.gameObject.SetActive(false);
        enterButton_Cao.gameObject.SetActive(false);
        enterButton_Geb.gameObject.SetActive(false);
        enterButton_Sen.gameObject.SetActive(false);

        Card_Cao.gameObject.SetActive(false);
        Card_Sen.gameObject.SetActive(false);
        Card_Geb.gameObject.SetActive(false);

        win.gameObject.SetActive(false);
    }

    public void UnlockNextLevel()
    {
        Debug.Log(passed);
        if (passed == 1)
        {
            //为啥不绑一起，因为图层不一样
            level_Cao.gameObject.SetActive(true);
            Card_Cao.gameObject.SetActive(true);
        }
        else if(passed==2)
        {
            level_Sen.gameObject.SetActive(true);
            Card_Sen.gameObject.SetActive(true);

        }
        else if (passed == 3)
        {
            level_Geb.gameObject.SetActive(true);
            Card_Geb.gameObject.SetActive(true);
        }
    }

    public void FanSheng(int level)
    {
        // 执行放归：将该地点的所有可放归动物从牌组中永久移除
        ReleaseAnimalsAtLocation(level);

        // 先隐藏所有 Enter 按钮
        enterButton_Cao.gameObject.SetActive(false);
        enterButton_Sen.gameObject.SetActive(false);
        enterButton_Geb.gameObject.SetActive(false);

        // 显示对应的 Enter 按钮
        switch (level)
        {
            case 1:
                ShowEnterButtonWithDelay(enterButton_Cao);
                break;
            case 2:
                ShowEnterButtonWithDelay(enterButton_Sen);
                break;
            case 3:
                ShowEnterButtonWithDelay(enterButton_Geb);
                break;
        }
    }

    /// <summary>
    /// 放归对应地点的所有动物
    /// </summary>
    void ReleaseAnimalsAtLocation(int locationId)
    {
        if (DeckManager.Instance == null)
        {
            Debug.LogError("DeckManager 不存在！");
            return;
        }

        List<string> animalsToRelease = new List<string>();

        switch (locationId)
        {
            case 1: // 丛林
                animalsToRelease = senLinAnimals;
                break;
            case 2: // 草原
                animalsToRelease = caoYuanAnimals;

                break;
            case 3: // 戈壁
                animalsToRelease = geBiAnimals;
                break;
        }

        foreach (string animalName in animalsToRelease)
        {
            DeckManager.Instance.ReleaseAnimal(animalName, locationId);
            Debug.Log($"已放归 {animalName}，从牌组中永久移除");
        }
    }

    void ShowEnterButtonWithDelay(Button enterButton)
    {
        enterButton.gameObject.SetActive(true);
        enterButton.interactable = false;
        StartCoroutine(EnableButtonAfterDelay(enterButton, 2f));
    }

    IEnumerator EnableButtonAfterDelay(Button button, float delay)
    {
        yield return new WaitForSeconds(delay);
        button.interactable = true;
    }

    public void JumpToNextLevel(int level)
    {
        Debug.Log("进入关卡：" + (level + 1));
        if (level < 3)
        {
            bgm.Stop();
            SceneManager.LoadScene(level + 1);
            
        }
        else
        {
            win.gameObject.SetActive(true);
        }
    }

    public void Win()
    {
        bgm.Stop();
        SceneManager.LoadScene("Menu");
    }
}