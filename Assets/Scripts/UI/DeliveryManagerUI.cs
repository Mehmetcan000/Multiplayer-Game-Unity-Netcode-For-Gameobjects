using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{

    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;


    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }
    private void Start()
    {
        DeliveryManager.Instance.OnRecipeSpawned += Instance_OnRecipeSpawned;
        DeliveryManager.Instance.OnRecipeCompleted += Instance_OnRecipeCompleted;
        UpdateVisual();
    }

    private void Instance_OnRecipeCompleted(object sender, EventArgs e)
    {
        UpdateVisual();
        Debug.Log("çalıştı");
    }

    private void Instance_OnRecipeSpawned(object sender, EventArgs e)
    {
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        foreach (Transform child in container)
        {
            if (child == recipeTemplate)
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        foreach (RecipeSO recipeSo in DeliveryManager.Instance.GetWaitingRecipeSOList())
        {
             Transform recipeTransform = Instantiate(recipeTemplate, container);
             recipeTransform.gameObject.SetActive(true);
             recipeTransform.GetComponent<DeliveryManagerSingleUI>().SetRecipeSO(recipeSo);
        }
    }
}
