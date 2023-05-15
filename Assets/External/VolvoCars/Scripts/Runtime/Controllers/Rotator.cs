using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour, MovablePart
{
    [HideInInspector]
    public float angle = 0;

    public float min;
    public float max;
    public Vector3 axis;

    public int priority;

    public HingeJoint hinge;

    [SerializeField][HideInInspector]
    private Vector3 originRot = Vector3.zero;

    private float Range
    {
        get
        {
            if (range == -1) range = Mathf.Abs(max - min);
            return range;
        }
    }
    private float range = -1;

    private Quaternion targetRotation;
    private float duration;

    public Vector3 GetOriginalRotation()
    {
        return originRot;
    }

    /// <summary>
    /// Set the original rotation value of rotator to calculate target rotation.
    /// </summary>
    /// <param name="rot"></param>
    public void SetOriginRotation(Vector3 rot)
    {
        originRot = rot;
    }

    /// <summary>
    /// Convert target moved angle to target rotation.
    /// </summary>
    /// <returns></returns>
    private Quaternion AngleToRotation()
    {
        return Quaternion.Euler(originRot) * Quaternion.AngleAxis(this.angle, axis);
    }

    /// <summary>
    /// Reset local rotation to original value.
    /// </summary>
    public void Reset()
    {
        Move(0f);
    }

    public string Name()
    {
        return name;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int Priority()
    {
        return priority;
    }

    public void Move(float amount)
    {
        SetAngle(amount);
    }

    public void MoveNormalized(float amount)
    {
        SetNormalizedAngle(amount);
    }

    public float MovedAmount()
    {
        return this.angle;
    }

    /// <summary>
    /// Rotate geometries between min and max rotations
    /// </summary>
    /// <param name="value"></param>
    public void SetNormalizedAngle(float value)
    {
        this.angle = Mathf.Lerp(min, max, value);
        transform.localRotation = AngleToRotation();
    }

    /// <summary>
    /// // Tries to rotate geometries to an actual angle
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public float SetAngle(float angle)
    {
        this.angle = Mathf.Clamp(angle, min, max);
        transform.localRotation = AngleToRotation();
        return this.angle;
    }

    /// <summary>
    /// Tries to rotate geometries to an actual angle, animated
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    public float SetAngleAnimated(float angle, float duration)
    {
        this.angle = Mathf.Clamp(angle, min, max);
        targetRotation = AngleToRotation();
        this.duration = duration;
        StartCoroutine(Animate());
        return this.angle;
    }

    public float Min()
    {
        return min;
    }
    public float Max()
    {
        return max;
    }

    IEnumerator Animate()
    {
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = targetRotation;
        float t = 0.0f;
        while (t < duration)
        {
            if (targetRotation != endRotation) break;
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Lerp(startRotation, targetRotation, t / duration);
            yield return null;
        }
    }

}
