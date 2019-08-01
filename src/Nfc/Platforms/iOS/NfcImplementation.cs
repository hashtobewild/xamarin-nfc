﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreFoundation;
using CoreNFC;
using Foundation;

namespace Plugin.Nfc
{
    public class NfcImplementation : NSObject, INfc, INFCNdefReaderSessionDelegate
    {
        public event EventHandler<TagDetectedEventArgs> TagDetected;
        public event EventHandler<TagErrorEventArgs> TagError;
        private NFCNdefReaderSession _session;
        
        public bool IsAvailable()
        {
            return NFCNdefReaderSession.ReadingAvailable;
        }

        public bool IsEnabled()
        {
            return NFCNdefReaderSession.ReadingAvailable;
        }

      
        public void StartListening()
        {
            try
            {
                if (!this.IsAvailable())
                {
                    TagError?.Invoke(null, new TagErrorEventArgs(new InvalidOperationException("NFC is not available")));
                    return;
                }

                _session = new NFCNdefReaderSession(this, DispatchQueue.CurrentQueue, false);
                _session.BeginSession();
            }
            catch(Exception ex)
            {
                TagError?.Invoke(null, new TagErrorEventArgs(ex));
            }
           
        }

        public void StopListening()
        {
             _session?.InvalidateSession();
             _session?.Dispose();
        }

        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            if (error == null)
            {
                return;
            }
            
            var readerError = (NFCReaderError)(long)error.Code;
            if (readerError != NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead &&
                readerError != NFCReaderError.ReaderSessionInvalidationErrorUserCanceled &&
                readerError != NFCReaderError.ReaderSessionInvalidationErrorSessionTimeout && 
                readerError != NFCReaderError.ReaderSessionInvalidationErrorSessionTerminatedUnexpectedly)
            {
               TagError?.Invoke(null, new TagErrorEventArgs(new NfcReadException(NfcReadError.TagResponseError, error.LocalizedFailureReason)));
            }
            else if (readerError == NFCReaderError.ReaderSessionInvalidationErrorUserCanceled ||
                 readerError == NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead ||
                 readerError == NFCReaderError.ReaderSessionInvalidationErrorSessionTimeout || 
                 readerError == NFCReaderError.ReaderSessionInvalidationErrorSessionTerminatedUnexpectedly
                 )
            {
                
            }
            else
            {
                TagError?.Invoke(null, new TagErrorEventArgs(new NfcReadException(NfcReadError.TagResponseError, error.LocalizedFailureReason)));
            }
          
            
        }

        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            TagDetected?.Invoke(null, new TagDetectedEventArgs(new iOSNfcDefTag(messages)));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing)
            {
                _session?.Dispose();
            }
        }

        public void SetSupportedTechnologies(IEnumerable<NfcTechnologyType> supportedTechnologies)
        {

        }
    }
}