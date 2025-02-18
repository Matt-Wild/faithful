using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class RecollectionShrine : Interactable
    {
        public RecollectionShrine()
        {
            // Initialise interactable
            Init("RECOLLECTION_SHRINE", "RecollectionShrineMesh");

            // Add set spawns
            AddSetSpawn("blackbeach2", new Vector3(-153, 11, -31), new Vector3(0, 0, 0));
            AddSetSpawn("snowyforest", new Vector3(-138, 5, 8), new Vector3(0, 0, 0));
            AddSetSpawn("village", new Vector3(134, 14, -149), new Vector3(0, 0, 0));

            // Add behaviour
            Behaviour.AddOnPrePopulateSceneCallback(OnPrePopulateScene);
        }

        private void OnPrePopulateScene(SceneDirector _director)
        {
            // This shrine has set spawns on various stages
            DoSetSpawn();
        }
    }
}
