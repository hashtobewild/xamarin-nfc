using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreNFC;
using Foundation;

namespace Plugin.Nfc
{
    public class NfcImplementation : NSObject, INfc
    {
        public event TagDetectedDelegate TagDetected;
        public ValueTask<bool> IsAvailableAsync()
        {
            return new ValueTask<bool>(NFCNdefReaderSession.ReadingAvailable);
        }

        public ValueTask<bool> IsEnabledAsync()
        {
            return new ValueTask<bool>(true);
        }

        public async Task StartListeningAsync(CancellationToken token = default(CancellationToken))
        {
            var reader = new NfcReader();
            var tag = await reader.ScanAsync(token);
            TagDetected?.Invoke(tag);
        }

        public async Task StopListeningAsync()
        {
            
        }

    }
}