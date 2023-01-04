using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotater : MonoBehaviour
{
    public float rotatePerSec = 90;
    public Vector2 randomStartAngle = new Vector2(0, 360);

    private void Awake()
    {
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.z += UnityEngine.Random.Range(randomStartAngle.x, randomStartAngle.y);
        transform.localEulerAngles = localEulerAngles;
    }

    private void Update()
    {
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.z += rotatePerSec * Time.deltaTime;
        transform.localEulerAngles = localEulerAngles;
    }
}
