using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

    public Transform startPos;
    public LayerMask wallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float distance;

    public Node[,] grid;
    public List<Node> finalPath;

    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;


    private void Start() {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }


    private void CreateGrid() {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = Vector3.Scale(transform.position, Vector3.up) - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {

                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool wall = true;

                if (Physics.CheckSphere(worldPoint, nodeRadius, wallMask)) {
                    wall = false;
                }

                grid[x, y] = new Node(wall, worldPoint, x, y);
            }

        }
    }


    public Node NodeFromWorldPosition(Vector3 _worldPosition) {
        float xPoint = ((_worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float yPoint = ((_worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y);

        xPoint = Mathf.Clamp01(xPoint);
        yPoint = Mathf.Clamp01(yPoint);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xPoint);
        int y = Mathf.RoundToInt((gridSizeY - 1) * yPoint);

        return grid[x, y];
    }


    public List<Node> GetNeighBourNodes(Node _node) {
        List<Node> neighbourNodes = new List<Node>();
        neighbourNodes.Add(GetANeighbourNode(_node.gridX + 1, _node.gridY)); // right side
        neighbourNodes.Add(GetANeighbourNode(_node.gridX - 1, _node.gridY)); // left side
        neighbourNodes.Add(GetANeighbourNode(_node.gridX, _node.gridY + 1)); // top side
        neighbourNodes.Add(GetANeighbourNode(_node.gridX, _node.gridY - 1)); // bottom side
        neighbourNodes.Add(GetANeighbourNode(_node.gridX + 1, _node.gridY + 1)); // right top
        neighbourNodes.Add(GetANeighbourNode(_node.gridX - 1, _node.gridY - 1)); // left bottom
        neighbourNodes.Add(GetANeighbourNode(_node.gridX - 1, _node.gridY + 1)); // right bottom
        neighbourNodes.Add(GetANeighbourNode(_node.gridX + 1, _node.gridY - 1)); // left top
        return neighbourNodes;
    }

    private Node GetANeighbourNode(int xCheckPos, int yCheckPos) {
        if (xCheckPos >= 0 && xCheckPos < gridSizeX) {
            if (yCheckPos >= 0 && yCheckPos < gridSizeY) {
                return grid[xCheckPos, yCheckPos];
            }
        }
        return null;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null) {
            foreach (Node node in grid) {
                if (node.isWall) {
                    Gizmos.color = Color.white;
                } else {
                    Gizmos.color = Color.yellow;
                }

                if (finalPath != null) {
                    if (finalPath.Contains(node)) {//If the current node is in the final path
                        Gizmos.color = Color.red;//Set the color of that node
                        Gizmos.DrawCube(node.pos, Vector3.one * (nodeDiameter - distance));
                    }
                }
            }
        }

    }
}