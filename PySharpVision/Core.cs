using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Net.Sockets;
using System.Windows.Markup;
using System.Net;
using System.Windows;

namespace PySharpVision
{
    class Core
    {

        TcpListener server = null;

        public (TcpListener server, TcpClient client, NetworkStream stream) TcpConnect()
        {
            // Set the TcpListener on port 8080.
            int port = 8080;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            // Create a TcpListener.
            server = new TcpListener(localAddr, port);
            // Start listening for client requests.
            server.Start();
            Console.WriteLine("Waiting for a connection...");
            // Perform a blocking call to accept requests.
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Connected!");
            // Get a stream object for reading and writing.
            NetworkStream stream = client.GetStream();
            // Return both the TcpListener and the NetworkStream.
            return (server, client, stream);
        }

        public void DisTcpConnect()
        {
            server.Stop();
        }

        public void Sendmsg(NetworkStream stream, string message)
        {
            byte[] msg = Encoding.ASCII.GetBytes(message);
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine("Sent: {0}", message);
        }

        public void Getmsg(Byte[] bytes, int i)
        {
            string data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
            Console.WriteLine("Received: {0}", data);
        }

        public string SaveMemory(object input, string memory_name)
        {
            Bitmap bitmap = null;
            if (input is string filePath)
            {
                bitmap = new Bitmap(filePath);
            }
            else if (input is Bitmap inputBitmap)
            {
                bitmap = inputBitmap;
            }
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            var mmf = MemoryMappedFile.CreateOrOpen(memory_name, bytes.Length, MemoryMappedFileAccess.ReadWrite);
            var viewAccessor = mmf.CreateViewAccessor(0, bytes.Length);
            viewAccessor.Write(0, bytes.Length); ;
            viewAccessor.WriteArray<byte>(0, bytes, 0, bytes.Length);
            return bytes.Length.ToString();
        }

        public string SaveMemory1(Bitmap bitmap, string memory_name)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            var mmf = MemoryMappedFile.CreateOrOpen(memory_name, bytes.Length, MemoryMappedFileAccess.ReadWrite);
            var viewAccessor = mmf.CreateViewAccessor(0, bytes.Length);
            viewAccessor.Write(0, bytes.Length); ;
            viewAccessor.WriteArray<byte>(0, bytes, 0, bytes.Length);
            return bytes.Length.ToString();
        }

        public string SaveMemory2(string file_path, string memory_name)
        {
            Bitmap b = new Bitmap(file_path);
            MemoryStream ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] bytes = ms.GetBuffer();
            ms.Close();
            var mmf = MemoryMappedFile.CreateOrOpen(memory_name, bytes.Length, MemoryMappedFileAccess.ReadWrite);
            var viewAccessor = mmf.CreateViewAccessor(0, bytes.Length);
            viewAccessor.Write(0, bytes.Length); ;
            viewAccessor.WriteArray<byte>(0, bytes, 0, bytes.Length);
            return bytes.Length.ToString();
        }
    }
}
