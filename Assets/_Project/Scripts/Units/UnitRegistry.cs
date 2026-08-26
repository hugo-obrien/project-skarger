using System.Collections.Generic;
using _Project.Scripts.Units;

namespace _Project.Scripts.UI {
    public class UnitRegistry {
        private static readonly HashSet<Unit> units = new HashSet<Unit>();

        public static IReadOnlyCollection<Unit> Units => units;

        public static void Register(Unit unit) {
            if (unit != null) {
                units.Add(unit);
            }
        }

        public static void Unregister(Unit unit) {
            if (unit != null) {
                units.Remove(unit);
            }
        }
    }
}