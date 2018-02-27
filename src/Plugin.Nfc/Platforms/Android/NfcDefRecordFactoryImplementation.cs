using System;
using Android.Nfc;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc.Tech;
using Android.OS;
using System.Text;
using System.Linq;

namespace Plugin.Nfc
{
    public class NfcDefRecordFactoryImplementation : INfcDefRecordFactory
    {
        public NfcDefRecord CreateApplicationRecord(string packageName)
        {
            var record = NdefRecord.CreateApplicationRecord(packageName);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateMimeRecord(string mimeType, byte[] mimeData)
        {
            var record = NdefRecord.CreateMime(mimeType, mimeData);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateExternalRecord(string domain, string type, byte[] data)
        {
            var record = NdefRecord.CreateExternal(domain, type, data);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateTextRecord(string languageCode, string text)
        {
            if (Build.VERSION.SdkInt > BuildVersionCodes.Kitkat)
            {
                var record = NdefRecord.CreateTextRecord(languageCode,text);
                var r = new AndroidNdefRecord(record);
                return r;
            }
            else
            {

                if(String.IsNullOrWhiteSpace(languageCode)) throw new ArgumentNullException(nameof(languageCode));
                if(String.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));

                var languageBytes = Encoding.ASCII.GetBytes(languageCode);
                var textBytes = Encoding.UTF8.GetBytes(text);
                var recordPayload = new byte[1 + (languageBytes.Length & 0x03F) + textBytes.Length];

                 recordPayload[0] = (byte)(languageBytes.Length & 0x03F);
                 Array.Copy(languageBytes, 0, recordPayload, 1, languageBytes.Length & 0x03F);
                 Array.Copy(textBytes, 0, recordPayload, 1 + (languageBytes.Length & 0x03F), textBytes.Length);

                 var record = new NdefRecord(NdefRecord.TnfWellKnown, NdefRecord.RtdText.ToArray(), null, recordPayload);
                 return new AndroidNdefRecord(record);
            }
        }

        public NfcDefRecord CreateUriRecord(Uri uri)
        {
            var record = NdefRecord.CreateUri(uri.AbsoluteUri);
            var r = new AndroidNdefRecord(record);
            return r;
        }
    }
}