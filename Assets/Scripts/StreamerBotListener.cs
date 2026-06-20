using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class StreamerBotListener : MonoBehaviour
{
	private UdpClient udpClient;
	private int port = 11000;
	private StreamerBotMovement movementScript;
	private readonly ConcurrentQueue<string> pendingMessages = new();

	void Start()
	{
		movementScript = GetComponent<StreamerBotMovement>();
		if (movementScript == null)
			Debug.LogError("[StreamerBotListener] Add StreamerBotMovement on the same GameObject.");

		try
		{
			udpClient = new UdpClient(port);
			udpClient.BeginReceive(OnReceive, null);
			Debug.Log($"[StreamerBotListener] Listening on UDP port {port} (Streamer.bot → this machine).");
		}
		catch (SocketException e)
		{
			Debug.LogError($"[StreamerBotListener] Could not bind port {port}: {e.Message}. Another app may be using it.");
		}
	}

	private void Update()
	{
		while (pendingMessages.TryDequeue(out string message))
		{
			if (movementScript != null)
				movementScript.ParseUDPMessage(message);

			if (message == "spawn_enemy")
				Debug.Log("[StreamerBotListener] spawn_enemy (hook your spawn logic here).");
		}
	}

	private void OnReceive(IAsyncResult res)
	{
		if (udpClient == null) return;

		try
		{
			IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, port);
			byte[] receivedBytes = udpClient.EndReceive(res, ref remoteIp);
			string message = Encoding.UTF8.GetString(receivedBytes);
			Debug.Log($"[StreamerBotListener] UDP from {remoteIp}: {message}");
			pendingMessages.Enqueue(message);
		}
		catch (ObjectDisposedException)
		{
			return;
		}
		catch (Exception e)
		{
			Debug.LogWarning($"[StreamerBotListener] Receive error: {e.Message}");
		}

		try
		{
			udpClient.BeginReceive(OnReceive, null);
		}
		catch (ObjectDisposedException) { }
	}

	void OnDestroy()
	{
		udpClient?.Close();
		udpClient = null;
	}
}
