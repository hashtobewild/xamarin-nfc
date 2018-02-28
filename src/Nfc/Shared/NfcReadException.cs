using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Nfc
{
     public class NfcReadException : Exception
    {
        public NfcReadException() : base("Failed read from NFC Tag")
        {

        }

        public NfcReadException(string message) : base(message)
        {

        }

        public NfcReadException(Exception inner) : base("Failed read from NFC Tag", inner)
        {

        }

        public NfcReadException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
