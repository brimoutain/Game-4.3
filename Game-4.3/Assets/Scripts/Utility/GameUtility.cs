using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtility
{
    // ========== 随机抽牌相关 ==========

    /// <summary>
    /// 从列表中随机抽取一张牌（不移除）
    /// </summary>
    public static T GetRandomCard<T>(List<T> cardList)
    {
        if (cardList == null || cardList.Count == 0)
            return default(T);

        int randomIndex = UnityEngine.Random.Range(0, cardList.Count);
        return cardList[randomIndex];
    }

    /// <summary>
    /// 从列表中随机抽取一张牌（移除）
    /// </summary>
    public static T DrawRandomCard<T>(List<T> cardList)
    {
        if (cardList == null || cardList.Count == 0)
            return default(T);

        int randomIndex = UnityEngine.Random.Range(0, cardList.Count);
        T card = cardList[randomIndex];
        cardList.RemoveAt(randomIndex);
        return card;
    }

    /// <summary>
    /// 从列表中随机抽取多张牌（不重复，不移除）
    /// </summary>
    public static List<T> GetRandomCards<T>(List<T> cardList, int count)
    {
        List<T> result = new List<T>();
        if (cardList == null || cardList.Count == 0)
            return result;

        // 创建副本
        List<T> tempList = new List<T>(cardList);

        for (int i = 0; i < count && tempList.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, tempList.Count);
            result.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        return result;
    }

    /// <summary>
    /// 从列表中随机抽取多张牌（不重复，移除）
    /// </summary>
    public static List<T> DrawRandomCards<T>(List<T> cardList, int count)
    {
        List<T> result = new List<T>();
        for (int i = 0; i < count && cardList.Count > 0; i++)
        {
            result.Add(DrawRandomCard(cardList));
        }
        return result;
    }

    // ========== 保存/读取数据相关 ==========

    /// <summary>
    /// 保存 int 数据
    /// </summary>
    public static void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 读取 int 数据
    /// </summary>
    public static int LoadInt(string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt(key, defaultValue);
    }

    /// <summary>
    /// 保存 float 数据
    /// </summary>
    public static void SaveFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 读取 float 数据
    /// </summary>
    public static float LoadFloat(string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat(key, defaultValue);
    }

    /// <summary>
    /// 保存 string 数据
    /// </summary>
    public static void SaveString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 读取 string 数据
    /// </summary>
    public static string LoadString(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(key, defaultValue);
    }

    /// <summary>
    /// 保存 bool 数据
    /// </summary>
    public static void SaveBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 读取 bool 数据
    /// </summary>
    public static bool LoadBool(string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    /// <summary>
    /// 检查是否存在某个键
    /// </summary>
    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    /// <summary>
    /// 删除指定数据
    /// </summary>
    public static void DeleteKey(string key)
    {
        PlayerPrefs.DeleteKey(key);
    }

    /// <summary>
    /// 删除所有数据
    /// </summary>
    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
    }

    // ========== 列表序列化辅助（保存/读取 List）==========

    /// <summary>
    /// 保存字符串列表
    /// </summary>
    public static void SaveStringList(string key, List<string> list)
    {
        string json = JsonUtility.ToJson(new StringListWrapper { list = list });
        SaveString(key, json);
    }

    /// <summary>
    /// 读取字符串列表
    /// </summary>
    public static List<string> LoadStringList(string key)
    {
        string json = LoadString(key);
        if (string.IsNullOrEmpty(json))
            return new List<string>();

        StringListWrapper wrapper = JsonUtility.FromJson<StringListWrapper>(json);
        return wrapper.list ?? new List<string>();
    }

    // 包装类用于序列化
    [Serializable]
    private class StringListWrapper
    {
        public List<string> list;
    }

    // ========== 其他实用工具 ==========

    /// <summary>
    /// 判断是否在数组中
    /// </summary>
    public static bool IsInArray<T>(T[] array, T value)
    {
        foreach (T item in array)
        {
            if (EqualityComparer<T>.Default.Equals(item, value))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 判断是否在列表中
    /// </summary>
    public static bool IsInList<T>(List<T> list, T value)
    {
        return list.Contains(value);
    }

    /// <summary>
    /// 获取随机 bool 值
    /// </summary>
    public static bool RandomBool()
    {
        return UnityEngine.Random.Range(0, 2) == 0;
    }
}