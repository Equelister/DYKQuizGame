using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace DYKClient.Net.IO
{
    class PacketReader : BinaryReader
    {

        public NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public string ReadMessage()
        {
            byte[] msgBuffer;
            var length3 = ReadInt32();
            Console.WriteLine("Length: " + length3);
            msgBuffer = new byte[length3];

            var abc = 0;
            if (_ns.CanRead)
            {
                abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);
                _ns.Flush();
            }
            Console.WriteLine(abc);

            if (abc == 0)
            {
                Console.WriteLine(abc);
                return null;
            }
            var msg = Encoding.UTF8.GetString(msgBuffer.ToArray());
            return msg;
        }

    }
}
