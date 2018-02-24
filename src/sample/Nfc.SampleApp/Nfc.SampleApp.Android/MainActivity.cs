using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Nfc;

namespace Nfc.SampleApp.Droid
{
    [Activity(Label = "Nfc.SampleApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    [IntentFilter(new []{ NfcAdapter.ActionNdefDiscovered }, Categories = new []{ "android.intent.category.DEFAULT" }, DataMimeType = "*/*", DataScheme = "https", DataHost = "www.talki.com")]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            CrossNfc.SetCurrentActivityResolver(() => this);
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
            CrossNfc.OnNewIntent(Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            CrossNfc.OnNewIntent(intent);
        }
    }
}

