using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField]
    float baseLengthPerSeconds = 21.0f;

    [SerializeField]
    float baseOutOfLeftLength = 10.0f;

    [SerializeField]
    float baseOutOfRightLength = 5.0f;

    [SerializeField]
    float basePerfectDiffTime = 0.05f;

    [SerializeField]
    float baseGreatDiffTime = 0.13f;

    [SerializeField]
    float baseDamagableRangeTime = 0.01f;

    [SerializeField]
    float baseSimpleNoteInteractibleRangeTime = 0.02f;

    [SerializeField, Range(0.5f, 1.0f)]
    float baseTapNoteCamSizeRatio = 0.86f;

    [SerializeField]
    float baseTapNoteReserveCamSizeRatioDuration = 0.1f;

    [SerializeField]
    float baseTapNoteCamShakeDistance = 0.2f;

    [SerializeField]
    float baseTapNoteCamShakeRestoreDuration = 0.1f;

    public float offset = 0.045f;

    public bool isEditor = true;

    public float lengthPerSeconds => baseLengthPerSeconds;

    public float localLeftVisibleX => -baseOutOfLeftLength;

    public float localRightVisibleX => baseOutOfRightLength + baseLengthPerSeconds;

    public float perfectDiffTime => basePerfectDiffTime;

    public float greatDiffTime => baseGreatDiffTime;

    public float damagableRangeTime => baseDamagableRangeTime;

    public float simpleNoteInteractibleRangeTime => baseSimpleNoteInteractibleRangeTime;

    public float tapNoteCamSizeRatio => baseTapNoteCamSizeRatio;

    public float tapNoteReserveCamSizeRatioDuration => baseTapNoteReserveCamSizeRatioDuration;

    public float tapNoteCamShakeDistance => baseTapNoteCamShakeDistance;

    public float tapNoteCamShakeRestoreDuration => baseTapNoteCamShakeRestoreDuration;
}
