 using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
 using UnityEngine.SceneManagement;

 public class KitchenGameMultiplayer : NetworkBehaviour
{
   public static KitchenGameMultiplayer Instance { get; private set; }

   [SerializeField] private KitchenObjectListSO kitchenObjectListSo;

   private const int MAX_PLAYER_AMOUNT = 4;


   public event EventHandler OnTryingToJoinGame;
   public event EventHandler OnFailedToJoinGame;
   
   
   private void Awake()
   {
      Instance = this;
      DontDestroyOnLoad(gameObject);
   }

   public void StartHost()
   {
      NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
      NetworkManager.Singleton.StartHost();
   }

   private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
   {
      if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
      {
         connectionApprovalResponse.Approved = false;
         connectionApprovalResponse.Reason = "Game has already start !";
         return;
      }

      if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
      {
         connectionApprovalResponse.Approved = false;
         connectionApprovalResponse.Reason = "Game has already full !";
         return;
      }
      
      connectionApprovalResponse.Approved = true;
      
      /*  if (KitchenGameManager.Instance.IsWaitingToStart())
      {
       
         connectionApprovalResponse.CreatePlayerObject = true;
      }
      else
      {
         connectionApprovalResponse.Approved = false;
      }*/
   }

   public void StartClient()
   {
      OnTryingToJoinGame?.Invoke(this,EventArgs.Empty);
      NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
      NetworkManager.Singleton.StartClient();
   }

   private void NetworkManager_OnClientDisconnectCallback(ulong obj)
   {
      OnFailedToJoinGame?.Invoke(this,EventArgs.Empty);
   }

   public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSo, IKitchenObjectParent kitchenObjectParent)
   {
     SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSo),kitchenObjectParent.GetNetworkObject());
   }
   
   
   [ServerRpc(RequireOwnership = false)]
   private void SpawnKitchenObjectServerRpc(int kitchenObjectSoIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
   {
      KitchenObjectSO kitchenObjectSo = GetKitchenObjectSOFromIndex(kitchenObjectSoIndex);
      Transform kitchenObjectTransform = Instantiate(kitchenObjectSo.prefab);

      NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
      kitchenObjectNetworkObject.Spawn(true);

      KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();

      kitchenObjectParentNetworkObjectReference.TryGet(
         out NetworkObject kitchenObjectParentNetworkObject);

      IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
      
      kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
   }

   public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSo)
   {
      return kitchenObjectListSo.KitchenObjectSoList.IndexOf(kitchenObjectSo);
   }

   public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
   {
      return kitchenObjectListSo.KitchenObjectSoList[kitchenObjectSOIndex];
   }

   public void DestroyKitchenObject(KitchenObject kitchenObject)
   {
      DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
   }

   [ServerRpc(RequireOwnership = false)]
   private void DestroyKitchenObjectServerRpc(NetworkObjectReference networkObjectReference)
   {
      networkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
      KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
      
      ClearKitchenObjectOnParentClientRpc(networkObjectReference);
      
      kitchenObject.DestroySelf();
      
   }

   [ClientRpc]
   private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference networkObjectReference)
   {
      networkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
      KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
      
      kitchenObject.ClearKitchenObjectParent();
   }

}
