using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Utils;

namespace _Project.Scripts.Controllers.ContextActions {
    /// <summary>
    /// Picks the context action whose strategy fits the current right-click context.
    /// Actions are considered in priority order; the fallback action handles everything
    /// the specialized strategies declined.
    /// </summary>
    public sealed class ContextActionResolver {
        private readonly List<IContextAction> sortedActions = new List<IContextAction>();
        private readonly IContextAction fallbackAction;

        public ContextActionResolver(IEnumerable<IContextAction> actions) {
            sortedActions.AddRange(actions.Where(action => action != null).OrderBy(action => action.Priority));

            fallbackAction = sortedActions.FirstOrDefault(action =>
                action.Priority == (int)ContextActionPriority.Fallback);

            if (fallbackAction == null) {
                LogUtil.Warn(nameof(ContextActionResolver), nameof(ContextActionResolver),
                    "No fallback context action registered, unhandled right clicks will be ignored");
            }
        }

        public void ResolveAndExecute(RightClickContext context, RightClickContextServices services) {
            foreach (var action in sortedActions) {
                if (!action.CanHandle(context)) continue;

                action.Execute(context, services);
                return;
            }

            fallbackAction?.Execute(context, services);
        }
    }
}
