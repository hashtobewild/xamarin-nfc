using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.Nfc
{
    public enum NfcReadError
    {
        TagResponseError,
        SessionTimeout,
        SessionTerminatedUnexpectedly,
    }

    public class NfcReadException : Exception
    {
        public NfcReadError ErrorType {get; }

        public NfcReadException(NfcReadError error) : this(error, "Failed read from NFC Tag")
        {
          
        }

        public NfcReadException(NfcReadError error, string message) : this(error, message, null)
        {

        }

        public NfcReadException(NfcReadError error, Exception inner) : this(error, "Failed read from NFC Tag", inner)
        {

        }

        public NfcReadException(NfcReadError error, string message, Exception inner) : base(message, inner)
        {
              ErrorType = error;
        }
    }
}
