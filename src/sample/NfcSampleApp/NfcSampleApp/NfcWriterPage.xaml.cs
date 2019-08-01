﻿using Plugin.Nfc;
using Plugin.Toasts;
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
    public partial class NfcWriterPage : ContentPage
    {
        private INfcTag _tag;
        public NfcWriterPage()
        {
            InitializeComponent();

            this.Title = "Write";

            this.MimeButton.Command = new Command(async () =>
            {
                await Navigation.PushModalAsync(new NavigationPage(new MimeRecordEntryPage(_tag, this.IsApplicationRecordEnabled.IsToggled)) { Title = "Mime" });
            });

            this.TextButton.Command = new Command(async () =>
            {
                await Navigation.PushModalAsync(new NavigationPage(new TextRecordEntryPage(_tag, this.IsApplicationRecordEnabled.IsToggled)) {
                    Title = "Text Record"
                });

            });

            this.UriButton.Command = new Command(async () =>
           {
               await Navigation.PushModalAsync(new NavigationPage(new UriRecordEntryPage(_tag, this.IsApplicationRecordEnabled.IsToggled)) { Title = "Uri" });
           });

            WriteAs.Toggled += WriteAs_Toggled;


        }

        private void WriteAs_Toggled(object sender, ToggledEventArgs e)
        {
            if(e.Value)
            {
                CrossNfc.Current.SetSupportedTechnologies(new[] { NfcTechnologyType.MifareUltraLight, NfcTechnologyType.Ndef,});
            }
            else
            {
                CrossNfc.Current.SetSupportedTechnologies(new[] { NfcTechnologyType.Ndef, NfcTechnologyType.MifareUltraLight });

            }
        }

        private async void Current_TagError(object sender, TagErrorEventArgs args)
        {
            var notificator = DependencyService.Get<IToastNotificator>();

            var options = new NotificationOptions()
                        {
                            Title = "Error",
                            Description = args.Exception.ToString()
                        };

            var result = await notificator.Notify(options);
             Device.BeginInvokeOnMainThread(() =>
            {
                Indicator.BackgroundColor = Color.Red;
            });
        }

        private void Current_TagDetected(object sender, TagDetectedEventArgs args)
        {
            _tag = args.Tag;
            Device.BeginInvokeOnMainThread(async () =>
            {
                Indicator.BackgroundColor = Color.Green;
            });
        }

        protected override void OnAppearing()
        {
            CrossNfc.Current.TagDetected += Current_TagDetected;
            CrossNfc.Current.TagError += Current_TagError;
            if(CrossNfc.Current.IsAvailable())
            {
               CrossNfc.Current.StartListening();
            }

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossNfc.Current.TagDetected -= Current_TagDetected;
            CrossNfc.Current.TagError -= Current_TagError;
            CrossNfc.Current.StopListening();

        }
	}
}