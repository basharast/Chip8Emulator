using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Core
{
    public class Cart
    {
        byte[] _bytes;

        public Cart(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte[] Bytes
        {
            get { return _bytes; }
        }
    }
}