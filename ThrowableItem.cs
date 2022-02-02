using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableItem : PortableItem
{
    [Header("Throwable Animation Data")]
    [SerializeField] protected string _throwAnimName;
    [Space]
    [SerializeField] protected float _throwAnimDelay;

    [Header("Throw Control")]
    public bool canThrow = true;
    [Space]
    [SerializeField] protected Vector2 _throwForce;
    [Space]
    [SerializeField] protected Vector2 _throwTorqueForce;

    protected override void Update()
    {
        base.Update();

        if (itemInPlayerHand && canThrow)
        {
            if (_player.PlayerIsGrounded())
            {
                if (Input.GetMouseButton(0) && Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(ThrowCoroutine());

                    Debug.Log("Throwning item");
                }
            }
        }
    }

    IEnumerator ThrowCoroutine()
    {
        Debug.Log("Throw Start");

        itemInPlayerHand = false;

        _player.itemInPlayerHand = itemInPlayerHand;

        _player.DisablePlayerMovement();

        DisableItemColliding();

        _playerAnimator.SetTrigger(_throwAnimName);

        while (true)
        {
            if (_playerAnimator.GetCurrentAnimatorStateInfo(0).IsName(_throwAnimName) && _playerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= _throwAnimDelay)
            {
                Debug.Log("Throwed");

                // disable Hands IK 
                // make override method instead of copying this shit over and over
                if (_IKEnabled)
                {
                    SetHandsIKWeight(itemType, 0);

                    _IKEnabled = false;
                }

                ThrowItem(_throwForce);

                break;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(_playerAnimator.GetCurrentAnimatorStateInfo(0).length);

        _player.EnablePlayerMovement();

        EnableItemColliding();

        Debug.Log("Throw Stop");
    }

    protected virtual void ThrowItem(Vector2 force)
    {
        _rb.isKinematic = false;

        _rb.velocity = Vector3.zero;

        _rb.useGravity = true;

        DisableItemRootContainer();

        _itemTargetTransform.SetParent(null, true);

        // add force
        _rb.AddForce(_player.transform.forward * force.x + _player.transform.up * force.y, ForceMode.Impulse);

        _rb.AddTorque(_player.transform.forward * _throwTorqueForce.x + _player.transform.up * _throwTorqueForce.y, ForceMode.VelocityChange);
    }
}