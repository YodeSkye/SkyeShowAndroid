
#pragma warning disable CA1422

using Android.Views;
using Microsoft.Maui.Platform;

#pragma warning disable IDE0130
namespace SkyeShowAndroid;
#pragma warning restore IDE0130

public static class FullscreenHelper
{
    public static void EnterImmersiveMode()
    {
        var activity = Platform.CurrentActivity;
        if (activity == null)
            return;

        var window = activity.Window;
        if (window == null)
            return;

        var decorView = window.DecorView;
        if (decorView == null)
            return;

        decorView.SystemUiFlags =
            SystemUiFlags.ImmersiveSticky |
            SystemUiFlags.HideNavigation |
            SystemUiFlags.Fullscreen |
            SystemUiFlags.LayoutHideNavigation |
            SystemUiFlags.LayoutFullscreen |
            SystemUiFlags.LayoutStable;
    }

    public static void ExitImmersiveMode()
    {
        var activity = Platform.CurrentActivity;
        if (activity == null)
            return;

        var window = activity.Window;
        if (window == null)
            return;

        var decorView = window.DecorView;
        if (decorView == null)
            return;

        decorView.SystemUiFlags = SystemUiFlags.LayoutStable;
    }
}
