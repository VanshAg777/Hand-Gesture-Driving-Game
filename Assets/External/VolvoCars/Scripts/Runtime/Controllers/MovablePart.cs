using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface MovablePart
{
    string Name();

    void Move(float amount);
    void MoveNormalized(float amount);

    void Reset();

    float MovedAmount();

    int Priority();

    float Min();
    float Max();

    GameObject GetGameObject();
}
