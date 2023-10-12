using Environment;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// Class to handle AI character's movement.
    /// </summary>
    public class AIAStarCharacter : AStarCharacter
    {
        /// <summary>
        /// The main character which is being targeted by the AI.
        /// </summary>
        [SerializeField] private MainAStarCharacter targetCharacter;

        private void Update()
        {
            // Get the nearest node to the target.
            var nearestNodeToTarget = GroundSystem.Instance.GetNearestNode(targetCharacter.transform.position);
            // If it's the same as the target node then return.
            if (TargetNode != null && TargetNode.Equals(nearestNodeToTarget))
                return;

            // Else set it as the target node.
            TargetNode = nearestNodeToTarget;
        }
    }
}