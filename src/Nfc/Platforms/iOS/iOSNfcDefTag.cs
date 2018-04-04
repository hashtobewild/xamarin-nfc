using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreNFC;

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
}