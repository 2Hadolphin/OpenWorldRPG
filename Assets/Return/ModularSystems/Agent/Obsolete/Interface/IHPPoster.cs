using System;
using UnityEngine;
public interface IHPPoster
{
    event Action<float> OnHealthPctChange;
    void Takedamage(int dmg);
}
