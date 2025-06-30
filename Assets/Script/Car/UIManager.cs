using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class Const
{
    public static bool isUsingModel = true;
    public static bool isSpawnerCar = false;
}



public class UIManager : MonoBehaviour
{
    public Button buttonSpawn;
    public Image BG;
    public TextMeshProUGUI textTF1;
    public TextMeshProUGUI textTF2;
    public TextMeshProUGUI textTF3;

    public TrafficLightController lightController1;
    public TrafficLightController lightController2;
    public TrafficLightController lightController3;
    void Start()
    {
        buttonSpawn.onClick.AddListener(() => { ButtonSpawn(); });
        buttonSpawn.GetComponentInChildren<TextMeshProUGUI>().text = "SpawnerCar: " + (Const.isSpawnerCar ? "On" : "Off");
        InvokeRepeating("GetInfoTrafficController", 1f, 1f);       // Cập nhật UI
        InvokeRepeating("ResetMinuteStats", 60f, 60f);
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
 

    public void GetInfoTrafficController()
    {
        string info1 = GetIntersectionInfo(lightController1, "Giao lộ 1");
        string info2 = GetIntersectionInfo(lightController2, "Giao lộ 2");
        string info3 = GetIntersectionInfo(lightController3, "Giao lộ 3");

        textTF1.text = info1;
        textTF2.text = info2;
        textTF3.text = info3;
    }
    private string GetIntersectionInfo(TrafficLightController controller, string name)
    {
        int passedLastMinute = controller.GetMinutePassedCarCount();
       
        float greenTime = controller.GetGreenTime();
        float greenLeft = greenTime;
        float leftDT = controller.greenCountdown_DT;
        float leftNB = controller.greenCountdown_NB;


        return $"{name} ({(Const.isUsingModel ? "AI điều khiển" : "Thủ công")})\n" +
               $"=== 1 phút gần nhất ===\n" +
               $"Xe qua: {passedLastMinute}\n" +
                $"Đèn xanh: {greenTime}\n"+
                 $"Xe qua: {greenLeft}\n";
    }



}
