using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode; // NetworkBehaviour'u ekledik. 'network tabanlı olması için şarttır!'
using UnityEngine;

public class Player : NetworkBehaviour , IKitchenObjectParent
{
    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyPickedSomething;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }
    
    public static Player LocalInstance { get; private set; }
    
    
    
    public event EventHandler OnPickedSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChangedEvent;
    public class OnSelectedCounterChangedEventArgs: EventArgs
    {
        public BaseCounter selectedCounter;
    }
    
    [SerializeField] private float movementSpeed = 7f;
    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private LayerMask collisionsLayerMask;
    [SerializeField] private List<Vector3> spawnPositionList;

    private Vector3 lastInteractDirection;
    private bool isWalking;

    private BaseCounter selectedCounter;
    
    [SerializeField] private Transform kitchenObjectHoldPoint;
    private KitchenObject kitchenObject;
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        transform.position = spawnPositionList[(int)OwnerClientId];
        OnAnyPlayerSpawned?.Invoke(this,EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;    
        }
        
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        // Bir istemcinin bağlantısı kesildiğinde çağrılacak geri arama. Bu geri arama yalnızca sunucuda ve bağlantıyı kesen yerel istemcide çalıştırılır.

        if (clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInputOnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInputOnInteractAlternateAction;
    }

    private void GameInputOnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }

        
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        } 
    }

    private void GameInputOnInteractAction(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }
    
    private void Update()
    {
        if (!IsOwner)
        {
            return; //Bir Player'i kontrol etmeyelim sadece o client'in kurucusu olan player kontrol edilebilsin !
        }
        HandleMovement();
        HandleInteractions();
    }
    
    public bool IsWalking()
    {
        return isWalking;
    }
    
    

    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDirection != Vector3.zero)
        {
            lastInteractDirection = moveDirection;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDirection, out RaycastHit raycastHit, interactDistance,countersLayerMask)) //raycasthit fonksiyondan çıkan deger !
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter)) //Normal getComponent ile farkı kendi içinde null check yapması !
            {
                // ClearCounter ile etkileşim halinde.
                if (baseCounter != this.selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else {
               SetSelectedCounter(null);
            }
        }
        else {
            SetSelectedCounter(null);
        }
        
    }
    
    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        
        float moveDistance = movementSpeed * Time.deltaTime; 
        float playerHeight = 2f;
        float playerRadius = .7f;
        
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one*playerRadius,moveDirection, Quaternion.identity,moveDistance,collisionsLayerMask);

        if (!canMove)
        {
            // istedigimiz yonde hareket edemiyorsak.
            Vector3 moveDirectionX = new Vector3(moveDirection.x, 0, 0).normalized;
            canMove = moveDirection.x !=0 && !Physics.BoxCast(transform.position,Vector3.one*playerRadius , // Karakterin çarptığı bir şey var mı ?
                 moveDirectionX,Quaternion.identity, moveDistance,collisionsLayerMask);
            if (canMove)
            {
                // Sadece x koordinatı üzerinde hareket edebilir.
                moveDirection = moveDirectionX;
            }
            else
            {
                // x yönünde hareket etmiyorsak.
                // O zaman z yönüne bakarız => çünkü iki yönlü hareket var.
                Vector3 moveDirectionZ = new Vector3(0, 0, moveDirection.z).normalized;
                canMove =moveDirection.z !=0 && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, // Karakterin çarptığı bir şey var mı ?
                    moveDirectionZ,Quaternion.identity, moveDistance,collisionsLayerMask);
                if (canMove)
                { // sadece z yönünde hareket edebiliyoruz!
                    moveDirection = moveDirectionZ;
                }
                else
                {
                    // Hiç bir yöne hareket edemiyoruz !
                }
            }
        }
        if (canMove) // Hareket edebiliyorsa bu state'e gir.
        {
            transform.position += moveDirection *moveDistance; //Aldigimiz inputlara göre hareketi saglar.
        }
        
        isWalking = moveDirection != Vector3.zero;
        /* Yukarıda ki kod aşağıdaki işi görür.
        if (moveDirection !=Vector3.zero)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
        */
        
        //Doğrusal interpolasyon ama küresel şekilde !
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
    } 


    private void SetSelectedCounter(BaseCounter selectedCounter)  //Kodu kompakt hale getirmek için eventi ayarlamak için eventi bir fonksiyona yedirdik.
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChangedEvent?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter // Bu ikisi aynı şey değil biri eventargs dan geliyor biri ise sınıfta tanımlanan selectedCounter.
        });
    }

    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }
    
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this,EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this,EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
