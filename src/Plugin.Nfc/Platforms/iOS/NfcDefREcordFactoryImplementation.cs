using System;

namespace Plugin.Nfc
{
    public class NfcDefRecordFactoryImplementation : INfcDefRecordFactory
    {
        public NfcDefRecord CreateApplicationRecord(string packageName)
        {
           return NfcDefRecord.Empty;
        }

        public NfcDefRecord CreateExternalRecord(string domain, string type, byte[] data)
        {
            return NfcDefRecord.Empty;
        }

        public NfcDefRecord CreateMimeRecord(string mimeType, byte[] mimeData)
        {
             return NfcDefRecord.Empty;
        }

        public NfcDefRecord CreateTextRecord(string languageCode, string text)
        {
            return NfcDefRecord.Empty;
        }

        public NfcDefRecord CreateUriRecord(Uri uri)
        {
           return NfcDefRecord.Empty;
        }
    }
}