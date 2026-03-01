using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful.Shared
{
    public enum RendererModifier : int
    {
        None = 0,
        HopooShader = 10,
        InfusionGlass = 20
    }

    [Serializable] public struct ShaderTextureRule { public string name; public Texture texture; }
    [Serializable] public struct ShaderColourRule { public string name; public Color colour; }
    [Serializable] public struct ShaderVectorRule { public string name; public Vector4 value; }
    [Serializable] public struct ShaderKeywordRule { public string keyword; public bool enabled; }
    [Serializable] public struct ShaderFloatRule { public string name; public float value; }
    [Serializable] public struct ShaderIntRule { public string name; public int value; }

    [DisallowMultipleComponent]
    public sealed class RendererRules : MonoBehaviour
    {
        [Header("General")]
        public RendererModifier modifier = RendererModifier.None;

        [Header("Shader Attributes")]
        public List<ShaderTextureRule> textureRules = new();
        public List<ShaderColourRule> colourRules = new();
        public List<ShaderVectorRule> vectorRules = new();
        public List<ShaderKeywordRule> keywordRules = new();
        public List<ShaderFloatRule> floatRules = new();
        public List<ShaderIntRule> intRules = new();
    }
}
