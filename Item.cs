using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    rightHand,
    leftHand,
    twoHand,
    rightFoot,
    leftFoot,
    twoFoot
}

[RequireComponent(typeof(Rigidbody))]
public abstract class Item : MonoBehaviour
{
    [Header("Item Refs")]
    [SerializeField] public Rigidbody _rb;
    [Space]
    [SerializeField] protected Transform _itemTargetTransform;

    [Header("Colliders")]
    [SerializeField] protected Collider _itemTriggerCollider;
    [Space]
    public Collider _itemCollider;

    [Header("Player Colliding")]
    [SerializeField] public int _itemPriority;
    [Space]
    [SerializeField] public bool _itemCanCollide = true;
    [Space]
    [SerializeField] protected bool _playerWithinItemCollider;
    [Space]
    [SerializeField] protected Player _player;
    [Space]
    [SerializeField] protected Animator _playerAnimator;
    [Space]
    [SerializeField] protected AnimatorSwapController _animatorSwapController;
    [Space]
    [SerializeField] protected bool canSwapItemLayers = true;

    protected virtual void Start()
    {
        _rb = GetComponent<Rigidbody>();

        // set item default pivot transform without offsetting it otherwise set offsetted target directly in inspector
        if (_itemTargetTransform == null)
        {
            _itemTargetTransform = transform;
        }

        if (_itemCollider == null)
        {
            _itemCollider = GetComponent<Collider>();
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        // disable item by priority
        if (other.TryGetComponent<Item>(out var collidedItem))
        {
            CheckCollidedItemPriority(collidedItem);
        }

        if (_itemCanCollide)
        {
            if (other.TryGetComponent<Player>(out var playerScript))
            {
                if (!playerScript.itemInPlayerHand)
                {
                    if (_player == null)
                    {
                        _player = playerScript;

                        _playerAnimator = playerScript.GetComponent<Animator>();

                        _animatorSwapController = _player.GetComponent<AnimatorSwapController>();
                    }

                    _playerWithinItemCollider = true;
                }
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        // enable item by priority
        if (other.TryGetComponent<Item>(out var collidedItem))
        {
            collidedItem._itemCanCollide = true;
        }

        if (other.TryGetComponent<Player>(out var playerScript))
        {
            _itemCanCollide = true;

            _playerWithinItemCollider = false;
        }
    }

    protected virtual void EnableItemColliding()
    {
        Physics.IgnoreCollision(_player.GetComponent<Collider>(), _itemCollider, false); // not item trigger

        // item trigger collider
        _itemTriggerCollider.enabled = true;
    }

    protected virtual void DisableItemColliding()
    {
        Physics.IgnoreCollision(_player.GetComponent<Collider>(), _itemCollider);  // not item trigger

        // item trigger collider
        _itemTriggerCollider.enabled = false;
    }

    protected virtual void EnableItemPhysics()
    {
        _rb.isKinematic = false;

        //_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _rb.useGravity = true;

        _rb.velocity = Vector3.zero;
    }

    protected virtual void DisableItemPhysics()
    {
        // disable physics

        _rb.isKinematic = true;

        _rb.useGravity = false;

        _rb.velocity = Vector3.zero;
    }

    protected virtual void SetIgnoreRaycastLayer()
    {
        if (canSwapItemLayers)
        {
            gameObject.layer = 2; //ignore raycast layer
        }
    }

    protected virtual void SetDefaultLayer()
    {
        if (canSwapItemLayers)
        {
            gameObject.layer = 0;
        }
    }

    void CheckCollidedItemPriority(Item item)
    {
        if (_itemPriority > item._itemPriority)
        {
            item._itemCanCollide = false;
        }
        else
        {
            _itemCanCollide = false;
        }
    }
}