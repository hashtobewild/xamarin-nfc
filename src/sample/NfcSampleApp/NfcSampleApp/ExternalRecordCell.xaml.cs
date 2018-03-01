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
	public partial class ExternalRecordCell : ViewCell
	{
		public ExternalRecordCell ()
		{
			InitializeComponent ();
		}
	}
}