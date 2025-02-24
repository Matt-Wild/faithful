using RoR2;
using UnityEngine;

namespace Faithful
{
    internal class RecollectionShrine : Interactable
    {
        // Remember reference to collectors vision item
        private ItemDef m_cvItem;

        public RecollectionShrine()
        {
            // Initialise interactable
            Init("RECOLLECTION_SHRINE", "RecollectionShrineMesh", PingIconType.Shrine, _costType: InteractableCostType.Custom, _symbolName: "texShrineRecollectionSymbol", 
                 _symbolColour: new Color(1.0f, 0.23525f, 0.49f), _customCostColour: ColorCatalog.ColorIndex.VoidItem, _requiredExpansion: InteractableRequiredExpansion.SurvivorsOfTheVoid);

            // Add set spawns
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

        public override void OnPurchase(FaithfulInteractableBehaviour _behaviour, Interactor _interactor)
        {
            // Get character body
            CharacterBody body = _interactor.GetComponent<CharacterBody>();
            if (body == null) return;

            // Get player master controller
            PlayerCharacterMasterController player = body.master?.playerCharacterMasterController;
            if (player == null) return;

            // Get lookup string for this player
            string lookupString = $"{player.networkUser.id} IC";

            // Get carryover inspiration amount
            int carryoverInspiration = LookupTable.GetInt(lookupString);
            if (carryoverInspiration == 0) return;

            // Do the shrine use effect
            _behaviour.DoShrineUseEffect();

            // Get inspiration buff definition
            BuffDef inspirationBuff = Buffs.GetBuff("INSPIRATION").buffDef;

            // Cycle through carried over inspiration
            for (int i = 0; i < carryoverInspiration; i++)
            {
                // Add stack of buff
                body.AddBuff(inspirationBuff);
            }

            // Reset carryover inspiration (this can be thought of as the cost)
            LookupTable.SetInt(lookupString, 0);

            // Check if nobody can use the shrine anymore
            if (!CanBeUsed())
            {
                // Deactivate shrine
                _behaviour.SetUnavailable();
            }

            // Create params for message
            string[] messageParams = [carryoverInspiration.ToString()];

            // Send interaction message
            SendInteractionMessage(body, messageParams);
        }

        public override bool CustomIsAffordable(CostTypeDef _costTypeDef, CostTypeDef.IsAffordableContext _context)
        {
            // Get player character body
            CharacterBody body = _context.activator.GetComponent<CharacterBody>();
            if (body == null) return false;

            // Get player inventory
            Inventory inv = body.inventory;
            if (inv == null) return false;

            // Check for item
            if (inv.GetItemCount(cvItem) <= 0) return false;

            // Get player master controller
            PlayerCharacterMasterController player = body.master?.playerCharacterMasterController;
            if (player == null) return false;

            // This shrine is interactable if this player has "cached" or carryover inspiration
            return LookupTable.GetInt($"{player.networkUser.id} IC") > 0;
        }

        public override void CustomPayCost(CostTypeDef _costTypeDef, CostTypeDef.PayCostContext _context)
        {
            // Cost needs to happen after shrine is purchased (so do cost in on purchase method)
        }

        private void OnPrePopulateScene(SceneDirector _director)
        {
            // Check if any players are able to use the shrine
            if (!CanBeUsed()) return;

            // This shrine has set spawns on various stages
            DoSetSpawn();
        }

        private bool CanBeUsed()
        {
            // Cycle through players
            foreach (PlayerCharacterMasterController player in Utils.GetPlayers())
            {
                // Check for cached inspiration
                if (LookupTable.GetInt($"{player.networkUser.id} IC") <= 0) continue;
            }

            // No valid player found
            return false;
        }

        private ItemDef cvItem
        {
            get
            {
                // Check for cached reference
                if (m_cvItem == null)
                {
                    // Get item reference
                    m_cvItem = Items.GetItem("COLLECTORS_VISION").itemDef;
                }

                // Return item reference
                return m_cvItem;
            }
        }
    }
}
