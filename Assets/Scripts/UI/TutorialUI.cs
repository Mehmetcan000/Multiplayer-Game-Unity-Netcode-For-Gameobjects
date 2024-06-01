using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
   [SerializeField] private TextMeshProUGUI keyMoveUpText;
   [SerializeField] private TextMeshProUGUI keyMoveDownText;
   [SerializeField] private TextMeshProUGUI keyMoveLeftText;
   [SerializeField] private TextMeshProUGUI keyMoveRightText;
   [SerializeField] private TextMeshProUGUI keyMoveInteractText;
   [SerializeField] private TextMeshProUGUI keyMoveInteractAlternateText;


   private void Start()
   {
      GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;
      KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManager_OnLocalPlayerReadyChanged;
      UpdateVisual();
      
      Show();
   }

   private void KitchenGameManager_OnLocalPlayerReadyChanged(object sender, EventArgs e)
   {
      if (KitchenGameManager.Instance.IsLocalPlayerReady())
      {
         Hide();
      }
   }
   

   private void GameInput_OnBindingRebind(object sender, EventArgs e)
   {
      UpdateVisual();
   }

   private void UpdateVisual()
   {
      keyMoveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
      keyMoveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
      keyMoveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
      keyMoveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
      keyMoveInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
      keyMoveInteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
   }

   private void Show()
   {
      gameObject.SetActive(true);
   }

   private void Hide()
   {
      gameObject.SetActive(false);
   }
}
