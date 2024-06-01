using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SelectedCounterVisual : MonoBehaviour
{
    
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;
    private void Start()
    {
        if (Player.LocalInstance !=null)
        {
            Player.LocalInstance.OnSelectedCounterChangedEvent += PlayerOnSelectedCounterChangedEvent;
        }
        else
        {
            Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        }
        
    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        if (Player.LocalInstance !=null)
        {
            Player.LocalInstance.OnSelectedCounterChangedEvent -= PlayerOnSelectedCounterChangedEvent;
            Player.LocalInstance.OnSelectedCounterChangedEvent += PlayerOnSelectedCounterChangedEvent;
        }
    }

    private void PlayerOnSelectedCounterChangedEvent(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (var visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(true);
        }
        
    }

    private void Hide()
    {
        foreach (var visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(false);
        }
    }
}
