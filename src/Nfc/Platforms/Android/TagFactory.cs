using Android.Nfc;
using Android.Nfc.Tech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Nfc
{
    public static class TagFactory
    {
        private static int MAX_PAGE_COUNT = 256;

        private static Dictionary<string, Func<Tag, INfcTag>> _creator = new Dictionary<string, Func<Tag, INfcTag>>()
        {
            {Java.Lang.Class.FromType(typeof(Ndef)).Name, (tag) => {
                var ndef = Ndef.Get(tag);
                ndef?.Connect();
                return GetNdefTag(ndef);
            }},

            {Java.Lang.Class.FromType(typeof(MifareUltralight)).Name, (tag) => {
                var mifare = MifareUltralight.Get(tag);
                mifare?.Connect();
                return GetMifareTag(mifare);
            }}
        };

        private static Dictionary<string, Func<Tag, Task<INfcTag>>> _creatorAsync = new Dictionary<string, Func<Tag, Task<INfcTag>>>()
        {
            {Java.Lang.Class.FromType(typeof(Ndef)).Name, async (tag) => {
                var ndef = Ndef.Get(tag);
                await ndef?.ConnectAsync();
                return GetNdefTag(ndef);
            }},

            {Java.Lang.Class.FromType(typeof(MifareUltralight)).Name, async (tag) => {
                var mifare = MifareUltralight.Get(tag);
                await mifare?.ConnectAsync();
                return GetMifareTag(mifare);
            }}
        };


        public static INfcTag Create(string[] techs, Tag tag, IEnumerable<string> supportedTechnologies)
        {
            var typeSupported = supportedTechnologies;

            foreach (var tech in typeSupported)
            {
                if (techs.Contains(tech) && _creator.ContainsKey(tech))
                {
                    return _creator[tech](tag);
                }
            }

            throw new NotSupportedException("tag technology is not supported");

        }

        public static async Task<INfcTag> CreateAsync(string[] techs, Tag tag, IEnumerable<string> supportedTechnologies)
        {
            var typeSupported = supportedTechnologies;

            foreach (var tech in typeSupported)
            {
                if (techs.Contains(tech) && _creatorAsync.ContainsKey(tech))
                {
                    return await _creatorAsync[tech](tag);
                }
            }

            throw new NotSupportedException("tag technology is not supported");
        }

        private static INfcTag GetMifareTag(MifareUltralight mifare)
        {
            var records = new NdefRecord[] { };
            var data = new List<byte[]>();
            try
            {
                for (var i = 4; i < MAX_PAGE_COUNT; i += 4)
                {
                    var pageData = mifare?.ReadPages(i);
                    if (pageData != null && pageData.Length > 0 && !pageData.All(m => m == 0x00))
                    {
                        data.Add(pageData);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                mifare?.Close();
            }

            var msg = MifareUltraLightTag.GetMessage(data);

            if(msg != null)
            {
                records = msg.GetRecords();
            }

            var nfcMifareTag = new MifareUltraLightTag(mifare, records);
            return nfcMifareTag;
        }

        private static INfcTag GetNdefTag(Ndef ndef)
        {
            var records = new NdefRecord[] { };
            try
            {
                records = ndef?.NdefMessage?.GetRecords() ?? new NdefRecord[] { };
            }
            finally
            {
                ndef?.Close();
            }
            var nfcDefTag = new NfcDefTag(ndef, records);
            return nfcDefTag;
        }

    }
}
