namespace _Project.Scripts.Units.Factions
{
    public class FactionRelations
    {
        public static bool AreHostile(UnitFaction a, UnitFaction b)
        {
            if (a == b)
            {
                return false;
            }

            bool aIsAggressor = a == UnitFaction.Enemy
                                || a == UnitFaction.User
                                || a == UnitFaction.Allied;
            bool bIsAggressor = b == UnitFaction.Enemy
                                || b == UnitFaction.User
                                || b == UnitFaction.Allied;

            if (!aIsAggressor || !bIsAggressor) return false;

            return a == UnitFaction.Enemy || b == UnitFaction.Enemy;
        }
    }
}