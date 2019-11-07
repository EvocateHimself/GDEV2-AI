using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour{

    protected Grid grid;
    public Transform startPos;
    public Transform targetPos;

    public bool isCalculatingPath;
    public bool canCalculate;

    protected float nextPathUpdateTime = 0.0f;
    public float pathUpdatePeriod = 0.2f;


    private void Awake() {
        canCalculate = true;
        isCalculatingPath = false;
        grid = GetComponent<Grid>();
        nextPathUpdateTime = Random.Range(0.0f, pathUpdatePeriod);
    }


    private void Update() {
        if (Time.time > nextPathUpdateTime && canCalculate) {
            isCalculatingPath = true;
            nextPathUpdateTime += pathUpdatePeriod;
            FindPath(startPos.position, targetPos.position);
        } else {
            isCalculatingPath = false;
        }
            
    }


    private void FindPath(Vector3 _startPos, Vector3 _targetPos) {
        Node startNode = grid.NodeFromWorldPosition(_startPos);
        Node targetNode = grid.NodeFromWorldPosition(_targetPos);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);
        
        while(openList.Count > 0) {
            Node currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++) {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].hCost < currentNode.hCost) {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == targetNode) {
                GetFinalPath(startNode, targetNode);
                break;
            }

            foreach(Node neighbourNode in grid.GetNeighBourNodes(currentNode)) {
                if (!neighbourNode.isWall || closedList.Contains(neighbourNode)) {
                    continue;
                }

                int moveCost = currentNode.gCost + GetManhattenDistance(currentNode, neighbourNode);

                if (moveCost < neighbourNode.gCost || !openList.Contains(neighbourNode)) {
                    neighbourNode.gCost = moveCost;
                    neighbourNode.hCost = GetManhattenDistance(neighbourNode, targetNode);
                    neighbourNode.parent = currentNode;

                    if (!openList.Contains(neighbourNode)) {
                        openList.Add(neighbourNode);
                    }
                }
            }

        }
    }


    private void GetFinalPath(Node _startingNode, Node _endNode) {
        List<Node> finalPath = new List<Node>();
        Node currentNode = _endNode;

        while(currentNode != _startingNode) {
            finalPath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        finalPath.Reverse();

        grid.finalPath = finalPath;
    }


    private int GetManhattenDistance(Node _nodeA, Node _nodeB) {
        int ix = Mathf.Abs(_nodeA.gridX - _nodeB.gridX);
        int iy = Mathf.Abs(_nodeA.gridY - _nodeB.gridY);

        return ix + iy;
    }
}