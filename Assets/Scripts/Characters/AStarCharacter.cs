using System.Collections;
using Environment;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// Abstract class to handle character movement. 
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class AStarCharacter : MonoBehaviour
    {
        /// <summary>
        /// Current Node the character is at.
        /// </summary>
        private Node _currentNode;

        /// <summary>
        /// Target's position, duh!!
        /// </summary>
        protected Node TargetNode
        {
            get => _targetNode;
            set
            {
                // Check if the value is same as the current target node, if it is then return. 
                if (_targetNode != null && _targetNode.Equals(value))
                    return;

                //Set the value as target node.
                _targetNode = value;
                // Stop any coroutine which is traversing a path.
                if (_traversalCoroutine != null)
                    StopCoroutine(_traversalCoroutine);

                // Start the traversal coroutine with new target.
                _traversalCoroutine = StartCoroutine(TraverseToTarget());
            }
        }

        /// <summary>
        /// Variable to store the target node.
        /// </summary>
        private Node _targetNode;

        /// <summary>
        /// Time taken to traverse between nodes.
        /// </summary>
        private const float TimeBetweenNodes = 0.2f;

        /// <summary>
        /// Coroutine used to traverse the given path.
        /// </summary>
        private Coroutine _traversalCoroutine;

        private void Start()
        {
            // Get the current node near which the character is standing.
            _currentNode = GroundSystem.Instance.GetNearestNode(transform.position);
            transform.position = _currentNode.WorldPos;
        }

        /// <summary>
        /// Coroutine to Traverse to the target node.
        /// </summary>
        private IEnumerator TraverseToTarget()
        {
            // Get the path to the target.
            var path = GroundSystem.Instance.GetPath(_currentNode, TargetNode, GetType() == typeof(MainAStarCharacter));

            // Check if the path is null or empty, if it is then break out of the Coroutine.
            if (path == null || path.Count == 0)
                yield break;

            // Traverse through all the nodes in the path
            while (path.Count > 0)
            {
                // Get the next node.
                var nextNode = path.Pop();
                var deltaTime = 0f;
                // Look at the next node
                LookAtTarget(_currentNode.WorldPos, nextNode.WorldPos);
                
                // Lerp to the next node.
                while (Vector2.Distance(transform.position, nextNode.WorldPos) > 0.01f)
                {
                    transform.position = Vector2.Lerp(_currentNode.WorldPos, nextNode.WorldPos, deltaTime / TimeBetweenNodes);
                    yield return null;
                    deltaTime += Time.deltaTime;
                }

                // Set the final position after lerp-ing.
                transform.position = nextNode.WorldPos;
                // Set the next node as the current node.
                _currentNode = nextNode;
            }
        }

        /// <summary>
        /// Function to make the character look at the target.
        /// </summary>
        private void LookAtTarget(Vector2 source, Vector2 destination)
        {
            // Get the normalized direction of the character.
            var direction = (destination - source).normalized;
            // Apply the rotation of the object.
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        }
    }
}