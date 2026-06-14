
namespace SkyeShowAndroid.Services
{
    public static class ServiceHelper
    {
        public static T GetService<T>() =>
            App.Services!.GetService<T>()!;
    }
}
