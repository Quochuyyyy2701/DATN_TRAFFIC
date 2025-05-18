using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button buttonSpawn;
    void Start()
    {
        buttonSpawn.onClick.AddListener(() => { ButtonSpawn(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ButtonSpawn()
    {
        CarSpawner.Instance.SpawnRandomPos();
    }    
}
