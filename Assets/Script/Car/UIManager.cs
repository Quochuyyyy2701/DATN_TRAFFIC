using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class Const
{

    public static bool isSpawnerCar =  false;
}



public class UIManager : MonoBehaviour
{
    public Button buttonSpawn;
 
    void Start()
    {
        buttonSpawn.onClick.AddListener(() => { ButtonSpawn(); });
        buttonSpawn.GetComponentInChildren<TextMeshProUGUI>().text = "SpawnerCar: " + (Const.isSpawnerCar ? "On" : "Off");
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ButtonSpawn()
    {
        Const.isSpawnerCar = !Const.isSpawnerCar;
        buttonSpawn.GetComponentInChildren<TextMeshProUGUI>().text = "SpawnerCar: " + (Const.isSpawnerCar ? "On" : "Off");

    }
}
