using System;
using System.Text;

namespace ESMReader
{
    class StringReader
    {
        private int length;
        private byte[] oldBytes;
        private int index = 0;

        public StringReader(int stringLength)
        {
            length = stringLength;
            int arrayLength = (int)Math.Ceiling(stringLength / 4m) * 4;
            oldBytes = new byte[arrayLength];
        }

        public bool Add(byte[] newBytes)
        {
            oldBytes[index] = newBytes[0];
            oldBytes[index + 1] = newBytes[1];
            oldBytes[index + 2] = newBytes[2];
            oldBytes[index + 3] = newBytes[3];
            index += 4;
            return index >= length;
        }

        public String Read()
        {
            return Encoding.UTF8.GetString(oldBytes, 0, length);
        }
    }
}
