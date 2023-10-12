using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Environment
{
    /// <summary>
    /// Class that handles nodes and gives characters paths to reach their target.
    /// </summary>
    public class GroundSystem : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance for the ground system.
        /// </summary>
        public static GroundSystem Instance { get; private set; }

        /// <summary>
        /// Layer on which the character cannot walk.
        /// </summary>
        [SerializeField] private LayerMask notWalkable;

        /// <summary>
        /// Size of the ground.
        /// </summary>
        [SerializeField] private Vector2 groundSize;

        /// <summary>
        /// Half size of the node.
        /// </summary>
        private const float NodeRadius = 0.2f;

        /// <summary>
        /// Size of the node grid.
        /// </summary>
        private Vector2Int _gridSize;

        /// <summary>
        /// 2D array to store nodes on the ground.
        /// </summary>
        private Node[,] _nodes;

        /// <summary>
        /// Source and destination nodes for the main character.
        /// </summary>
        private Node _sourceNode, _destinationNode;

        /// <summary>
        /// Path for the main character.
        /// </summary>
        private List<Node> _path;

        /// <summary>
        /// Directions in which the A* algorithm can traverse.
        /// </summary>
        private readonly Vector2Int[] _directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.up + Vector2Int.right,
            Vector2Int.up + Vector2Int.left,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down + Vector2Int.left
        };

        // Start is called before the first frame update
        void Start()
        {
            // Setup the singleton for the ground system.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // Initialize the environment.
            InitEnvironment();
        }

        /// <summary>
        /// Function to setup nodes for the ground.
        /// </summary>
        void InitEnvironment()
        {
            // Calculate the node diameter and grid size.
            const float nodeDiameter = NodeRadius * 2;
            _gridSize = Vector2Int.RoundToInt(groundSize / nodeDiameter);

            // Create a 2D array of nodes with the help of grid size calculated above.
            _nodes = new Node[_gridSize.x, _gridSize.y];

            // Populate all the node values in the grid.
            for (var x = 0; x < _gridSize.x; x++)
            {
                for (var y = 0; y < _gridSize.y; y++)
                {
                    var xCord = -groundSize.x / 2 + (NodeRadius + nodeDiameter * x);
                    var yCord = groundSize.y / 2 - (NodeRadius + nodeDiameter * y);
                    var worldPos = new Vector2(xCord, yCord);
                    var isWalkable = !(bool)Physics2D.OverlapCircle(worldPos, nodeDiameter, notWalkable)?.transform;

                    _nodes[x, y] = new Node(worldPos, isWalkable, new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// Function to get the nearest node to a given world co-ordinate.
        /// </summary>
        /// <param name="position">World co-ordinate for which the nearest node is to be found.</param>
        /// <returns>Nearest node to World co-ordinate.</returns>
        public Node GetNearestNode(Vector2 position)
        {
            // Set the distance to the nearest node to max value.
            var distanceToNearestNode = float.MaxValue;
            Node nearestNode = null;
            
            // Check with all nodes which is the nearest.
            foreach (var node in _nodes)
            {
                var currentDistance = Vector2.Distance(node.WorldPos, position);
                if (currentDistance < distanceToNearestNode)
                {
                    distanceToNearestNode = currentDistance;
                    nearestNode = node;
                }
            }

            // Return the nearest node.
            return nearestNode;
        }

        /// <summary>
        /// Function to find path for a character, with source and destination nodes.
        /// </summary>
        /// <param name="source">Node from which the character will start moving.</param>
        /// <param name="destination">Node at which the character will end moving.</param>
        /// <param name="isMainCharacter">Flag to show main character's path.</param>
        /// <returns></returns>
        public Stack<Node> GetPath(Node source, Node destination, bool isMainCharacter = false)
        {
            // Check if source and destination are the same, if yes then return.
            if (source.Equals(destination))
                return null;

            // Reset all nodes and create open and closed nodes lists.
            ResetAllNodes();
            var open = new List<Node>();
            var closed = new List<Node>();
            // Assign the source as the current node.
            var currentNode = source;
            // Add the current node to open nodes list.
            open.Add(currentNode);

            // Loop through the open list while the destination hasn't been reached.
            while (!currentNode.Equals(destination))
            {
                // Loop through all the directions the character can go in.
                foreach (var direction in _directions)
                {
                    // Find the index of the next node.
                    var newNodeIndex = currentNode.Index + direction;

                    // Check if the node is out of the map, if it is then continue.
                    if (newNodeIndex.x < 0 || newNodeIndex.x >= _gridSize.x || newNodeIndex.y < 0 || newNodeIndex.y >= _gridSize.y) continue;

                    // Get the node.
                    var newNode = _nodes[newNodeIndex.x, newNodeIndex.y];
                    // Check if it is walkable, if not then continue.
                    if (!newNode.IsWalkable) continue;
                    // Check if it has already been traversed, if yes then continue.
                    if (closed.Contains(newNode)) continue;

                    // Calculate the G, H and F values for the node.
                    newNode.G = Vector2.Distance(currentNode.WorldPos, newNode.WorldPos) + currentNode.G;
                    newNode.H = Vector2.Distance(newNode.WorldPos, destination.WorldPos);
                    newNode.F = newNode.G + newNode.H;

                    // Set the parent node of the next node as the current node.
                    newNode.ParentNode = currentNode;

                    // Check if the open list contains the new node, if not then add it to the list.
                    if (!open.Contains(newNode))
                        open.Add(newNode);
                }

                // Add the current node to the closed list.
                closed.Add(currentNode);
                // Remove the current node from the open list.
                open.Remove(currentNode);
                // Sort the open list first by F and then by H.
                open = open.OrderBy(node => node.F).ThenBy(node => node.H).ToList();

                // Assign the first node in the open list as the next node to explore.
                currentNode = open[0];
            }

            // Create a stack of nodes for the path.
            var path = new Stack<Node>();
            // Populate the stack with nodes.
            do
            {
                path.Push(currentNode);
                currentNode = currentNode.ParentNode;
            } while (currentNode.ParentNode != null);

            // If this is the main character then assign the source, destination nodes and path to draw out by gizmos.
            if (isMainCharacter)
            {
                _sourceNode = source;
                _destinationNode = destination;

                _path = new List<Node> { source };
                _path.AddRange(path);
            }

            // Return the path formed.
            return path;
        }

        /// <summary>
        /// Function to reset all node data.
        /// </summary>
        private void ResetAllNodes()
        {
            // Reset all nodes.
            foreach (var node in _nodes)
            {
                node.Reset();
            }
        }

        /// <summary>
        /// Function to draw gizmos for nodes, source, destination and path.
        /// </summary>
        private void OnDrawGizmos()
        {
            // If there are no nodes then return.
            if (_nodes == null)
                return;

            // Draw each node.
            foreach (var node in _nodes)
            {
                // Destination as magenta wire sphere.
                if (node.Equals(_destinationNode))
                    Gizmos.color = Color.magenta;
                // Source as blue wire sphere.
                else if (node.Equals(_sourceNode))
                    Gizmos.color = Color.blue;
                // Walkable nodes as white.
                else if (node.IsWalkable)
                    Gizmos.color = Color.white;
                // Unwalkable nodes as red.
                else
                    Gizmos.color = Color.red;

                // Draw the wire sphere.
                Gizmos.DrawWireSphere(node.WorldPos, NodeRadius * 0.75f);
            }

            // Check if the path is valid.
            if (_path is { Count: > 0 })
            {
                // Give black color to the path.
                Gizmos.color = Color.black;
                for (var i = 0; i < _path.Count - 1; i++)
                    Gizmos.DrawLine(_path[i].WorldPos, _path[i + 1].WorldPos); // Draw the line between nodes.
            }
        }
    }

    /// <summary>
    /// Class to hold node data.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// World position of the node.
        /// </summary>
        public readonly Vector2 WorldPos;
        /// <summary>
        /// Index of the node in the ground system array.
        /// </summary>
        public readonly Vector2Int Index;
        /// <summary>
        /// Flag to indicate if the node is walkable.
        /// </summary>
        public readonly bool IsWalkable;

        /// <summary>
        /// Variables to store the G, H and F values of the node while pathfinding. 
        /// </summary>
        public float G, H, F;
        /// <summary>
        /// Parent node of this node, for pathfinding.
        /// </summary>
        public Node ParentNode;

        /// <summary>
        /// Constructor to create a node.
        /// </summary>
        /// <param name="worldPos">World Position of the node.</param>
        /// <param name="isWalkable">Flag to indicate if node is walkable.</param>
        /// <param name="index">Index of the node in the ground grid.</param>
        public Node(Vector2 worldPos, bool isWalkable, Vector2Int index)
        {
            // Initialize the values.
            WorldPos = worldPos;
            IsWalkable = isWalkable;
            Index = index;
        }

        
        /// <summary>
        /// Function to compare nodes.
        /// </summary>
        /// <param name="nodeToCompareWith">Node with which this node will be compared.</param>
        /// <returns>True if nodes are same, False if they are not.</returns>
        public override bool Equals(object nodeToCompareWith)
        {
            // Check if both nodes have same world position.
            return WorldPos.Equals(((Node)nodeToCompareWith)?.WorldPos);
        }

        /// <summary>
        /// Reset the node values before finding a new path.
        /// </summary>
        public void Reset()
        {
            // Reset the G, H, F and parent node values.
            G = 0;
            H = 0;
            F = 0;
            ParentNode = null;
        }
    }
}