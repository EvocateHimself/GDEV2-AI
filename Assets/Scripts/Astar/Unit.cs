﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;
	bool followingPath = true;


	[Header("Movement")]
	public Transform player;
	private float currentSpeed;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDst = 5;
	public float stoppingDst = 10;

	public Transform spawnpoint;
	public float spawnCooldown = 5f;
	public Transform[] checkpoints;
	public int checkpointCounter = 0;
	private float waitTime;
	public float checkpointWaitTime = 3.0f;

	[Header("Health")]
	public Image healthBar;
	public float currentHealth;
	private float currentHealthValue;
	public float maxHealth = 100f;
	public float lerpSpeed = 10f;

	public float CurrentHealth {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

	[Header("Attack")]
	public Transform firePoint;
	public GameObject fireBallPrefab;
	private float fireBallInterval;
	public float startTimeFireBallInterval = 1f;
	public GameObject lavaPrefab;
	public float lavaIgniteRadius = 5f;
	public float lavaSpreadTime = 1f;
	public float lavaMaxSpread = 15f;
	private float lavaInterval;
	public float startTimeLavaInterval = 5f;

	public float lookRadius = 10f;
	public float attackRadius = 5f;

	public Image statusImage;

	public List<GameObject> shields = new List<GameObject>();

	[Task]
	public bool playerInRange = false;

	[Task]
	public bool attackPlayer = false;

	[Task]
	public bool defendAllies = false;

	CreatePath path;

	private void Start() {
		currentSpeed = speed;
		waitTime = checkpointWaitTime;
		currentHealth = maxHealth;
		fireBallInterval = startTimeFireBallInterval;
		lavaInterval = startTimeLavaInterval;
		foreach(GameObject shield in GameObject.FindGameObjectsWithTag("Shield")) {
            shields.Add(shield);
            shield.SetActive(false);
        }
        //randomSpot = Random.Range(0, moveSpots.Length);
	}

	private void Update() {
		// If inside the lookRadius
		float playerDistance = Vector3.Distance(player.position, transform.position);

		if (playerDistance <= lookRadius) {
			playerInRange = true;
			statusImage.color = new Color32(255,255,0,255);
			if (playerDistance <= attackRadius) {
				// Attack
				attackPlayer = true;
				statusImage.color = new Color32(255,0,0,255);
			}
			else {
				attackPlayer = false;
				statusImage.color = new Color32(255,255,0,255);
			}
		} 
		else {
			playerInRange = false;
			statusImage.color = new Color32(0,255,0,255);
		}

		// Die
		if (currentHealth <= 0) {
			Destroy(gameObject, .2f);

		}

		currentHealthValue = Map(CurrentHealth, 0, maxHealth, 0, 1);
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealthValue, Time.deltaTime * lerpSpeed);
	}

	public void OnPathFound(Vector3[] waypoints, bool pathSuccessful) {
		if (pathSuccessful) {
			path = new CreatePath(waypoints, transform.position, turnDst, stoppingDst);

			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	[Task]
	private void MoveToPlayer() {
		UpdatePath(player);
		Task.current.Succeed();
	}

	[Task]
	private void MoveToCheckpoint() {
		// Move to checkpoint
		UpdatePath(checkpoints[checkpointCounter]);

		// Distance to the checkpoint
		float patrolDistance = Vector3.Distance(checkpoints[checkpointCounter].position, transform.position);

		// If distance is between unit and checkpoint destination is smaller or equal to destination, the unit has reached its destination
		if (patrolDistance <= stoppingDst) {
			Task.current.Succeed();
		}
	}

	[Task]
	private void FindNextCheckpoint() {
		if (checkpointCounter < checkpoints.Length - 1) {
			checkpointCounter++;
		}
		else {
			checkpointCounter = 0;
		}
		Task.current.Succeed();
	}

	[Task]
	private void ShootFireball() {
		if (fireBallInterval <= 0) {
			var bullet = Instantiate(fireBallPrefab, firePoint.transform.position, Quaternion.identity);
            bullet.transform.position = firePoint.transform.position;
            bullet.transform.rotation = firePoint.transform.rotation;

			fireBallInterval = startTimeFireBallInterval;
		} else {
			fireBallInterval -= Time.deltaTime;
		}
		Task.current.Succeed();
	}

	[Task]
	private void CreateLavaPit() {
		if (lavaInterval <= 0) {
			StartCoroutine(SizeLavaPit());
			lavaInterval = startTimeLavaInterval;
		} else {
			lavaInterval -= Time.deltaTime;
		}
		Task.current.Succeed();
	}

	private IEnumerator SizeLavaPit() {
		Vector3 pos = new Vector3(Random.Range(-lavaIgniteRadius, lavaIgniteRadius), -0.4f, Random.Range(-lavaIgniteRadius, lavaIgniteRadius));
		var lava = Instantiate (lavaPrefab, pos, Quaternion.identity);

		Vector3 beginScale = new Vector3(0, 0.43f, 0);

		lava.transform.localScale = beginScale;

		while (lava.transform.localScale.z < lavaMaxSpread) {
			lava.transform.localScale += new Vector3(1f, 0, 1f) / lavaSpreadTime * Time.deltaTime;
			//lava.transform.rotation = Quaternion.Lerp(crop.transform.rotation, randomRotation, processingTime * Time.deltaTime);

			yield return new WaitForEndOfFrame();
		}

		Destroy(lava, 120f);
	}

	[Task]
	private void ActivateShield() {
        foreach(GameObject shield in shields) {
            shield.SetActive(false);
        }
		Task.current.Succeed();
	}

	[Task]
	private void DeactivateShield() {
        foreach(GameObject shield in shields) {
            shield.SetActive(false);
        }
		Task.current.Succeed();
	}
	
	private void UpdatePath(Transform _target) {
		PathRequestManager.RequestPath (new PathRequest(transform.position, _target.position, OnPathFound));
	}

	IEnumerator FollowPath() {

		int pathIndex = 0;
		transform.LookAt (path.lookPoints [0]);

		float speedPercent = 1;

		while (followingPath) {
			Vector2 pos2D = new Vector2 (transform.position.x, transform.position.z);
			while (path.turnBoundaries [pathIndex].HasCrossedLine (pos2D)) {
				if (pathIndex == path.finishLineIndex) {
					followingPath = false;
					break;
				} else {
					pathIndex++;
				}
			}

			if (followingPath) {

				if (pathIndex >= path.slowDownIndex && stoppingDst > 0) {
					speedPercent = Mathf.Clamp01 (path.turnBoundaries [path.finishLineIndex].DistanceFromPoint (pos2D) / stoppingDst);
					if (speedPercent < 0.01f) {
						followingPath = false;
					}
				}

				Quaternion targetRotation = Quaternion.LookRotation (path.lookPoints [pathIndex] - transform.position);
				transform.rotation = Quaternion.Lerp (transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
				transform.Translate (Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
			}

			yield return null;

		}
	}

	// Take damage when hit
	private void OnTriggerEnter(Collider other) {
		if(other.CompareTag("Bullet")) {
			CurrentHealth -= player.GetComponent<Shooting>().damage;
			print(player.GetComponent<Shooting>().damage);
		}
	}

	// Heal enemy when in touch with the lava
	private void OnTriggerStay(Collider other) {
		if(other.CompareTag("Lava")) {
			CurrentHealth += 2f;
		}
	}

	private void OnDrawGizmos() {
		if (path != null) {
			path.DrawWithGizmos ();
		}
	}

    // Create Gizmos around gameObject in the inspector
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
		Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lavaIgniteRadius);
    }

	// This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
