using System;

namespace Plugin.Nfc
{
    public class NfcDefRecordFactoryImplementation : INfcDefRecordFactory
    {
        public NfcDefRecord CreateApplicationRecord(string packageName)
        {
            throw new NotImplementedException();
        }

        public NfcDefRecord CreateExternalRecord(string domain, string type, byte[] data)
        {
            throw new NotImplementedException();
        }

        public NfcDefRecord CreateMimeRecord(string mimeType, byte[] mimeData)
        {
            throw new NotImplementedException();
        }

        public NfcDefRecord CreateTextRecord(string languageCode, string text)
        {
            throw new NotImplementedException();
        }

        public NfcDefRecord CreateUriRecord(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}