namespace _Project.Scripts.Combat
{
    public interface IWeaponProvider
    {
        WeaponProfile MeleeProfile { get; }
        WeaponProfile RangedProfile { get; }
        WeaponProfile SpellProfile { get; }

        AttackType CurrentAttackType { get; }
    }
}