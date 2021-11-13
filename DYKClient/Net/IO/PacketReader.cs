using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DYKClient.Net.IO
{
    class PacketReader : BinaryReader
    {

        private NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public string ReadMessage()
        {
            try
            {
                byte[] msgBuffer;
                var length = ReadInt32();
                msgBuffer = new byte[length];
                //Console.WriteLine(_ns.Read(msgBuffer, 0, length));

                if (_ns.CanRead)
                {
                    _ns.Read(msgBuffer, 0, length-1);
                    _ns.Flush();
                }

                var msg = Encoding.ASCII.GetString(msgBuffer);

               // Console.WriteLine("*******\r\n" + _ns.ToString() + "\r\n********");
                return msg;
            }catch(System.ObjectDisposedException ODE)
            {
                Console.WriteLine(ODE.ToString());
               // Console.WriteLine("*******\r\n" + _ns.ToString() + "\r\n********");
            }
            catch(System.IO.IOException ioe)
            {
                Console.WriteLine(ioe.ToString());
               // Console.WriteLine("*******\r\n" + _ns.ToString() + "\r\n********");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
               // Console.WriteLine("*******\r\n" + _ns.ToString() +"\r\n********");
            }

            return "";
        }

    }
}
