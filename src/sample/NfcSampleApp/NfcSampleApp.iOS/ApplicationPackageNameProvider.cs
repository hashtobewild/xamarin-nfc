using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using NfcSampleApp.iOS;
using UIKit;

[assembly: Xamarin.Forms.Dependency (typeof (ApplicationPackageNameProvider))]
namespace NfcSampleApp.iOS
{
    public class ApplicationPackageNameProvider : IApplicationInfoProvider
    {
        public string PackageName => NSBundle.MainBundle.BundleIdentifier;
    }
}