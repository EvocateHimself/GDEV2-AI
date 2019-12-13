using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UnitBoss : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;
	bool followingPath = true;

	[Header("Movement")]
	[SerializeField] private Transform player;
	private float currentSpeed;
	[SerializeField] private float speed = 20;
	[SerializeField] private float turnSpeed = 3;
	[SerializeField] private float turnDst = 5;
	[SerializeField] private float stoppingDst = 10;

	[Header("Health")]
	[SerializeField] private Image healthBar;
	[SerializeField] private float currentHealth;
	private float currentHealthValue;
	[SerializeField] private float maxHealth = 600f;
	[SerializeField] private float lerpSpeed = 10f;

	public float CurrentHealth {
        get { return currentHealth; }
        set { currentHealth = Mathf.Clamp(value, 0, maxHealth); }
    }

	[SerializeField] private GameObject shield;

	[Header("Attack")]
	[SerializeField] private Transform firePoint;
	[SerializeField] private GameObject fireBallPrefab;
	[SerializeField] private AudioSource fireBallSound;
	[SerializeField] private AudioSource lavaPitSound;
	[SerializeField] private AudioSource takeHitSound;
	[SerializeField] private AudioSource dieSound;
	public float megaFireBallDamage = 10f;
	public float lavaDamage = 10f;
	private float fireBallInterval;
	[SerializeField] private float startTimeFireBallInterval = 1f;
	[SerializeField] private GameObject lavaPrefab;
	[SerializeField] private float lavaIgniteRadius = 5f;
	[SerializeField] private float lavaSpreadTime = 1f;
	[SerializeField] private float lavaMaxSpread = 15f;
	private float lavaInterval;
	[SerializeField] private float startTimeLavaInterval = 5f;

	[SerializeField] private float lookRadius = 10f;
	[SerializeField] private float attackRadius = 5f;

	[SerializeField] private Image statusImage;
	[SerializeField] private Sprite attackSprite;
	private Sprite defendSprite;
	private bool isDead = false;
	private bool shieldAlive = true;
	private int index;

	public TempleHealth[] templeHealths;

	[Task]
	private bool playerInRange = false;

	[Task]
	private bool attackPlayer = false;

	[Task]
	private bool defendAllies = false;

	CreatePath path;

	private void Start() {
		currentSpeed = speed;
		currentHealth = maxHealth;
		fireBallInterval = startTimeFireBallInterval;
		lavaInterval = startTimeLavaInterval;
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

		// Check if all temples are destroyed - for loop doesn't seem to be working so doing it manually
		if (templeHealths[0].CurrentHealth <= 0 
		&& templeHealths[1].CurrentHealth <= 0 
		&& templeHealths[2].CurrentHealth <= 0 
		&& templeHealths[3].CurrentHealth <= 0 
		&& templeHealths[4].CurrentHealth <= 0 
		&& templeHealths[5].CurrentHealth <= 0 
		&& templeHealths[6].CurrentHealth <= 0) {
			shield.SetActive(false); // Deactivate the shield
			shieldAlive = false;
		}

		// Die
		if (currentHealth <= 0 && !isDead) {
			dieSound.Play();
			isDead = true;
        	SceneManager.LoadScene("Win");
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
	private void CreateLavaPit() {
		if (lavaInterval <= 0) {
			StartCoroutine(SizeLavaPit());
			lavaInterval = startTimeLavaInterval;
		} else {
			lavaInterval -= Time.deltaTime;
		}
		Task.current.Succeed();
	}

	// Increase the size of the lava pit over time
	private IEnumerator SizeLavaPit() {
		Vector3 pos = new Vector3(Random.Range(-lavaIgniteRadius, lavaIgniteRadius), -0.4f, Random.Range(-lavaIgniteRadius, lavaIgniteRadius));
		var lava = Instantiate (lavaPrefab, pos, Quaternion.identity);
		lavaPitSound.Play();

		Vector3 beginScale = new Vector3(0, 0.43f, 0);

		lava.transform.localScale = beginScale;

		while (lava.transform.localScale.z < lavaMaxSpread) {
			lava.transform.localScale += new Vector3(1f, 0, 1f) / lavaSpreadTime * Time.deltaTime;

			yield return new WaitForEndOfFrame();
		}

		Destroy(lava, 120f);
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

			if (!shieldAlive) { 
				CurrentHealth -= player.GetComponent<Shooting>().damage;
			}
		}
	}

	// Create gizmos for the waypoints
	private void OnDrawGizmos() {
		if (path != null) {
			path.DrawWithGizmos ();
		}
	}

    // Create Gizmos around gameObject in the inspector for the look, attack and lava ignite radius
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
