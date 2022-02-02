using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractiveItem : Item
{
    [Header("Item Type")]
    [SerializeField] public ItemType itemType;

    [Header("Moving And Rotating Towards Item")]
    MoveAndRotateTowards _moveAndRotateTowards = new MoveAndRotateTowards();
    [Space]
    [SerializeField] float _itemPosOffset;
    [Space]
    [SerializeField] float _moveTowardsSpeed;
    [Space]
    [SerializeField] float _rotateTowardsSpeed;

    [Header("IK Control")]
    [SerializeField] public PlayerItemsIKControl _playerItemsIK;
    [Space]
    [SerializeField] public bool _useIK;
    [Space]
    [SerializeField] public bool _IKEnabled;
    [Space]
    [Range(0f, 1f)] [SerializeField] protected float _IKEnableWeight = 1f;

    [Header("Item Hint")]
    [SerializeField] public GameObject itemHintObj;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            base.OnTriggerEnter(other);

            if (_playerItemsIK == null)
            {
                _playerItemsIK = player.GetComponent<PlayerItemsIKControl>();
            }
        }

        // disable other item near this one
        if (other.TryGetComponent<Item>(out var item))
        {
            base.OnTriggerEnter(other);
        }
    }

    protected virtual IEnumerator CheckIfPlayerTowardItem()
    {
        SetIgnoreRaycastLayer();

        DisableItemPhysics();

        DisableItemColliding();

        _player.DisablePlayerMovement();

        bool isPlayerTowardsItem = false;

        bool isPlayerFacingItem = false;

        while (!isPlayerTowardsItem || !isPlayerFacingItem)
        {
            Debug.Log("Moving And Rotating Towards");

            if (!isPlayerTowardsItem)
            {
                isPlayerTowardsItem = _moveAndRotateTowards.MoveTowards(_player.transform, _itemTargetTransform, _moveTowardsSpeed, _itemPosOffset); // returns true if player near item
            }

            if (!isPlayerFacingItem)
            {
                isPlayerFacingItem = _moveAndRotateTowards.RotateTowards(_player.transform, _itemTargetTransform, _rotateTowardsSpeed); // returns true if player facing item
            }

            yield return null;
        }

        Debug.Log("Moving And Rotating Towards Ends");

        if (_useIK)
        {
            _IKEnabled = true;
        }
    }
}