using System.Collections;
using UnityEngine;

public class TakableItem : InteractiveItem
{
    [Header("Takable Animation Data")]
    [SerializeField] protected string _takeAnimName;
    [Space]
    [SerializeField] protected float _takeAnimDelay;

    [Header("To Item Parent Root")]
    [SerializeField] protected Vector3 _itemToParentPositionOffset;
    [Space]
    [SerializeField] protected Quaternion _itemToParentRotationOffset;
    [Space]
    [SerializeField] protected float _itemPositionSpeed;
    [Space]
    [SerializeField] protected float _itemRotationSpeed;

    [Header("Item Take Control")]
    public bool _canTakeItem = true;
    [Space]
    public bool itemInPlayerHand;
    [Space]
    [SerializeField] protected bool _itemIsTaking;

    [Header("Item Sounds")]
    public string takeItemSoundName;
    [Space]
    public string dropItemSoundName;

    protected virtual void Update()
    {
        if (_playerWithinItemCollider && !itemInPlayerHand && !_itemIsTaking)
        {
            if (_canTakeItem && _player.PlayerIsGrounded())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(TakeCoroutine());
                }
            }
        }

        //FaceDirection facingDirection = _player.CheckFacing(_player.transform, transform);
    }

    IEnumerator TakeCoroutine()
    {      
        _itemIsTaking = true;

        if (itemHintObj != null)
        {
            itemHintObj.SetActive(false);
        }

        DisableItemPhysics();

        DisableItemColliding();

        _playerWithinItemCollider = false;

        SetItemRootContainer();

        Debug.Log("Take Start");

        yield return CheckIfPlayerTowardItem();  // wait till moving and rotating towards

        Debug.Log("CheckIfPlayerTowardItem Ends");

        _playerAnimator.SetTrigger(_takeAnimName);

        while (true)
        {
            //set Hands IK  like update
            if (_IKEnabled)
            {
                SetAnimationHandsIK(itemType);
            }

            if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(_takeAnimName) && _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= _takeAnimDelay)
            {
                Debug.Log("Pickup item");

                // set ik values to full 1 weight
                if (_IKEnabled)
                {
                    SetHandsIKWeight(itemType, _IKEnableWeight);
                }

                TakeItem();

                StartCoroutine(RotateAndMoveItemInHand());

                break;
            }

            yield return new WaitForEndOfFrame();
        }

        //yield return StartCoroutine(RotateAndMoveItemInHand());

        // кладет предмет, когда полностью анимация взятия проиграется - заккомитить, если не нужна задержка
        yield return new WaitForSeconds(_playerAnimator.GetCurrentAnimatorStateInfo(0).length - 0.9f);

        itemInPlayerHand = true;

        _player.itemInPlayerHand = itemInPlayerHand;

        _itemIsTaking = false;

        _player.EnablePlayerMovement();

        Debug.Log("Take Stop");
    }

    protected virtual void TakeItem()
    {     
        ParentItemToPlayerHand();

        AkSoundEngine.PostEvent(takeItemSoundName, gameObject);
    }

    protected virtual void ParentItemToPlayerHand()
    {
        //_playerItemsIK.itemContainer.transform.ResetTransformation();

        _itemTargetTransform.SetParent(_playerItemsIK.itemContainer.transform, true);
    }

    IEnumerator RotateAndMoveItemInHand()
    {
        bool targetPosReached = false;

        bool targetRotReached = false;

        while (!targetPosReached || !targetRotReached)
        {
            if (!targetPosReached)
            {
                if (Vector3.Distance(_itemTargetTransform.localPosition, _itemToParentPositionOffset) < 0.001f)
                {
                    Debug.Log("Position Reached");

                    targetPosReached = true;
                }
                else
                {
                    _itemTargetTransform.localPosition = Vector3.MoveTowards(_itemTargetTransform.localPosition, _itemToParentPositionOffset, _itemPositionSpeed * Time.deltaTime);
                }
            }

            if (!targetRotReached)
            {
                //if (_itemToParentRotationOffset != Quaternion.identity) // check if offset rotation != null
                //{
                if (Quaternion.Angle(_itemTargetTransform.localRotation, _itemToParentRotationOffset) <= 0.01f)
                {
                    Debug.Log("Rotation Reached");

                    targetRotReached = true;
                }
                else
                {
                    _itemTargetTransform.localRotation = Quaternion.RotateTowards(_itemTargetTransform.localRotation, _itemToParentRotationOffset, _itemRotationSpeed * Time.deltaTime);
                }
                //}
            }

            yield return new WaitForEndOfFrame();
        }

        Debug.Log("Coroutine Ends");
    }

    void SetItemRootContainer()
    {
        var sources = _playerItemsIK._itemParentConstraint.data.sourceObjects;

        switch (itemType)
        {
            case ItemType.rightHand:
                sources.SetWeight(0, 1.0f);
                break;
            case ItemType.leftHand:
                sources.SetWeight(1, 1.0f);
                break;
            case ItemType.twoHand:
                sources.SetWeight(2, 1.0f);
                break;
            case ItemType.rightFoot:
                sources.SetWeight(3, 1.0f);
                break;
            case ItemType.leftFoot:
                sources.SetWeight(4, 1.0f);
                break;
        }

        _playerItemsIK._itemParentConstraint.data.sourceObjects = sources;
    }

    protected virtual void DisableItemRootContainer()
    {
        var sources = _playerItemsIK._itemParentConstraint.data.sourceObjects;

        for (int i = 0; i < sources.Count; i++)
        {
            sources.SetWeight(i, 0);
        }

        _playerItemsIK._itemParentConstraint.data.sourceObjects = sources;
    }

    protected virtual void SetAnimationHandsIK(ItemType itemType)
    {
        if (itemType == ItemType.rightHand)
        {
            _playerItemsIK._rightArm.weight = _playerAnimator.GetFloat("RightHand");
        }
        else if (itemType == ItemType.leftHand)
        {
            _playerItemsIK._leftArm.weight = _playerAnimator.GetFloat("LeftHand");
        }
        else if (itemType == ItemType.twoHand)
        {
            _playerItemsIK._rightArm.weight = _playerAnimator.GetFloat("RightHand");

            _playerItemsIK._leftArm.weight = _playerAnimator.GetFloat("LeftHand");
        }
    }

    protected virtual void SetHandsIKWeight(ItemType itemType, float weightValue)
    {
        if (itemType == ItemType.rightHand)
        {
            _playerItemsIK._rightArm.weight = weightValue;
        }
        else if (itemType == ItemType.leftHand)
        {
            _playerItemsIK._leftArm.weight = weightValue;
        }
        else if (itemType == ItemType.twoHand)
        {
            _playerItemsIK._rightArm.weight = weightValue;

            _playerItemsIK._leftArm.weight = weightValue;
        }
    }
}