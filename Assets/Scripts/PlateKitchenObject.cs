using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{

    public event EventHandler<OnGredientAddedEventArgs> OnIngredientAdded;
    
    public class  OnGredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSo;
    }
    
    
    private List<KitchenObjectSO> _kitchenObjectSoList;

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;
    protected override void Awake()
    {
        base.Awake();
        _kitchenObjectSoList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSo) //Aynı turden malzeme ekletemez.Malzeme ekleme metotu!
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSo))
        {
            //Not a valid ingredient
            return false;
        }
        if (_kitchenObjectSoList.Contains(kitchenObjectSo))
        {
            // Already has this type
            return false;
        }
        else
        {
           AddIngredientServerRpc(
               KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSo)
               );
            
            return true;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddIngredientServerRpc(int kitchenObjectSOIndex)
    {
        AddIngredientClientRpc(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void AddIngredientClientRpc(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSo  =  KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        _kitchenObjectSoList.Add(kitchenObjectSo);  // EKLER  
        OnIngredientAdded?.Invoke(this , new OnGredientAddedEventArgs
        {
            kitchenObjectSo = kitchenObjectSo
        });
    }

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return _kitchenObjectSoList;
    }
}
