
#pragma warning disable IDE0130

using Android.App;
using Android.Content.PM;
using Android.OS;

namespace SkyeShowAndroid
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static readonly int RequestVideoPermissionId = 1001;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Tiramisu)
            {
#pragma warning disable CA1416
                if (CheckSelfPermission(Android.Manifest.Permission.ReadMediaVideo)
                    != Android.Content.PM.Permission.Granted)
                {
                    RequestPermissions([Android.Manifest.Permission.ReadMediaVideo],
                        RequestVideoPermissionId);
                }
#pragma warning restore CA1416
            }
            else
            {
                if (CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage)
                    != Android.Content.PM.Permission.Granted)
                {
                    RequestPermissions([Android.Manifest.Permission.ReadExternalStorage],
                        RequestVideoPermissionId);
                }
            }
        }
    }
}
