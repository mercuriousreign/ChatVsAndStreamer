using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class QuizPrompt
{
	public string question;
	public string[] options;
	public int correctIndex;
}

public class StreamerBotListener : MonoBehaviour
{
	public static StreamerBotListener Instance { get; private set; }

	[SerializeField]
	private TextMeshProUGUI StreamerBotStatus;
	[SerializeField]
	private TextMeshProUGUI StreamerBotMSG;
	private UdpClient udpClient;
	private int port = 11000;
	private StreamerBotMovement movementScript;
	private readonly ConcurrentQueue<string> pendingMessages = new();

	public QuizPrompt CurrentQuiz { get; private set; }
	public event Action<QuizPrompt> QuizReceived;

	void Awake()
	{
		if (Instance != null && Instance != this)
			Debug.LogWarning("[StreamerBotListener] Another listener is already active.");
		else
			Instance = this;
	}

	void Start()
	{
		if (Instance != this)
			return;

		movementScript = GetComponent<StreamerBotMovement>();
		if (movementScript == null)
			Debug.LogError("[StreamerBotListener] Add StreamerBotMovement on the same GameObject.");

		try
		{
			udpClient = new UdpClient(port);
			udpClient.BeginReceive(OnReceive, null);
			Debug.Log($"[StreamerBotListener] Listening on UDP port {port} (Streamer.bot → this machine).");
			if (StreamerBotStatus != null)
				StreamerBotStatus.text = $"[StreamerBotListener] Listening on UDP port {port} (Streamer.bot → this machine).";
		}
		catch (SocketException e)
		{
			Debug.LogError($"[StreamerBotListener] Could not bind port {port}: {e.Message}. Another app may be using it.");
			if (StreamerBotStatus != null)
				StreamerBotStatus.text += $"[StreamerBotListener] Could not bind port {port}: {e.Message}. Another app may be using it.";
		}
	}

	private void Update()
	{
		if (Instance != this)
			return;

		while (pendingMessages.TryDequeue(out string message))
		{
			if (movementScript != null)
				movementScript.ParseUDPMessage(message);

			ParseQuiz(message);

			if (message == "spawn_enemy")
				Debug.Log("[StreamerBotListener] spawn_enemy (hook your spawn logic here).");
		}
	}

	public void ParseQuiz(string message)
	{
		if (!TryParseQuiz(message, out QuizPrompt quiz))
			return;

		CurrentQuiz = quiz;
		Debug.Log($"[StreamerBotListener] Quiz ready: \"{quiz.question}\" ({quiz.options.Length} options, correct index {quiz.correctIndex})");
		if (StreamerBotMSG != null)
			StreamerBotMSG.text = quiz.question;

		QuizReceived?.Invoke(quiz);
	}

	public static bool TryParseQuiz(string message, out QuizPrompt quiz)
	{
		quiz = null;
		if (string.IsNullOrWhiteSpace(message))
			return false;

		string trimmed = message.Trim();
		if (trimmed.Length >= 2 && trimmed[0] == '"' && trimmed[trimmed.Length - 1] == '"')
			trimmed = trimmed.Substring(1, trimmed.Length - 2).Trim();

		string body;
		if (trimmed.StartsWith("!quiz", StringComparison.OrdinalIgnoreCase))
			body = trimmed.Substring(5);
		else if (trimmed.StartsWith("quiz", StringComparison.OrdinalIgnoreCase))
			body = trimmed.Substring(4);
		else
			return false;

		body = body.TrimStart(' ', ':', '=', '-');
		if (body.Length == 0)
			return false;

		string[] parts = body.Split('|');
		if (parts.Length < 4)
		{
			Debug.LogWarning("[StreamerBotListener] Quiz needs a question, at least two options, and an answer. Example: !quiz What is 2+2?|4|3|5|22|0");
			return false;
		}

		for (int i = 0; i < parts.Length; i++)
			parts[i] = parts[i].Trim();

		if (string.IsNullOrEmpty(parts[0]))
			return false;

		int optionCount = parts.Length - 2;
		var options = new string[optionCount];
		for (int i = 0; i < optionCount; i++)
		{
			options[i] = parts[i + 1];
			if (string.IsNullOrEmpty(options[i]))
				return false;
		}

		string answerToken = parts[parts.Length - 1];
		int correct;
		if (int.TryParse(answerToken, out int index) && index >= 0 && index < options.Length)
			correct = index;
		else
		{
			correct = -1;
			for (int i = 0; i < options.Length; i++)
			{
				if (string.Equals(options[i], answerToken, StringComparison.OrdinalIgnoreCase))
				{
					correct = i;
					break;
				}
			}

			if (correct < 0)
			{
				Debug.LogWarning($"[StreamerBotListener] \"{answerToken}\" is not an option index or an option text.");
				return false;
			}
		}

		quiz = new QuizPrompt
		{
			question = parts[0],
			options = options,
			correctIndex = correct
		};
		return true;
	}

	[ContextMenu("Send Test Quiz")]
	void SendTestQuiz()
	{
		ParseQuiz("!quiz What is 2 + 2?|4|3|5|22|0");
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
			if (StreamerBotMSG != null)
				StreamerBotMSG.text = $"[StreamerBotListener] UDP from {remoteIp}: {message}";

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
		if (Instance == this)
			Instance = null;

		udpClient?.Close();
		udpClient = null;
	}
}
