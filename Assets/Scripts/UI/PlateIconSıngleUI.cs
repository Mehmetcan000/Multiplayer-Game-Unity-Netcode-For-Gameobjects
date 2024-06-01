using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateIconSıngleUI : MonoBehaviour
{
   [SerializeField] private Image image;
   
   public void SetKitchenObjectSO(KitchenObjectSO kitchenObjectSo)
   {
      image.sprite = kitchenObjectSo.spriteIcon;
   }
}
