using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Timed buff or debuff from a closed poll. One effect at a time.
/// Poll titles arrive as !vote fullsail, nimble, ghost, magnet, anchor,
/// drunkwheel, enginecut, crosswind, or barrelroll.
/// </summary>
public class BoatVoteEffect : MonoBehaviour
{
	public float fullSailMultiplier = 1.75f;
	public float nimbleMultiplier = 1.6f;
	public float anchorMultiplier = 0.35f;
	public float magnetRadius = 20f;
	public float magnetForce = 18f;
	public float crosswindForce = 8f;

	public bool InvertSteering { get; private set; }
	public bool BlocksPowerOn { get; private set; }

	BoatController boat;
	GenerateLevel level;
	Rigidbody hull;
	float baseAccel;
	float baseTurn;
	bool capturedBase;
	string activeLabel;
	float timeLeft;
	bool ghost;
	bool magnet;
	bool crosswind;
	readonly List<Collider> boatColliders = new();
	readonly List<Collider> ignoredRocks = new();

	void Awake()
	{
		boat = GetComponent<BoatController>();
		level = FindAnyObjectByType<GenerateLevel>();
		ResolveHull();
	}

	void ResolveHull()
	{
		if (boat == null)
			boat = GetComponent<BoatController>();
		if (hull != null)
			return;

		if (boat != null && boat.rb != null)
			hull = boat.rb;
		else if (boat != null)
			hull = boat.GetComponent<Rigidbody>();
		else
			hull = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		ResolveHull();
		if (hull == null)
			return;

		if (magnet)
		{
			if (level == null)
				level = FindAnyObjectByType<GenerateLevel>();
			if (level != null && level.spawned != null)
				PullCubes();
		}

		if (crosswind)
			hull.AddForce(hull.transform.right * crosswindForce);
	}

	void LateUpdate()
	{
		if (timeLeft > 0f)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f)
				EndEffect();
			else
				PushHud();
		}

		if (BlocksPowerOn)
			ForcePowerOff();
	}

	public bool Apply(string voteId)
	{
		ResolveHull();
		switch (voteId)
		{
			case "fullsail":
				Begin("Full Sail", 20f, fullSailMultiplier, 0f, false, false, false, false);
				return true;
			case "nimble":
				Begin("Nimble", 20f, 0f, nimbleMultiplier, false, false, false, false);
				return true;
			case "ghost":
				Begin("Ghost", 12f, 0f, 0f, true, false, false, false);
				return true;
			case "magnet":
				Begin("Magnet", 15f, 0f, 0f, false, true, false, false);
				return true;
			case "anchor":
				Begin("Anchor", 20f, anchorMultiplier, 0f, false, false, false, false);
				return true;
			case "drunkwheel":
				Begin("Drunk Wheel", 15f, 0f, 0f, false, false, true, false);
				return true;
			case "enginecut":
				Begin("Engine Cut", 8f, 0f, 0f, false, false, false, true);
				return true;
			case "crosswind":
				Begin("Crosswind", 12f, 0f, 0f, false, false, false, false, true);
				return true;
			case "barrelroll":
				EndEffect();
				if (hull != null)
					hull.transform.Rotate(Vector3.forward, 180f, Space.Self);
				activeLabel = "Barrel Roll";
				timeLeft = 3f;
				PushHud();
				return true;
			default:
				Debug.LogWarning("[BoatVoteEffect] Unknown vote '" + voteId + "'.");
				return false;
		}
	}

	void Begin(string label, float duration, float accelMultiplier, float turnMultiplier, bool useGhost, bool useMagnet, bool invertWheel, bool cutEngine, bool useCrosswind = false)
	{
		EndEffect();
		CaptureBase();

		activeLabel = label;
		timeLeft = duration;
		if (boat != null && accelMultiplier > 0f)
			boat.maxaccel = baseAccel * accelMultiplier;
		if (boat != null && turnMultiplier > 0f)
			boat.maxturn = baseTurn * turnMultiplier;

		ghost = useGhost;
		magnet = useMagnet;
		InvertSteering = invertWheel;
		BlocksPowerOn = cutEngine;
		crosswind = useCrosswind;

		if (ghost)
			SetGhost(true);
		if (BlocksPowerOn)
			ForcePowerOff();

		Debug.Log("[BoatVoteEffect] " + label + " for " + duration + "s");
		PushHud();
	}

	void EndEffect()
	{
		if (ghost)
			SetGhost(false);

		ghost = false;
		magnet = false;
		crosswind = false;
		InvertSteering = false;
		BlocksPowerOn = false;
		activeLabel = null;
		timeLeft = 0f;
		RestoreStats();
		PushHud();
	}

	void CaptureBase()
	{
		if (capturedBase || boat == null)
			return;

		baseAccel = boat.maxaccel;
		baseTurn = boat.maxturn;
		capturedBase = true;
	}

	void RestoreStats()
	{
		if (!capturedBase || boat == null)
			return;

		boat.maxaccel = baseAccel;
		boat.maxturn = baseTurn;
	}

	void ForcePowerOff()
	{
		if (boat == null)
			return;

		boat.powerstatus = false;
		if (boat.onlever != null)
			boat.onlever.value = false;
	}

	void PullCubes()
	{
		Vector3 origin = hull.position;
		float radiusSqr = magnetRadius * magnetRadius;
		foreach (GameObject obj in level.spawned)
		{
			if (obj == null || !obj.name.Contains("CubePoints"))
				continue;

			Vector3 toBoat = origin - obj.transform.position;
			if (toBoat.sqrMagnitude > radiusSqr || toBoat.sqrMagnitude < 0.25f)
				continue;

			Rigidbody cubeBody = obj.GetComponent<Rigidbody>();
			if (cubeBody != null)
				cubeBody.AddForce(toBoat.normalized * magnetForce, ForceMode.Acceleration);
		}
	}

	void SetGhost(bool enabled)
	{
		if (!enabled)
		{
			for (int i = 0; i < ignoredRocks.Count; i++)
			{
				Collider rock = ignoredRocks[i];
				if (rock == null)
					continue;
				for (int b = 0; b < boatColliders.Count; b++)
				{
					if (boatColliders[b] != null)
						Physics.IgnoreCollision(boatColliders[b], rock, false);
				}
			}
			ignoredRocks.Clear();
			return;
		}

		if (hull == null || level == null || level.spawned == null)
			return;

		boatColliders.Clear();
		Collider[] found = hull.GetComponentsInChildren<Collider>();
		foreach (Collider col in found)
		{
			if (col != null && !col.isTrigger)
				boatColliders.Add(col);
		}

		foreach (GameObject obj in level.spawned)
		{
			if (obj == null || obj.name.Contains("CubePoints"))
				continue;

			Collider[] rockCols = obj.GetComponentsInChildren<Collider>();
			foreach (Collider rock in rockCols)
			{
				if (rock == null || rock.isTrigger)
					continue;

				foreach (Collider boatCol in boatColliders)
					Physics.IgnoreCollision(boatCol, rock, true);
				ignoredRocks.Add(rock);
			}
		}
	}

	void PushHud()
	{
		if (level == null)
			level = FindAnyObjectByType<GenerateLevel>();
		if (level == null)
			return;

		level.SetVoteStatus(activeLabel, timeLeft);
	}
}
