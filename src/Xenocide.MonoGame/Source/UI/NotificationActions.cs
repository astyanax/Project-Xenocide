using ProjectXenocide.Assets;
using ProjectXenocide.UI.Screens;

namespace ProjectXenocide.UI
{
    /// <summary>
    /// Executes the context actions offered by notification entries (the
    /// "Go To ..." buttons in the situation log / pending-actions inbox).
    /// </summary>
    public static class NotificationActions
    {
        /// <summary>Button caption for an action, or null if the action is unknown.</summary>
        public static string LabelFor(string actionId)
        {
            return actionId switch
            {
                NotificationMapping.ActionGoToAircraft => "Go To Aircraft",
                NotificationMapping.ActionGoToResearch => "Go To Research",
                NotificationMapping.ActionGoToManufacture => "Go To Manufacture",
                NotificationMapping.ActionGoToBase => "Go To Base",
                NotificationMapping.ActionGoToGlobe => "Target On Globe",
                _ => null,
            };
        }

        /// <summary>True if the action is recognised and can be executed.</summary>
        public static bool CanInvoke(string actionId)
        {
            return LabelFor(actionId) != null;
        }

        /// <summary>
        /// Navigate to the object the notification refers to.  The dialog that
        /// invoked this should already be closing.
        /// </summary>
        public static void Invoke(string actionId, string targetId)
        {
            var screenManager = Xenocide.ScreenManager;
            if (screenManager == null)
                return;

            int outpostCount = Xenocide.GameState?.GeoData?.Outposts?.Count ?? 0;

            switch (actionId)
            {
                case NotificationMapping.ActionGoToResearch:
                    screenManager.ScheduleScreen(new ResearchScreen());
                    break;

                case NotificationMapping.ActionGoToManufacture:
                    if (outpostCount > 0)
                        screenManager.ScheduleScreen(new ManufactureScreen(0));
                    break;

                case NotificationMapping.ActionGoToBase:
                    if (outpostCount > 0)
                        screenManager.ScheduleScreen(new BasesScreen(0));
                    break;

                case NotificationMapping.ActionGoToAircraft:
                case NotificationMapping.ActionGoToGlobe:
                    // TODO: centre the globe on the target craft/position.
                    screenManager.ScheduleScreen(new GeoscapeScreen());
                    break;
            }
        }
    }
}
