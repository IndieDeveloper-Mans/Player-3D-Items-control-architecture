using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerItemsIKControl : MonoBehaviour
{
    [Header("Hands IK")]
    public TwoBoneIKConstraint _leftArm;
    [Space]
    public TwoBoneIKConstraint _rightArm;

    [Header("Hands Containers")]
    public GameObject _leftArmItemContainer;
    [Space]
    public GameObject _rightArmItemContainer;
    [Space]
    public GameObject _twoHandsItemContainer;

    [Header("Foot IK")]
    public TwoBoneIKConstraint _leftFoot;
    [Space]
    public TwoBoneIKConstraint _rightFoot;

    [Header("Parent Constraint")]
    public MultiParentConstraint _itemParentConstraint;
    [Space]
    public GameObject itemContainer;
}