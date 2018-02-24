using System;
using System.Threading;
using System.Threading.Tasks;
using CoreFoundation;
using CoreNFC;
using Foundation;

namespace Plugin.Nfc
{
    public class NfcReader : NSObject, INFCNdefReaderSessionDelegate
    {
        private NFCNdefReaderSession _session;
        private TaskCompletionSource<INfcDefTag> _tcs;

        public Task<INfcDefTag> ScanAsync(CancellationToken token = default(CancellationToken))
        {
            if (!NFCNdefReaderSession.ReadingAvailable)
            {
                throw new InvalidOperationException("Reading NDEF is not available");
            }

            _tcs = new TaskCompletionSource<INfcDefTag>();
            _session = new NFCNdefReaderSession(this, DispatchQueue.CurrentQueue, true);
            _session.BeginSession();
            
            token.Register(() =>
            {
                _session.InvalidateSession(); 
            });
            return _tcs.Task;
        }

        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            if (error == null)
            {
                _tcs.SetResult(null);
                return;
            }
            
            var readerError = (NFCReaderError)(long)error.Code;
            if (readerError != NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead &&
                readerError != NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                _tcs.TrySetException(new Exception(error.LocalizedFailureReason));
            }
            else if (readerError == NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                _tcs.TrySetCanceled();
            }
            else
            {
                _tcs.TrySetException(new Exception(error.LocalizedFailureReason));
            }
          
            
        }

        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            _tcs.SetResult(new iOSNfcDefTag(messages));
        }
    }
}