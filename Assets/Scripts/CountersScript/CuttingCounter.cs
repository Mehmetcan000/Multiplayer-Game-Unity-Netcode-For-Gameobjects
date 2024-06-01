using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class CuttingCounter : BaseCounter,IHasProgress
{

  public static event EventHandler OnAnyCut;

  new public static void ResetStaticData()
  {
    OnAnyCut = null;
  }

  public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

  public event EventHandler OnCut;

  [SerializeField] private CuttingRecipeSO[] cutKitchenObjectSOArray;

  private int cuttingProgress;
  
  public override void Interact(Player player)
  {
    if (!HasKitchenObject())
    {
      //There is no KitchenObject here
      if (player.HasKitchenObject())
      { 
        // Player carrying something
        if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
        {
          //Player carrying something that can be CUT !
          KitchenObject kitchenObject = player.GetKitchenObject();
          kitchenObject.SetKitchenObjectParent(this);
         
          InteractLogicPlaceObjectOnCounterServerRpc();
        }
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
      }
      else
      {
        // Player not carrying anything
        GetKitchenObject().SetKitchenObjectParent(player); //ClearCounter üstündeki objeyi player eline aldı.
      }
    }
  }

  [ServerRpc(RequireOwnership = false)]
  private void InteractLogicPlaceObjectOnCounterServerRpc()
  {
    InteractLogicPlaceObjectOnCounterClientRpc();
  }
 
  [ClientRpc]
  private void InteractLogicPlaceObjectOnCounterClientRpc()
  {
    cuttingProgress = 0;
    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
    {
      progressNormalized =0f
    });
  }

  public override void InteractAlternate(Player player)
  {
    if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
    {
      // There is a Kitchen Object here! AND it can be cut !

      CutObjectServerRpc();
      TestCuttingProgressDoneServerRpc();
    }
  }

  [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRpc()
    {
      CutObjectClientRpc();
    }

    [ClientRpc]
    private void CutObjectClientRpc()
    {
      cuttingProgress++;
      
      OnCut?.Invoke(this,EventArgs.Empty);
      OnAnyCut?.Invoke(this,EventArgs.Empty);
      
      CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
      
      OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
      {
        progressNormalized = (float)cuttingProgress / cuttingRecipeSo.cuttingProgressMax
      });    
      
      
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRpc()
    {
      CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
      if (cuttingProgress>=cuttingRecipeSo.cuttingProgressMax)
      {
        KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
              
        KitchenObject.DestroyKitchenObject(GetKitchenObject());
        KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
      }
    }
    
    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
   {
    CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
    return cuttingRecipeSo != null;
   }
  
  private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
  {
    CuttingRecipeSO cuttingRecipeSo = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
    if (cuttingRecipeSo != null)
    {
      return cuttingRecipeSo.output;
    }
    else
    {
      return null;
    }
  }

  private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
  {
    foreach (CuttingRecipeSO cuttingRecipeSo in cutKitchenObjectSOArray)
    {
      if (cuttingRecipeSo.input == inputKitchenObjectSO)
      {
        return cuttingRecipeSo;
      }
    }
    return null;
  }
}