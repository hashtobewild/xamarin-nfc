using Xamarin.Forms;

namespace NfcSampleApp
{
    public abstract class PageBase : ContentPage
    {
        public PageBase(bool shouldAddPackageRecord)
        {
            ShouldAddPackageRecord = shouldAddPackageRecord;
        }

        public bool ShouldAddPackageRecord { get; }

        public string PackageName
        {
            get
            {
                var provider = DependencyService.Get<IApplicationInfoProvider>();
                return provider.PackageName;
            }
        }
    }
}