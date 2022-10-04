using UnityEngine;

public interface IDamageState
{
    DamageState.Result Sampling();
    DamageState.Result Sampling(float gusetLuck);
    void ReceiveDamage(DamageState.Package package);
    void ReceiveDamage(DamageState.DamageType type, int Damage);
}
