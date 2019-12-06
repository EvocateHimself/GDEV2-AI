using UnityEngine;
using System.Collections;
using Panda;

public class Unit : MonoBehaviour {

	const float minPathUpdateTime = .2f;
	const float pathUpdateMoveThreshold = .5f;

	public Transform player;
	public Transform firePoint;
	private float currentSpeed;
	public float speed = 20;
	public float turnSpeed = 3;
	public float turnDst = 5;
	public float stoppingDst = 10;

	public float currentHealth;
	public float maxHealth = 100f;

	public float shootInterval;
	public float startTimeInterval;
	public GameObject fireBall;

	[Task]
	public bool playerInRange = false;

	[Task]
	public bool attackPlayer = false;

	public Transform[] checkpoints;
	public int checkpointCounter = 0;
	private float waitTime;
	public float checkpointWaitTime = 3.0f;

	public float lookRadius = 10f;
	public float attackRadius = 5f;

	CreatePath path;

	private void Start() {
		currentSpeed = speed;
		waitTime = checkpointWaitTime;
		currentHealth = maxHealth;
		shootInterval = startTimeInterval;
        //randomSpot = Random.Range(0, moveSpots.Length);
	}

	private void Update() {
		// If inside the lookRadius
		float playerDistance = Vector3.Distance(player.position, transform.position);

		if (playerDistance <= lookRadius) {
			playerInRange = true;
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
		}

		// Die
		if (currentHealth <= 0) {

		}


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
		if (shootInterval <= 0) {
			var bullet = Instantiate(fireBall, firePoint.transform.position, Quaternion.identity);
            bullet.transform.position = firePoint.transform.position;
            bullet.transform.rotation = firePoint.transform.rotation;

			shootInterval = startTimeInterval;
		} else {
			shootInterval -= Time.deltaTime;
		}
		Task.current.Succeed();
	}

	
	private void UpdatePath(Transform _target) {
		PathRequestManager.RequestPath (new PathRequest(transform.position, _target.position, OnPathFound));
	}

	IEnumerator FollowPath() {

		bool followingPath = true;
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

	public void OnDrawGizmos() {
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
    }
}
