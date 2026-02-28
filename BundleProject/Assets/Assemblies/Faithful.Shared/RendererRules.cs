using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful.Shared
{
    public enum RendererModifier : int
    {
        None = 0,
        HopooShader = 10
    }

    [Serializable]
    public struct ShaderKeywordRule
    {
        public string keyword;
        public bool enabled;
    }

    [DisallowMultipleComponent]
    public sealed class RendererRules : MonoBehaviour
    {
        [Header("General")]
        public RendererModifier modifier = RendererModifier.None;

        [Header("Shader Attributes")]
        public List<ShaderKeywordRule> keywordRules = new();
    }
}
