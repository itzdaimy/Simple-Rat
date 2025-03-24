# **Simple RAT (Remote Access Tool)**

A **simple, yet powerful Remote Access Tool (RAT)** written in **C#** that enables remote control of a system. This tool allows attackers to execute commands, capture keystrokes in real-time, and retrieve system information from the target machine. The project is designed to educate users about the inner workings of RATs and help them understand how to prevent such attacks.
for support you may contact me on discord, my username is daimyh
---

**🦠 Virus total**: [https://www.virustotal.com/](https://www.virustotal.com/gui/file/da2a28b696269f41d2b3300298a455b67889ed3223930ece69d12dc288b43931?nocache=1)

## **🚀 Features**

- **🔑 Keylogger**: Capture every keystroke typed on the victim machine and save it to a file on the attacker's machine in **real-time**.
- **💻 System Information Gathering**: Retrieve detailed information about the victim's machine, including OS, CPU, RAM, and more.
- **⚡ Remote Command Execution**: Execute commands on the victim machine remotely via a **simple command shell**.
- **📜 Logs**: All captured keystrokes are logged and stored in a **beautiful text file** for easy tracking.
- **🖼️ Screenshot Capture**: Take screenshots of the victim's desktop and save them on the attacker's machine.
- **👥 Multi-Client Support**: Support for multiple clients connected to the server simultaneously, allowing control of multiple victim machines.
- **🔐 Secure Communication**: Communication between the client (victim) and server (attacker) happens over TCP.
- **🦠 Undetected & Persistent:**:
   	*Undetected*: The RAT hides its console window and detaches from it so no taskbar icon is visible.
   	*Persistence*  Automatically copies itself to the AppData folder and adds a registry key to run at startup.
- **🔄 Auto-Reconnect**: If the connection to the server fails, it will retry every 2 minutes.
---

## **📂 Project Structure**

```plaintext
Simple-RAT/
├── client.cs            # Main logic for RAT client
├── server.cs            # Main logic for RAT server
├── logs/
│   └── keylog.txt       # Keystroke logs (created on the server)
├── screenshots/         # Directory to store captured screenshots
├── README.md            # Project documentation
```

## **📜 Example Logs**

When the victim types something like this: my password is 123

The **logs.txt** on the attacker's machine will look like this: 
```plaintext
my password is 123 [ENTER]
```

---

## **💻 Build into exe**

1. Put both the client.cs and server.cs into 1 folder
2. Open up cmd in the folder.
3. type "dotnet new console -n server" and "dotnet new console -n client"
4. then put your cmd in each folders it created and run "dotnet publish -c Release -r win-x64 --self-contained" OR you can use "dotnet publish -c Release -r win-x64 --self-contained false" (user must have .net installed)
5. done!


## **⚠️ Warning**

This project is for **educational purposes only**. It is essential to understand how remote access tools work so that you can protect your own systems and networks. **Do not use this tool on machines you don't own or have explicit permission to test**.

Using this tool without permission is illegal and unethical. Always follow **ethical hacking** principles and ensure that you're testing only on machines that you have permission to access or on your own devices.

---

Please be responsible and use this knowledge to improve security and learn more about the techniques used by attackers and defenders.
