using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

    public int gridX;
    public int gridY;

    public bool isWall;
    public Vector3 pos;

    public Node parent;

    public int gCost; // moving to the next square
    public int hCost; // distance to the goal

    public int FCost { get { return gCost + hCost; } } // get function that adds gCost and hCost

    public Node(bool _isWall, Vector3 _pos, int _gridX, int _gridY) {
        isWall = _isWall;
        pos = _pos;
        gridX = _gridX;
        gridY = _gridY;
    }
}