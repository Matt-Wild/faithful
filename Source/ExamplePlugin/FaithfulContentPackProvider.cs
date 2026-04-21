using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Faithful
{
    internal sealed class FaithfulContentPackProvider : IContentPackProvider
    {
        internal static readonly FaithfulContentPackProvider Instance = new();

        private static bool registered;

        private readonly ContentPack contentPack = new();
        private ItemRelationshipProvider corruptionProvider;

        public string identifier => Faithful.PluginGUID;

        internal static void Init()
        {
            if (registered) return;

            registered = true;
            ContentManager.collectContentPackProviders += AddContentPackProvider;
        }

        private static void AddContentPackProvider(ContentManager.AddContentPackProviderDelegate _addContentPackProvider)
        {
            _addContentPackProvider(Instance);
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs _args)
        {
            contentPack.identifier = identifier;

            if (corruptionProvider == null)
            {
                corruptionProvider = ScriptableObject.CreateInstance<ItemRelationshipProvider>();
                corruptionProvider.name = $"{Faithful.PluginGUID}.ItemCorruptions";
                corruptionProvider.relationshipType =
                    Addressables.LoadAssetAsync<ItemRelationshipType>("RoR2/DLC1/Common/ContagiousItem.asset")
                        .WaitForCompletion();

                corruptionProvider.relationships = [];

                contentPack.itemRelationshipProviders.Add([corruptionProvider]);
            }

            _args.ReportProgress(1.0f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs _args)
        {
            contentPack.identifier = identifier;

            corruptionProvider.relationships = Utils.BuildCorruptionPairsForContentPack(_args.peerLoadInfos);

            ContentPack.Copy(contentPack, _args.output);
            _args.ReportProgress(1.0f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs _args)
        {
            _args.ReportProgress(1.0f);
            yield break;
        }
    }
}
