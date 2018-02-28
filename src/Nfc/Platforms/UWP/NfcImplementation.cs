using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Nfc
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    public class NfcImplementation : INfc
    {
        public event TagDetectedDelegate TagDetected;
        public event TagErrorDelegate TagError;

        public ValueTask<bool> IsAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> IsEnabledAsync()
        {
            throw new NotImplementedException();
        }

        public Task StartListeningAsync(CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task StopListeningAsync()
        {
            throw new NotImplementedException();
        }
    }
}
