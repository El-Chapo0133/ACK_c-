using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web.Script.Serialization;

namespace project.socket.ack {
	public class Ack {
		private Ip ips;
		public void Init(string ip_remote) {
			this.ips = new Ip();
			ips.Remote = ip_remote;
			ips.Local = this.getLocalIp();
		}
		public void Ack_Send() {
			if (this.isPingable(this.ips.Remote)) {
				string toSend = this.stringifyJson(this.generateJsonSend());
				Socket socket = this.generateSocket_send();
				socket.Send(this.convertSendMessage(toSend));
			}
		}
		public void StartResponde() {
			Buffer_perso buffer = new Buffer_perso();
			Socket socket = this.generateSocket_responde(buffer);
		}

		private void ack_Receive() {
			string toSend = this.stringifyJson(this.generateJsonReceived());
			Socket socket = this.generateSocket_responde(output.source, buffer);
			socket.Send(this.convertSendMessage(toSend));
		}
		private messageCallback(IAsyncResult input) {
			try {
				var output = this.serializeJson(this.convertReceivedMessage(input));
				if (this.isAckSend(output)) {
					this.ack_Receive(output.source);
				}
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		}
		private bool isAckSend(var input) {
			if (input.type == "send")
				return true;
			else
				return false;
		}
		private string convertReceivedMessage(byte[] input) {
			ASCIIEncoding aEncoding = new ASCIIEncoding();
			return aEncoding.GetString(input);
		}
		private byte[] convertSendMessage(string input) {
			ASCIIEncoding aEncding = new ASCIIEncoding();
			return aEncoding.GetBytes(input);
		}
		private Socket generateSocket_send() {
			Port ports = new Port();
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocoleType.Udp);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAdrress, true);
			EndPoint epLocal = new IPEndPoint(IPAddess.Parse(this.ips.Local), Convert.ToInt32(ports.Local));
			EndPoint epRemote = new IPEndPoint(IPAddress.Parse(this.ips.remote), Convert.ToInt32(ports.Remote));
			socket.Connect(epRemote);
			// TODO : start listening
		}
		private Socket generateSocket_responde(string ip_remote, Buffer_perso buffer) {
			Port ports = new Port();
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocoleType.Udp);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAdrress, true);
			EndPoint epLocal = new IPEndPoint(IPAddess.Parse(this.ips.Local), Convert.ToInt32(ports.Remote));
			EndPoint epRemote = new IPEndPoint(IPAddress.Parse(ip_remote), Convert.ToInt32(ports.Local));
			socket.Connect(epRemote);
			socket.BeginReceiveFrom(buffer.BufferByte, 0, buffer.BufferByte.Length, SocketFlags.None, ref epRemote, new asyncCallback(messageCallback), buffer.BufferByte);
		}
		private string getLocalIp() {
			IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            // try to get local ip
            foreach (IPAddress ip in host.AddressList)
            {
                if (isOurIp(ip))
                    return ip.ToString();
            }
            // else, return local default ip
            return "127.0.0.1";

            // SOURCE : https://github.com/El-Chapo0133/SimpleClientServer/blob/master/SimpleClientServer/SimpleClientServer/_object/Ip.cs
		}
		private bool isOurIp(IPAddress ip) {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return true;
            else
                return false;
            // SOURCE : https://github.com/El-Chapo0133/SimpleClientServer/blob/master/SimpleClientServer/SimpleClientServer/_object/Ip.cs
        }
		private bool isPingable(string ip) {
			Ping ping = new Ping();

			try {
				return this.ping(ip, ping);
			} catch (PingException ex) {
				Debug.WriteLine(ex);
				return false;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return false;
			}
		}
		private bool ping(string ip, Ping ping) {
			PingReply pong = ping.Send(ip);
			return (pong.Status == IPStatus.Success)
		}
		private any generateJsonSend() {
			Ack_obj objSend = new Ack_obj {
				source = this.ips.Local;
				destination = this.ips.Remote;
				type: "Send";
				timestamp = this.getTimestamp();
			}

			return this.serializeSend(objSend);
		}
		private any generateJsonReceived() {
			Ack_obj objSend = new Ack_obj {
				source = this.ips.Local;
				destination = this.ips.Remote;
				type: "Received";
				timestamp = this.getTimestamp();
			}

			return this.serializeSend(objSend);
		}
		private string stringifyJson(var json) {
			return JsonSerializer.Deserialize(json);
		}
		private any serializeJson(string json) {
			return JsonSerializer.Serialize(json);
		}
		private any serializeSend(Send input) {
			return new JavaScriptSerializer().Serialize(input);
			// SOURCE : https://stackoverflow.com/questions/6201529/how-do-i-turn-a-c-sharp-object-into-a-json-string-in-net
		}
		private string getTimestamp() {
			return DateTime.Now;
		}
	}

	private class Ip() {
		private string local;
		private string remote;
		public Local {
			get { return this.local; }
			set { this.local = value; }
		}
		public remote {
			get { return this.remote; }
			set { this.remote = value; }
		}
	}
	private class Ack_obj() {
		public string source;
		public string destination;
		public string type;
		public string timestamp;
	}
	private class Port {
		private string local = "9723";
		private string remote = "2983";
		public string Local {
			get { return this.local; }
		}
		public string Remote {
			get { return this.remote; }
		}
	}
	private class Buffer_perso {
		private const int MAXCHARINMESSAGE = 255;
		private byte[] buffer = new byte[MAXCHARINMESSAGE];
		public byte[] BufferByte {
			get { return this.buffer; }
			set { this.buffer = value; }
		}
	}
}