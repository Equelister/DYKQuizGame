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

        public NetworkStream _ns;
        public PacketReader(NetworkStream ns) : base(ns)
        {
            _ns = ns;
        }

        public string ReadMessage()
        {
            /* try
             {*/
                          byte[] msgBuffer;
                            //var length2 = ReadByte();
                            var length3 = ReadInt32();
            Console.WriteLine("Length: " + length3);
            msgBuffer = new byte[length3];
            //Console.WriteLine(_ns.Read(msgBuffer, 0, length));

            /*            var length = ReadInt32();
                            byte[] msgBuffer = new byte[ReadInt32()];*/
            /*
                        List<byte[]> msgBuffer = new List<byte[]>();
                        byte[] buffer = new byte[ReadInt32()];*/
            /*                Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                            Console.WriteLine("XXXXXXXXX   length: " + msgBuffer.Length + "        XXXXXXXXXXXXXXXXXX");
                            Console.WriteLine("XXXXXXXXX   length: " + msgBuffer + "        XXXXXXXXXXXXXXXXXX");
                            Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
                            Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");*/

            //var xd = _ns.Position;

                var abc = 0;
                if (_ns.CanRead)
                {
                    //_ns.Read(msgBuffer, 0, msgBuffer.Length);
/*
                    abc = _ns.Read(msgBuffer, 0, 1);
                    abc = _ns.Read(msgBuffer, 0, 1);
                    abc = _ns.Read(msgBuffer, 0, 1);
                    abc = _ns.Read(msgBuffer, 0, 1);
                    abc = _ns.Read(msgBuffer, 0,2);
                    abc = _ns.Read(msgBuffer, 0, 2);
                    abc = _ns.Read(msgBuffer, 0, 2);
                    abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);
                    abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);
                    abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);
                    abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);
                    abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);*/
                    abc = _ns.Read(msgBuffer, 0, msgBuffer.Length);
                    _ns.Flush();
                }
            //Console.WriteLine(abc);
            //Console.WriteLine(msgBuffer.ToString());
            Console.WriteLine(abc);

            if (abc == 0)
                {
                    Console.WriteLine(abc);
                    return null;
                }
            var msg = Encoding.ASCII.GetString(msgBuffer.ToArray());
                return msg;

            /*catch(System.ObjectDisposedException ODE)
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
            }*/

            return "";
        }

    }
}
