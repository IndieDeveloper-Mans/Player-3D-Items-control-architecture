using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PortableItem : TakableItem
{
    [Header("Portable Animation Data")]
    [SerializeField] public bool _canDropItem = true;
    [Space]
    [SerializeField] public bool _knockItemFromHands;
    [Space]
    [SerializeField] protected bool _isDropping;
    [Space]
    [SerializeField] protected string _dropAnimName;
    [Space]
    [SerializeField] protected float _dropAnimDelay;

    protected override void Update()
    {
        base.Update();

        if (itemInPlayerHand && !_isDropping)
        {
            if (_canDropItem)
            {
                if (_player.PlayerIsGrounded())
                {
                    if (!Input.GetMouseButton(0))
                    {
                        StartCoroutine(DropCoroutine());
                    }
                }
            }
        }
    }

    IEnumerator DropCoroutine()
    {
        Debug.Log("Drop Start");

        _isDropping = true;

        _player.DisablePlayerMovement();

        _playerAnimator.SetTrigger(_dropAnimName);

        while (true)
        {
            if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(_dropAnimName) && _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.2f)
            {
                Debug.Log("Drop item");

                // disable Hands IK
                if (_IKEnabled)
                {
                    SetHandsIKWeight(itemType, 0);

                    _IKEnabled = false;
                }

                DropItem();

                break;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(_playerAnimator.GetCurrentAnimatorStateInfo(0).length - 0.5f);

        _isDropping = false;

        itemInPlayerHand = false;

        _player.itemInPlayerHand = itemInPlayerHand;

        if (!_knockItemFromHands)
        {
            _player.EnablePlayerMovement();
        }

        yield return new WaitForEndOfFrame();

        EnableItemColliding();

        SetDefaultLayer();

        Debug.Log("Drop Stop");
    }

    public virtual void DropItem()
    {
        DisableItemRootContainer();

        _itemTargetTransform.SetParent(null, true);

        EnableItemPhysics();

        AkSoundEngine.PostEvent(dropItemSoundName, gameObject);
    }

    public virtual void RemoveItem(Transform parent = null, bool isItemStatic = true)
    {
        if (_IKEnabled)
        {
            SetHandsIKWeight(itemType, 0);

            _IKEnabled = false;
        }

        itemInPlayerHand = false;

        _player.itemInPlayerHand = itemInPlayerHand;

        _itemTargetTransform.SetParent(parent, true);

        DisableItemRootContainer();

        if (!isItemStatic)
        {
            EnableItemColliding();

            EnableItemPhysics();
        }

        Debug.Log("Remove Item");
    }
}