using UnityEngine;

namespace _Project.Scripts.Units.Factions {
    public enum UnitFaction
    {
        User,
        Allied,
        Neutral,
        Enemy
    }

    public static class UnitFactionColors {
        public static readonly Color UserSelection = Color.lightGreen;
        public static readonly Color AlliedSelection = Color.lightSkyBlue;
        public static readonly Color NeutralSelection = Color.yellow;
        public static readonly Color EnemySelection = Color.red;

        public static Color GetSelectionColor(UnitFaction faction) {
            return faction switch {
                UnitFaction.User => UserSelection,
                UnitFaction.Allied => AlliedSelection,
                UnitFaction.Neutral => NeutralSelection,
                UnitFaction.Enemy => EnemySelection,
                _ => Color.whiteSmoke
            };
        }
    }
}
