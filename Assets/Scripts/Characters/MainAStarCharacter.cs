using Environment;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// Class to handle Main character's movement.
    /// </summary>
    public class MainAStarCharacter : AStarCharacter
    {
        private void OnEnable()
        {
            InputHandler.OnMouseClick += ChangeTarget;
        }

        private void OnDisable()
        {
            InputHandler.OnMouseClick -= ChangeTarget;
        }

        /// <summary>
        /// Function to change the target of the main character.
        /// </summary>
        /// <param name="mouseClickWorldPosition">World position of the mouse click.</param>
        private void ChangeTarget(Vector2 mouseClickWorldPosition)
        {
            // Get the nearest node from the Ground system.
            var nearestNodeToTarget = GroundSystem.Instance.GetNearestNode(mouseClickWorldPosition);
            // Check if it is walkable, if it is then set the target node as that node.
            if (nearestNodeToTarget.IsWalkable)
                TargetNode = nearestNodeToTarget;
        }
    }
}