using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NfcSampleApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NfcTabs : TabbedPage
    {
        public NfcTabs ()
        {
            InitializeComponent();

            this.Children.Add(new NfcReaderPage());

            if(Device.RuntimePlatform == Device.Android)
            {
                this.Children.Add(new NfcWriterPage());
            }
        }
    }
}