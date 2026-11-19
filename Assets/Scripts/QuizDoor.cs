using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Opens a quiz when a player gets close to the door or touches it.
/// The question and answers come from the latest !quiz message parsed by StreamerBotListener.
/// A correct answer swings the door open. Number keys 1-9 pick an answer.
/// </summary>
public class QuizDoor : MonoBehaviour
{
	public Transform doorPivot;
	public Collider blockingCollider;
	public Renderer doorRenderer;
	public Renderer[] frameRenderers;
	public StreamerBotListener listener;

	[Tooltip("Collider names or parent names that count as the player. Boat, Boat2, and BoatParent all match \"Boat\".")]
	public string playerNameContains = "Boat";
	public string playerTag = "";

	public float openAngle = -100f;
	public float openSpeed = 1.5f;
	public bool freezePlayerWhileOpen = true;
	public Vector3 canvasOffset = new Vector3(0f, 2.2f, 0f);

	public Color doorColor = new Color(0.45f, 0.28f, 0.14f, 1f);
	public Color frameColor = new Color(0.22f, 0.18f, 0.16f, 1f);

	int insideCount;
	bool isOpen;
	bool quizVisible;
	bool solved;
	Rigidbody heldBody;
	Transform approacher;
	QuizPrompt showing;

	GameObject canvasRoot;
	Text questionLabel;
	Text statusLabel;
	TextMeshProUGUI questionLabelTmp;
	TextMeshProUGUI statusLabelTmp;
	RectTransform buttonColumn;
	readonly List<Button> optionButtons = new();
	bool useTmp;

	static readonly Key[] answerKeys =
	{
		Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
		Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
	};

	static readonly Key[] numpadKeys =
	{
		Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5,
		Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9
	};

	bool subscribed;

	void Awake()
	{
		if (doorPivot == null || blockingCollider == null)
			BuildGeometry();

		EnsureApproachTrigger();
		Tint(doorRenderer, doorColor);
		if (frameRenderers != null)
		{
			foreach (Renderer frame in frameRenderers)
				Tint(frame, frameColor);
		}

		if (blockingCollider != null && blockingCollider.gameObject != gameObject &&
			blockingCollider.GetComponent<QuizDoorContactRelay>() == null)
		{
			QuizDoorContactRelay relay = blockingCollider.gameObject.AddComponent<QuizDoorContactRelay>();
			relay.door = this;
		}
	}

	void OnEnable()
	{
		TrySubscribe();
	}

	void Start()
	{
		TrySubscribe();
	}

	void OnDisable()
	{
		if (listener != null)
			listener.QuizReceived -= OnQuizReceived;
		subscribed = false;
	}

	void TrySubscribe()
	{
		if (subscribed)
			return;

		StreamerBotListener source = ActiveListener();
		if (source == null)
			return;

		listener = source;
		source.QuizReceived += OnQuizReceived;
		subscribed = true;
	}

	void BuildGeometry()
	{
		frameRenderers = new Renderer[3];
		frameRenderers[0] = CreateBlock("LeftPost", new Vector3(-1.2f, 1.6f, 0f), new Vector3(0.25f, 3.2f, 0.25f));
		frameRenderers[1] = CreateBlock("RightPost", new Vector3(1.2f, 1.6f, 0f), new Vector3(0.25f, 3.2f, 0.25f));
		frameRenderers[2] = CreateBlock("Lintel", new Vector3(0f, 3.25f, 0f), new Vector3(2.65f, 0.25f, 0.25f));

		GameObject pivot = new GameObject("DoorPivot");
		pivot.transform.SetParent(transform, false);
		pivot.transform.localPosition = new Vector3(-1f, 0f, 0f);
		doorPivot = pivot.transform;

		GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leaf.name = "DoorLeaf";
		leaf.transform.SetParent(pivot.transform, false);
		leaf.transform.localPosition = new Vector3(1f, 1.5f, 0f);
		leaf.transform.localScale = new Vector3(1.95f, 2.9f, 0.12f);
		doorRenderer = leaf.GetComponent<Renderer>();
		blockingCollider = leaf.GetComponent<Collider>();
	}

	Renderer CreateBlock(string blockName, Vector3 localPosition, Vector3 scale)
	{
		GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
		block.name = blockName;
		block.transform.SetParent(transform, false);
		block.transform.localPosition = localPosition;
		block.transform.localScale = scale;
		return block.GetComponent<Renderer>();
	}

	void EnsureApproachTrigger()
	{
		BoxCollider approach = GetComponent<BoxCollider>();
		if (approach != null)
			return;

		approach = gameObject.AddComponent<BoxCollider>();
		approach.isTrigger = true;
		approach.center = new Vector3(0f, 1.6f, 0f);
		approach.size = new Vector3(8f, 4f, 8f);
	}

