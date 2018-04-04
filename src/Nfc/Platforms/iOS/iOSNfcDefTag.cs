using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreNFC;
using Foundation;

namespace Plugin.Nfc
{
    public class iOSNfcDefTag : INfcTag
    {
        public bool IsWriteable { get; }
        public NfcDefRecord[] Records { get; }

        public string TagId {get;}

        public bool HasNfcDefRecords => Records != null && Records.Length > 0;

        public iOSNfcDefTag(IEnumerable<NFCNdefMessage> records)
        {
            IsWriteable = false;
            Records = records
                .SelectMany(r => r.Records.Select(m => new iOSNdefRecord(m)))
                .ToArray();
        }

        public Task<bool> WriteMessage(NfcDefMessage message)
        {
           return Task.FromResult(true);
        }

        public void Dispose()
        {
           
        }
    }

    public class iOSMifareUltraLightTag : INfcTag
    {
        public string TagId => null;
        public bool IsWriteable => false;
        public bool HasNfcDefRecords => Records != null && Records.Length > 0;
        public NfcDefRecord[] Records { get; }

        public void Dispose()
        {

        }

        public Task<bool> WriteMessage(NfcDefMessage message)
        {
            return Task.FromResult(true);
        }

        public static NFCNdefMessage GetMessage(IEnumerable<byte[]> data)
        {
            var array = data.ToArray();
            var r = ByteArrayExtensions.Combine(array);
            var control = array[0];
            var ln = (int)control[0];
            var t = Convert.ToChar(control[1]);

            if (t == NfcRecordTypeConstants.RTD_MIFAREULTRALIGHT_NDEF_MESSAGE)
            {
                var x = new byte[ln];
                Buffer.BlockCopy(r, 4, x, 0, ln);
                return new NFCNdefMessage(new NSCoder())
                {
                    Records = GetNdefRecordFromData(x)
                };
            }

            return null;

        }

        private static NFCNdefPayload[] GetNdefRecordFromData(byte[] x) => throw new NotImplementedException();
    }
}