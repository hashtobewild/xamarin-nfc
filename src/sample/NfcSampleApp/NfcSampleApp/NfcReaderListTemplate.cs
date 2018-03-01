using System;
using System.Collections.Generic;
using System.Text;
using Plugin.Nfc;
using Xamarin.Forms;

namespace NfcSampleApp
{
    public class NfcReaderListTemplate : DataTemplateSelector
    {
        public DataTemplate UrlRecordTemplate { get; set; }
        public DataTemplate TextRecordTemplate { get; set; }
        public DataTemplate ExternalRecordTemplate { get; set; }
        public DataTemplate ApplicationRecordTemplate { get; set; }
        public DataTemplate MimeRecordTemplate { get; set; }
        public DataTemplate UnknownRecordTemplate { get; set; }
        
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is UriRecord) return UrlRecordTemplate;
            if (item is TextRecord) return TextRecordTemplate;
            if (item is ExternalRecord) return ExternalRecordTemplate;
            if (item is ApplicationRecord) return ApplicationRecordTemplate;
            if (item is MimeRecord) return MimeRecordTemplate;

            return UnknownRecordTemplate;
        }
    }
}