	StreamerBotListener ActiveListener()
	{
		if (StreamerBotListener.Instance != null)
			return StreamerBotListener.Instance;
		return listener;
	}

	void Update()
	{
		if (!quizVisible || solved || showing == null || Keyboard.current == null)
			return;

		int count = Mathf.Min(showing.options.Length, answerKeys.Length);
		for (int i = 0; i < count; i++)
		{
			if (Keyboard.current[answerKeys[i]].wasPressedThisFrame || Keyboard.current[numpadKeys[i]].wasPressedThisFrame)
				SelectOption(i);
		}
	}

	void LateUpdate()
	{
		if (freezePlayerWhileOpen && quizVisible && !solved && heldBody != null)
		{
			heldBody.linearVelocity = Vector3.zero;
			heldBody.angularVelocity = Vector3.zero;
		}

		if (quizVisible && canvasRoot != null)
		{
			Camera view = ActiveCamera();
			if (view != null)
				FaceCanvasAt(view.transform.position);
			else if (approacher != null)
				FaceCanvasAt(approacher.position);
		}
	}

	static Camera ActiveCamera()
	{
		if (Camera.main != null)
			return Camera.main;
		return Camera.allCamerasCount > 0 ? Camera.allCameras[0] : null;
	}

	void OnTriggerEnter(Collider other)
	{
		HandleEnter(other);
	}

	void OnTriggerExit(Collider other)
	{
		HandleExit(other);
	}

	public void HandleEnter(Collider other)
	{
		if (isOpen || !IsPlayer(other))
			return;

		insideCount++;
		if (other.attachedRigidbody != null)
			heldBody = other.attachedRigidbody;
		approacher = other.transform;

		if (insideCount == 1)
			ShowQuiz();
	}

	public void HandleExit(Collider other)
	{
		if (!IsPlayer(other))
			return;

		insideCount = Mathf.Max(0, insideCount - 1);
		if (insideCount == 0 && !isOpen)
		{
			heldBody = null;
			HideQuiz();
		}
	}

	void OnQuizReceived(QuizPrompt quiz)
	{
		if (isOpen)
			return;

		if (quizVisible)
			FillQuiz(quiz);
		else if (insideCount > 0)
			ShowQuiz();
	}

	bool IsPlayer(Collider other)
	{
		if (other == null)
			return false;

		if (string.IsNullOrEmpty(playerNameContains) && string.IsNullOrEmpty(playerTag))
			return other.attachedRigidbody != null;

		Transform current = other.transform;
		while (current != null)
		{
			if (!string.IsNullOrEmpty(playerTag) && HasTag(current.gameObject, playerTag))
				return true;
			if (!string.IsNullOrEmpty(playerNameContains) &&
				current.name.IndexOf(playerNameContains, StringComparison.OrdinalIgnoreCase) >= 0)
				return true;
			current = current.parent;
		}

		return false;
	}

	static bool HasTag(GameObject target, string tag)
	{
		try
		{
			return target.CompareTag(tag);
		}
		catch (UnityException)
		{
			return false;
		}
	}

	void ShowQuiz()
	{
		EnsureCanvas();
		canvasRoot.SetActive(true);
		quizVisible = true;
		StreamerBotListener source = ActiveListener();
		FillQuiz(source != null ? source.CurrentQuiz : null);
		Camera view = ActiveCamera();
		if (canvasRoot.GetComponent<Canvas>().worldCamera == null)
			canvasRoot.GetComponent<Canvas>().worldCamera = view;
		if (view != null)
			FaceCanvasAt(view.transform.position);
		else if (approacher != null)
			FaceCanvasAt(approacher.position);
	}

	void HideQuiz()
	{
		quizVisible = false;
		if (canvasRoot != null)
			canvasRoot.SetActive(false);
	}

	void FillQuiz(QuizPrompt quiz)
	{
		showing = quiz;
		ClearButtons();

		if (quiz == null)
		{
			SetQuestion("The door is locked.");
			SetStatus("Waiting for chat.\n!quiz What is 2+2?|4|3|5|22|0");
			return;
		}

		SetQuestion(quiz.question);
		SetStatus("Pick an answer. Keys 1-" + Mathf.Min(quiz.options.Length, 9) + " work too.");

		for (int i = 0; i < quiz.options.Length; i++)
		{
			int captured = i;
			Button button = CreateButton((i + 1) + ". " + quiz.options[i]);
			button.onClick.AddListener(() => SelectOption(captured));
			optionButtons.Add(button);
		}
	}

