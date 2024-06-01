using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSo;


    public event EventHandler OnPlayerGrabbedObject;
    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject()) //Playerin elinde yok ve talep ediyorsa, PLAYER ın elinde var ise yapamaz!
        {
            // Player is not carrying anything
            KitchenObject.SpawnKitchenObject(kitchenObjectSo, player);
            InteractLogicServerRpc();    
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }

    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        OnPlayerGrabbedObject?.Invoke(this,EventArgs.Empty);
    }
}
