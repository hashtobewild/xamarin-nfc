using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Nfc
{
    public class NfcWriteException : Exception
    {
        public NfcWriteException() : base("Failed writing to NFC Tag")
        {

        }

        public NfcWriteException(string message) : base(message)
        {

        }

        public NfcWriteException(Exception inner) : base("Failed writing to NFC Tag", inner)
        {

        }

        public NfcWriteException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
