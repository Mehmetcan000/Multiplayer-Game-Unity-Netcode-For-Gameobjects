using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{ 
   /* Bu temelde, ClearCounter sınıfı, bir tezgahın üstündeki bir noktayı takip eden
  ve o noktaya yerleştirilen mutfak nesnesi ile etkileşimde bulunan bir scripti temsil eder.
  */
    [SerializeField] private KitchenObjectSO kitchenObjectSo;
    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //There is no KitchenObject here
            if (player.HasKitchenObject())
            { 
                // Player carrying something
                player.GetKitchenObject().SetKitchenObjectParent(this); //Player elindeki objeyi clearCounter'in üzerine koydu.
            }
            else
            {
                // Player not carrying anything
            }
        }
        else
        {
            // There is a KitchenObject here
            if (player.HasKitchenObject())
            {
                // Player carrying something
                if (player.GetKitchenObject().TryGetPlate(out  PlateKitchenObject plateKitchenObject)) //Player'in elinde tuttuğu obje tabak ise bu lojiğe gir!
                {
                    // Player is holding Plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                    }
                }
                else
                {
                    // Player is not carrying Plate but something else
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        // Counter is holding Plate
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
                        }
                        
                    }
                }
            }
            else
            {
                // Player not carrying anything
                GetKitchenObject().SetKitchenObjectParent(player); //ClearCounter üstündeki objeyi player eline aldı.
            }
        }
    }
}
