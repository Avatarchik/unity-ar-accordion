﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using Newtonsoft.Json;

public class infoPopup : MonoBehaviour
{

 private string jsonString;

 public Dictionary<string, Dictionary <string, string>> layerConfig;
    

    void Start()
    {
        DeserializeJson();    
    }

    void DeserializeJson() {  
        string jsonPath = Application.streamingAssetsPath + "/content.json";
        jsonString = File.ReadAllText(jsonPath);
        layerConfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary <string, string>>>(jsonString);
        UpdateInformation(0);
    }

    public void UpdateInformation(int layerNumber) {
        Text Information = this.transform.Find("Text").gameObject.GetComponent<Text>();
        Information.text = layerConfig["layer" + layerNumber]["information"];
    }
}
