using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreNFC;

namespace Plugin.Nfc
{
    public class iOSMifareUltraLightTag : INfcTag
    {
        public string TagId => null;
        public bool IsWriteable => false;
        public bool HasNfcDefRecords => Records != null && Records.Length > 0;
        public NfcDefRecord[] Records { get; }

        public iOSMifareUltraLightTag(NfcDefRecord[] records)
        {
            if (records != null && records.Length > 0)
            {
                Records = records;
            }
            else
            {
                Records = new NfcDefRecord[] { };
            }
        }

        public void Dispose()
        {

        }

        public Task<bool> WriteMessage(NfcDefMessage message) => throw new NotSupportedException();
      
        public static NfcDefRecord[] GetMessage(IEnumerable<byte[]> data)
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
                return iOSNdefRecordExtensions.Create(x);
            }

            return null;

        }

        public Task<bool> Lock() => throw new NotSupportedException();
    }
}