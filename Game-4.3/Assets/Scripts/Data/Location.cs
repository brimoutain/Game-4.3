using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Location
{
    public string locationName;           // 地点名称
    public List<string> releasableAnimals; // 可放归动物名称列表

    // 构造函数
    public Location(string name, List<string> animals)
    {
        locationName = name;
        releasableAnimals = animals;
    }
}