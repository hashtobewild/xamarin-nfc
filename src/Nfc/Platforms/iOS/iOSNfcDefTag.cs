using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreNFC;

namespace Plugin.Nfc
{
    public class iOSNfcDefTag : INfcTag
    {
        public bool IsWriteable => false;
        public NfcDefRecord[] Records { get; }
        public string TagId => null;

        public bool HasNfcDefRecords => Records != null && Records.Length > 0;

        public iOSNfcDefTag(IEnumerable<NFCNdefMessage> records)
        {
            Records = records
                .SelectMany(r => r.Records.Select(m => new iOSNdefRecord(m)))
                .ToArray();
        }

        public Task<bool> WriteMessage(NfcDefMessage message) => throw new NotSupportedException();

        public void Dispose()
        {

        }

        public Task<bool> Lock() => throw new NotSupportedException();
    }
}