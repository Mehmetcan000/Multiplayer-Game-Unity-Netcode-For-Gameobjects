using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{

   private const string IS_WALKING = "IsWalking";
   
   private Animator _animator;

   [SerializeField] private Player player;

   private void Awake()
   {
       _animator = GetComponent<Animator>();
   }
   
   private void Update()
   {
       
       if (!IsOwner)
       {
           return;
       }
       _animator.SetBool(IS_WALKING,player.IsWalking());
   }
}
