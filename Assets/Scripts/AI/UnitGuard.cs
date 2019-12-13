using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using UnityEngine.UI;

public class UnitGuard : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;
	bool followingPath = true;

	[Header("Movement")]
	[SerializeField] private Transform player;
	private float currentSpeed;
	[SerializeField] private float speed = 8;
	[SerializeField] private float turnSpeed = 3;
	[SerializeField] private float turnDst = 5;
	[SerializeField] private float stoppingDst = 10;
	[SerializeField] private Transform[] checkpoints;
	[SerializeField] private int checkpointCounter = 0;
	private float waitTime;
	[SerializeField] private float checkpointWaitTime = 3.0f;

	[Header("Health")]
	[SerializeField] private Image healthBar;
	[SerializeField] private float currentHealth;
	private float currentHealthValue;
	[SerializeField] private float maxHealth = 200f;
	[SerializeField] private float lerpSpeed = 10f;
	[SerializeField] private float healByLavaValue = 2f;

	public float CurrentHealth {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

	[SerializeField] private TempleHealth templeHealth;
	[SerializeField] private GameObject shield;

	[Header("Attack")]
	[SerializeField] private Transform firePoint;
	[SerializeField] private GameObject fireBallPrefab;
	[SerializeField] private AudioSource fireBallSound;
	[SerializeField] private AudioSource takeHitSound;
	[SerializeField] private AudioSource dieSound;
	public float fireBallDamage = 10f;
	private float fireBallInterval;
	[SerializeField] private float startTimeFireBallInterval = 2f;
	[SerializeField] private float lookRadius = 25f;
	[SerializeField] private float attackRadius = 20f;

	[SerializeField] private Image statusImage;
	[SerializeField] private Sprite attackSprite;
	private Sprite defendSprite;
	private bool isDead = false;

	[Task]
	private bool playerInRange = false;

	[Task]
	private bool attackPlayer = false;

	[Task]
	public bool templeAlive = true;

	[Task]
	public bool isHealing = false;

	CreatePath path;

	private void Start() {
		currentSpeed = speed;
		waitTime = checkpointWaitTime;
		currentHealth = maxHealth;
		fireBallInterval = startTimeFireBallInterval;
		defendSprite = statusImage.sprite;
	}

	private void Update() {
		// If inside the lookRadius
		float playerDistance = Vector3.Distance(player.position, transform.position);

		if (playerDistance <= lookRadius) {
			playerInRange = true;
			statusImage.sprite = attackSprite;

			if (playerDistance <= attackRadius) {
				// Attack
				attackPlayer = true;
			}
			else {
				attackPlayer = false;
			}
		}
		else {
			playerInRange = false;
			statusImage.sprite = defendSprite;
		}

		if (templeHealth.CurrentHealth <= 0) {
			shield.SetActive(false);
			templeAlive = false;
		}

		// Die
		if (currentHealth <= 0 && !isDead) {
			dieSound.Play();
			isDead = true;
			Destroy(gameObject, .5f);

		}

		// Mapping the health bar
		currentHealthValue = Map(CurrentHealth, 0, maxHealth, 0, 1);
        healthBar.fillAmount = Mathf.Lerp(healthBar.fillAmount, currentHealthValue, Time.deltaTime * lerpSpeed);
	}

	// If the path has been found, follow the path
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
			fireBallSound.Play();

			fireBallInterval = startTimeFireBallInterval;
		} else {
			fireBallInterval -= Time.deltaTime;
		}
		Task.current.Succeed();
	}

	[Task]
	private void HealAI() {
		if (CurrentHealth < maxHealth) {
			CurrentHealth += healByLavaValue;
		}
		Task.current.Succeed();
	}
	
	// Update path to target position
	private void UpdatePath(Transform _target) {
		PathRequestManager.RequestPath (new PathRequest(transform.position, _target.position, OnPathFound));
	}

	// Function that calculates the path and follows it
	IEnumerator FollowPath() {

		int pathIndex = 0;

		// Smooth lookAt
		if (path.lookPoints.Length > 1) {
			transform.LookAt (path.lookPoints[1]);
		}
		else {
			transform.LookAt (path.lookPoints[0]);
		}

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
			takeHitSound.Play();

			if (!templeAlive) { 
				CurrentHealth -= player.GetComponent<Shooting>().damage;
			}
		}
	}

	// Heal enemy when in touch with the lava
	private void OnTriggerStay(Collider other) {
		if(other.CompareTag("Lava")) {
			isHealing = true;
		}
	}

	private void OnTriggerExit(Collider other) {
		if(other.CompareTag("Lava")) {
			isHealing = false;
		}
	}

	private void OnDrawGizmos() {
		if (path != null) {
			path.DrawWithGizmos ();
		}
	}

    // Create Gizmos around gameObject in the inspector for the look and attack radius
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
		Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

	// This method maps a range of numbers into another range
    public float Map(float x, float in_min, float in_max, float out_min, float out_max) {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}
