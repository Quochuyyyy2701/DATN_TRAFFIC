using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomMonoBehaviour : MonoBehaviour
{
    private void Awake()
    {
        this.LoadComponent();
    }
    private void Reset()
    {
        this.LoadComponent();
    }

    public abstract void LoadComponent();
 

}
