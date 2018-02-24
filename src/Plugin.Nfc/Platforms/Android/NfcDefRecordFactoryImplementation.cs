using System;
using Android.Nfc;

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
            var record = NdefRecord.CreateTextRecord(languageCode,text);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateUriRecord(Uri uri)
        {
            var record = NdefRecord.CreateUri(uri.AbsoluteUri);
            var r = new AndroidNdefRecord(record);
            return r;
        }
    }
}