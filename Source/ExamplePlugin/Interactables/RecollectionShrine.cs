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

            AddSetSpawn("moon2", new Vector3(1038, -282, 1154), new Vector3(0, 0, 0));
            AddSetSpawn("limbo", new Vector3(-48, -9, -36), new Vector3(0, 0, 0));
            AddSetSpawn("voidraid", new Vector3(-21, 30, -170), new Vector3(0, 0, 0));
            AddSetSpawn("meridian", new Vector3(-223, -186, 238), new Vector3(0, 0, 0));
            AddSetSpawn("goldshores", new Vector3(-8, 71, -66), new Vector3(0, 0, 0));
            AddSetSpawn("goldshores", new Vector3(102, -5, 69), new Vector3(0, 0, 0));
            AddSetSpawn("goldshores", new Vector3(-90, -6, -88), new Vector3(0, 0, 0));
            AddSetSpawn("voidstage", new Vector3(-44, 94, -52), new Vector3(0, 0, 0));
            AddSetSpawn("voidstage", new Vector3(55, 0, 180), new Vector3(0, 0, 0));
            AddSetSpawn("voidstage", new Vector3(-170, 31, -210), new Vector3(0, 0, 0));
            AddSetSpawn("voidstage", new Vector3(-101, 31, 65), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld", new Vector3(78, -17, 81), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld", new Vector3(-1, 2, 23), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld", new Vector3(94, -4, -97), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld01", new Vector3(-78, -21, -46), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld01", new Vector3(-33, -22, 45), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld01", new Vector3(38, -26, -117), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld02", new Vector3(-74, -8, -25), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld02", new Vector3(10, 15, -144), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld02", new Vector3(20, 20, -22), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld02", new Vector3(1, -35, 49), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld03", new Vector3(-56, 20, -103), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld03", new Vector3(60, 14, 73), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld03", new Vector3(-28, 31, 16), new Vector3(0, 0, 0));

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
