using System;
using System.IO;
using System.Text;

namespace DYKClient.Net.IO
{
    class PacketBuilder
    {
        MemoryStream _ms;
        public PacketBuilder()
        {
            _ms = new MemoryStream();
        }

        public void WriteOpCode(byte opcode)
        {
            _ms.WriteByte(opcode);
        }

        public void WriteString(string msg)
        {
            var msgLength = msg.Length;
            Console.WriteLine(msgLength + ", msg: " + msg);
            _ms.Write(BitConverter.GetBytes(msgLength));
            _ms.Write(Encoding.UTF8.GetBytes(msg));
        }

        public byte[] GetPacketBytes()
        {
            return _ms.ToArray();
        }
    }
}
