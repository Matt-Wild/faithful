using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class RecollectionShrine : Interactable
    {
        public RecollectionShrine()
        {
            // Initialise interactable
            Init("RECOLLECTION_SHRINE", "RecollectionShrineMesh", PingIconType.Shrine, _costType: CostTypeIndex.None, _symbolName: "texShrineRecollectionSymbol", _symbolColour: new Color(1.0f, 0.23525f, 0.49f));

            // Add set spawns
            AddSetSpawn("blackbeach", new Vector3(31, -211, -122), new Vector3(0, 0, 0));
            AddSetSpawn("blackbeach2", new Vector3(-153, 11, -31), new Vector3(0, 0, 0));
            AddSetSpawn("snowyforest", new Vector3(-138, 5, 8), new Vector3(0, 0, 0));
            AddSetSpawn("village", new Vector3(134, 14, -149), new Vector3(0, 0, 0));
            AddSetSpawn("golemplains", new Vector3(-120, -139, -160), new Vector3(0, 0, 0));
            AddSetSpawn("golemplains2", new Vector3(-20, 10, -11), new Vector3(0, 0, 0));
            AddSetSpawn("lakes", new Vector3(-69, 1, -145), new Vector3(0, 0, 0));

            AddSetSpawn("moon2", new Vector3(1038F, -284.05F, 1154F), new Vector3(0, 65, 0));
            AddSetSpawn("limbo", new Vector3(-47.825F, -11.1F, -35F), new Vector3(0, 180, 0));
            AddSetSpawn("voidraid", new Vector3(-21F, 28F, -170F), new Vector3(0, 140, 0));
            AddSetSpawn("meridian", new Vector3(-223F, -188.125F, 238F), new Vector3(0, 30, 0));
            AddSetSpawn("goldshores", new Vector3(-8.5F, 69.125F, -66.25F), new Vector3(0, 339, 0));
            AddSetSpawn("goldshores", new Vector3(102F, -6.75F, 67.5F), new Vector3(0, 40, 0));
            AddSetSpawn("goldshores", new Vector3(-90F, -8.125F, -88F), new Vector3(0, 249, 0));
            AddSetSpawn("voidstage", new Vector3(-44F, 91.75F, -53F), new Vector3(0, 15, 0));
            AddSetSpawn("voidstage", new Vector3(55F, -2F, 180F), new Vector3(0, 0, 0));
            AddSetSpawn("voidstage", new Vector3(-170F, 29.5F, -210F), new Vector3(0, 75, 0));
            AddSetSpawn("voidstage", new Vector3(-101F, 29.625F, 65F), new Vector3(0, 120, 0));
            AddSetSpawn("artifactworld", new Vector3(78F, -19.25F, 81F), new Vector3(0, 90, 0));
            AddSetSpawn("artifactworld", new Vector3(-1F, -0.275F, 23F), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld", new Vector3(93.9F, -6.3F, -97F), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld01", new Vector3(-80F, -22.5F, -46.125F), new Vector3(0, 90, 0));
            AddSetSpawn("artifactworld01", new Vector3(-33F, -23F, 45F), new Vector3(0, 270, 0));
            AddSetSpawn("artifactworld01", new Vector3(37.55F, -27.75F, 117.5F), new Vector3(0, 180, 0));
            AddSetSpawn("artifactworld02", new Vector3(-74F, -10.25F, -25F), new Vector3(0, 180, 0));
            AddSetSpawn("artifactworld02", new Vector3(10.75F, 13F, -144.5F), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld02", new Vector3(-8.5F, 51.45F, 36F), new Vector3(0, 225, 0));
            AddSetSpawn("artifactworld02", new Vector3(0.625F, -36.625F, 49F), new Vector3(0, 180, 0));
            AddSetSpawn("artifactworld03", new Vector3(-56.325F, 27.5F, -103.5F), new Vector3(0, 0, 0));
            AddSetSpawn("artifactworld03", new Vector3(59.5F, 13.125F, 75F), new Vector3(0, 180, 0));
            AddSetSpawn("artifactworld03", new Vector3(-28.375F, 29.25F, 17.5F), new Vector3(0, 180, 0));

            // Add behaviour
            Behaviour.AddOnPrePopulateSceneCallback(OnPrePopulateScene);
        }

        public override void OnPurchase(Interactor _interactor)
        {
            Debug.Log("SHRINE USED");
        }

        private void OnPrePopulateScene(SceneDirector _director)
        {
            // This shrine has set spawns on various stages
            DoSetSpawn();
        }
    }
}
