# **Simple RAT (Remote Access Tool)**

A **simple, yet powerful Remote Access Tool (RAT)** written in **C#** that enables remote control of a system. This tool allows attackers to execute commands, capture keystrokes in real-time, and retrieve system information from the target machine. The project is designed to educate users about the inner workings of RATs and help them understand how to prevent such attacks.

---

## **🚀 Features**

- **🔑 Keylogger**: Capture every keystroke typed on the victim machine and save it to a file on the attacker's machine in **real-time**.
- **💻 System Information Gathering**: Retrieve detailed information about the victim's machine, including OS, CPU, RAM, and more.
- **⚡ Remote Command Execution**: Execute commands on the victim machine remotely via a **simple command shell**.
- **📜 Logs**: All captured keystrokes are logged and stored in a **beautiful text file** for easy tracking.
- **🔐 Secure Communication**: Communication between the client (victim) and server (attacker) happens over TCP.

---

## **📂 Project Structure**

```plaintext
Simple-RAT/
├── client.cs # Main logic for RAT client
├── server.cs # Main logic for RAT server
├── logs.txt             # Keystroke logs (created on the server)
├── README.md            # Project documentation
