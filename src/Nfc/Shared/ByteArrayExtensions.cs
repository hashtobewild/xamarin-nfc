using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Plugin.Nfc
{
    public static class ByteArrayExtensions
    {
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (byte[] data in arrays)
            {
                var b = data.Select(m => m != 0x00);
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }

            return ret;
        }
    }
}