	void SelectOption(int index)
	{
		if (solved || showing == null || index < 0 || index >= showing.options.Length)
			return;

		if (index == showing.correctIndex)
		{
			solved = true;
			SetStatus("Correct. The door opens.");
			foreach (Button button in optionButtons)
				button.interactable = false;
			StartCoroutine(OpenDoor());
		}
		else
		{
			SetStatus("Wrong answer. Try another.");
			if (index < optionButtons.Count)
				optionButtons[index].interactable = false;
		}
	}

	IEnumerator OpenDoor()
	{
		isOpen = true;
		heldBody = null;
		if (blockingCollider != null)
			blockingCollider.enabled = false;

		Quaternion start = doorPivot != null ? doorPivot.localRotation : Quaternion.identity;
		Quaternion end = start * Quaternion.Euler(0f, openAngle, 0f);
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * openSpeed;
			if (doorPivot != null)
				doorPivot.localRotation = Quaternion.Slerp(start, end, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t)));
			yield return null;
		}

		HideQuiz();
	}

	void EnsureCanvas()
	{
		if (canvasRoot != null)
			return;

		EnsureEventSystem();
		useTmp = TMP_Settings.defaultFontAsset != null;

		canvasRoot = new GameObject("QuizCanvas");
		canvasRoot.transform.SetParent(transform, false);

		Canvas canvas = canvasRoot.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		Camera view = Camera.main;
		if (view == null && Camera.allCamerasCount > 0)
			view = Camera.allCameras[0];
		canvas.worldCamera = view;

		RectTransform canvasRect = canvasRoot.GetComponent<RectTransform>();
		canvasRect.sizeDelta = new Vector2(900f, 700f);
		canvasRoot.transform.localScale = Vector3.one * 0.0022f;

		canvasRoot.AddComponent<GraphicRaycaster>();
		Type trackedRaycaster = Type.GetType("UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster, Unity.XR.Interaction.Toolkit");
		if (trackedRaycaster != null)
			canvasRoot.AddComponent(trackedRaycaster);

		GameObject panel = new GameObject("Panel");
		panel.transform.SetParent(canvasRoot.transform, false);
		Image panelImage = panel.AddComponent<Image>();
		panelImage.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);
		RectTransform panelRect = panel.GetComponent<RectTransform>();
		Stretch(panelRect);

		GameObject questionGo = CreateTextObject("Question", panel.transform, out questionLabel, out questionLabelTmp, 42, Color.white, TextAnchor.UpperCenter);
		RectTransform questionRect = questionGo.GetComponent<RectTransform>();
		questionRect.anchorMin = new Vector2(0f, 1f);
		questionRect.anchorMax = new Vector2(1f, 1f);
		questionRect.pivot = new Vector2(0.5f, 1f);
		questionRect.sizeDelta = new Vector2(-48f, 180f);
		questionRect.anchoredPosition = new Vector2(0f, -24f);

		GameObject statusGo = CreateTextObject("Status", panel.transform, out statusLabel, out statusLabelTmp, 28, new Color(1f, 0.85f, 0.4f), TextAnchor.UpperCenter);
		RectTransform statusRect = statusGo.GetComponent<RectTransform>();
		statusRect.anchorMin = new Vector2(0f, 1f);
		statusRect.anchorMax = new Vector2(1f, 1f);
		statusRect.pivot = new Vector2(0.5f, 1f);
		statusRect.sizeDelta = new Vector2(-48f, 80f);
		statusRect.anchoredPosition = new Vector2(0f, -210f);

		GameObject column = new GameObject("Options");
		column.transform.SetParent(panel.transform, false);
		buttonColumn = column.AddComponent<RectTransform>();
		buttonColumn.anchorMin = new Vector2(0f, 0f);
		buttonColumn.anchorMax = new Vector2(1f, 1f);
		buttonColumn.offsetMin = new Vector2(36f, 24f);
		buttonColumn.offsetMax = new Vector2(-36f, -300f);
		VerticalLayoutGroup layout = column.AddComponent<VerticalLayoutGroup>();
		layout.spacing = 12f;
		layout.childAlignment = TextAnchor.UpperCenter;
		layout.childControlWidth = true;
		layout.childControlHeight = true;
		layout.childForceExpandWidth = true;
		layout.childForceExpandHeight = false;
	}

	void FaceCanvasAt(Vector3 worldPoint)
	{
		Vector3 flat = worldPoint - transform.position;
		flat.y = 0f;
		Vector3 origin = transform.TransformPoint(canvasOffset);
		if (flat.sqrMagnitude > 0.01f)
			origin += flat.normalized * 1.5f;

		canvasRoot.transform.position = origin;
		Vector3 lookTarget = worldPoint;
		lookTarget.y = origin.y;
		Vector3 away = origin - lookTarget;
		if (away.sqrMagnitude > 0.001f)
			canvasRoot.transform.rotation = Quaternion.LookRotation(away);
	}

	Button CreateButton(string label)
	{
		GameObject buttonGo = new GameObject("Option");
		buttonGo.transform.SetParent(buttonColumn, false);
		Image image = buttonGo.AddComponent<Image>();
		image.color = new Color(0.16f, 0.38f, 0.58f, 1f);
		Button button = buttonGo.AddComponent<Button>();
		button.targetGraphic = image;
		ColorBlock colors = button.colors;
		colors.highlightedColor = new Color(0.28f, 0.55f, 0.78f, 1f);
		colors.pressedColor = new Color(0.12f, 0.65f, 0.4f, 1f);
		colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.7f);
		button.colors = colors;

		LayoutElement element = buttonGo.AddComponent<LayoutElement>();
		element.preferredHeight = 64f;
		element.minHeight = 64f;

		CreateTextObject("Label", buttonGo.transform, out Text legacy, out TextMeshProUGUI tmp, 30, Color.white, TextAnchor.MiddleCenter);
		Stretch(legacy != null ? legacy.rectTransform : tmp.rectTransform);
		if (tmp != null)
			tmp.text = label;
		else
			legacy.text = label;

		return button;
	}

	GameObject CreateTextObject(string name, Transform parent, out Text legacy, out TextMeshProUGUI tmp, float size, Color color, TextAnchor anchor)
	{
		GameObject go = new GameObject(name);
		go.transform.SetParent(parent, false);
		go.AddComponent<RectTransform>();
		legacy = null;
		tmp = null;

		if (useTmp)
		{
			tmp = go.AddComponent<TextMeshProUGUI>();
			tmp.font = TMP_Settings.defaultFontAsset;
			tmp.fontSize = size;
			tmp.color = color;
			tmp.alignment = ToTmp(anchor);
			tmp.enableWordWrapping = true;
			tmp.raycastTarget = false;
		}
		else
		{
			legacy = go.AddComponent<Text>();
			legacy.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
			legacy.fontSize = Mathf.RoundToInt(size);
			legacy.color = color;
			legacy.alignment = anchor;
			legacy.horizontalOverflow = HorizontalWrapMode.Wrap;
			legacy.verticalOverflow = VerticalWrapMode.Overflow;
			legacy.raycastTarget = false;
		}

		return go;
	}

	void SetQuestion(string value)
	{
		if (questionLabelTmp != null)
			questionLabelTmp.text = value;
		else if (questionLabel != null)
			questionLabel.text = value;
	}

	void SetStatus(string value)
	{
		if (statusLabelTmp != null)
			statusLabelTmp.text = value;
		else if (statusLabel != null)
			statusLabel.text = value;
	}

	void ClearButtons()
	{
		foreach (Button button in optionButtons)
		{
			if (button != null)
				Destroy(button.gameObject);
		}
		optionButtons.Clear();
	}

	static void Stretch(RectTransform rect)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	static TextAlignmentOptions ToTmp(TextAnchor anchor)
	{
		switch (anchor)
		{
			case TextAnchor.UpperCenter:
				return TextAlignmentOptions.Top;
			case TextAnchor.MiddleCenter:
				return TextAlignmentOptions.Center;
			default:
				return TextAlignmentOptions.Center;
		}
	}

	static void Tint(Renderer target, Color color)
	{
		if (target == null)
			return;

		Shader shader = Shader.Find("Universal Render Pipeline/Lit");
		if (shader == null)
			shader = Shader.Find("Standard");
		if (shader == null)
			return;

		Material material = new Material(shader);
		if (material.HasProperty("_BaseColor"))
			material.SetColor("_BaseColor", color);
		if (material.HasProperty("_Color"))
			material.SetColor("_Color", color);
		target.material = material;
	}

	static void EnsureEventSystem()
	{
		if (FindAnyObjectByType<EventSystem>() != null)
			return;

		GameObject eventSystem = new GameObject("EventSystem");
		eventSystem.AddComponent<EventSystem>();
		eventSystem.AddComponent<InputSystemUIInputModule>();
	}

	void OnDrawGizmosSelected()
	{
		BoxCollider box = GetComponent<BoxCollider>();
		if (box == null || !box.isTrigger)
			return;

		Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawWireCube(box.center, box.size);
	}
}

public class QuizDoorContactRelay : MonoBehaviour
{
	public QuizDoor door;

	void OnCollisionEnter(Collision collision)
	{
		if (door != null)
			door.HandleEnter(collision.collider);
	}

	void OnCollisionExit(Collision collision)
	{
		if (door != null)
			door.HandleExit(collision.collider);
	}
}
