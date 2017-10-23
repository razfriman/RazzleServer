﻿using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using MapleLib.Helper;
using MapleLib.MapleCryptoLib;

namespace MapleLib.PacketLib
{
	public class ClientSocket : IDisposable
	{
		private readonly Socket _socket;
		private readonly byte[] _socketBuffer;
		private readonly string _host;
        private readonly byte[] _hostBytes;
		private readonly int _port;
		private readonly object _disposeSync;
        private readonly IClient _client;
		private bool disposed;

		private static ILogger Log = LogManager.Log;

		public MapleCipherProvider Crypto { get; private set; }
		public bool Connected => !disposed;
		public string Host => _host;
        public byte[] HostBytes => _hostBytes;
		public int Port => _port;

        public ClientSocket(Socket socket, IClient client, ushort currentGameVersion, ulong aesKey)
		{
			_socket = socket;
			_socketBuffer = new byte[1024];
			_host = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
            _hostBytes = ((IPEndPoint)socket.RemoteEndPoint).Address.GetAddressBytes();
			_port = ((IPEndPoint)socket.LocalEndPoint).Port;
			_disposeSync = new object();
			_client = client;

            Crypto = new MapleCipherProvider(currentGameVersion, aesKey);
			Crypto.PacketFinished += (data) =>
			{
				_client.RecvPacket(new PacketReader(data));
			};

			WaitForData();
		}
		private void WaitForData()
		{
			if (!disposed)
			{
				SocketError error = SocketError.Success;

				var socketArgs = new SocketAsyncEventArgs()
				{
					SocketFlags = SocketFlags.None
				};

				socketArgs.SetBuffer(_socketBuffer, 0, _socketBuffer.Length);
				socketArgs.Completed += PacketReceived;
				_socket.ReceiveAsync(socketArgs);

				if (error != SocketError.Success)
				{
					Disconnect();
				}
			}
		}

		private void PacketReceived(object sender, SocketAsyncEventArgs e)
		{
			if (!disposed)
			{
				int size = e.BytesTransferred;

				if (size == 0 || e.SocketError != SocketError.Success)
				{
					Disconnect();
				}
				else
				{
					Crypto.AddData(_socketBuffer, 0, size);
					WaitForData();
				}
			}
		}

		public void SendRawPacket(byte[] final)
		{
			if (!disposed)
			{
				int offset = 0;

				while (offset < final.Length)
				{
					SocketError outError = SocketError.Success;
					int sent = _socket.Send(final, offset, final.Length - offset, SocketFlags.None, out outError);

					if (sent == 0 || outError != SocketError.Success)
					{
						Disconnect();
						return;
					}

					offset += sent;
				}
			}
		}

		public void SendPacket(PacketWriter data)
		{
			if (!disposed)
			{
				var buffer = data.ToArray();
				Crypto.Encrypt(ref buffer, true);
				SendRawPacket(buffer);
			}
		}

		public void Disconnect()
		{
			Log.LogInformation("Client Disconnected");
			_client.Disconnected();
			Dispose();
		}

		public void Dispose()
		{
			lock (_disposeSync)
			{
				if (disposed)
					return;

				disposed = true;

				try
				{
					_socket.Shutdown(SocketShutdown.Both);
				}
				finally
				{
					_client.Disconnected();
				}
			}
		}

		~ClientSocket() => Dispose();
	}
}
