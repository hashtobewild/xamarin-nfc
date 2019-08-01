using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NfcSampleApp;
using NfcSampleApp.Droid;

[assembly: Xamarin.Forms.Dependency (typeof (ApplicationPackageNameProvider))]
namespace NfcSampleApp.Droid
{
    public class ApplicationPackageNameProvider : IApplicationInfoProvider
    {
        public string PackageName => Application.Context.PackageName;
    }
}