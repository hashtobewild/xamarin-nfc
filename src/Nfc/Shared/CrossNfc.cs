using System;
using System.Threading;

namespace Plugin.Nfc
{
    public partial class CrossNfc
    {
        private static Lazy<INfc> _implementation = new Lazy<INfc>(CreateNfc, LazyThreadSafetyMode.PublicationOnly);
        private static Lazy<INfcDefRecordFactory> _factory = new Lazy<INfcDefRecordFactory>(CreateFactory, LazyThreadSafetyMode.PublicationOnly);
        private static Lazy<INfcDefRecordConverter> _converter = new Lazy<INfcDefRecordConverter>(CreateConverter,LazyThreadSafetyMode.PublicationOnly);

        private static INfcDefRecordConverter CreateConverter()
        {
            return new NfcDefRecordConverter();
        }

        public static bool IsSupported => _implementation.Value == null ? false : true;

        public static INfcDefRecordConverter CurrentConverter
        {
            get
            {
                var ret = _converter.Value;
                if (ret == null)
                {
                    NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        public static INfcDefRecordFactory CurrentFactory
        {
            get
            {
                var ret = _factory.Value;
                if (ret == null)
                {
                    NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static INfcDefRecordFactory CreateFactory()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#else
            return new NfcDefRecordFactoryImplementation();
#endif
        }

        public static INfc Current
        {
            get
            {
                var ret = _implementation.Value;
                if (ret == null)
                {
                    NotImplementedInReferenceAssembly();
                }
                return ret;
            }
        }

        private static INfc CreateNfc()
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            throw NotImplementedInReferenceAssembly();
#else
            return new NfcImplementation();
#endif
        }

        public static void Dispose()
        {
            if (_implementation != null && _implementation.IsValueCreated)
            {
                _implementation = new Lazy<INfc>(CreateNfc, LazyThreadSafetyMode.PublicationOnly);
            }

            if (_factory != null && _factory.IsValueCreated)
            {
                _factory = new Lazy<INfcDefRecordFactory>(CreateFactory, LazyThreadSafetyMode.PublicationOnly);
            }

            if (_converter != null && _converter.IsValueCreated)
            {
                _converter = new Lazy<INfcDefRecordConverter>(CreateConverter, LazyThreadSafetyMode.PublicationOnly);
            }
        }

        private static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}