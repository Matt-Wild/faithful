using System;
using System.Collections.Generic;
using UnityEngine;

namespace Faithful.Shared
{
    public enum RendererModifier : int
    {
        None = 0,
        HopooShader = 10,
        InfusionGlass = 20,
        AreaIndicatorIntersectionOnly = 30,
        AreaIndicatorRim = 40,
        Artifact = 50,
        ArtifactShellExplosionIndicator = 60,
        BaubleTimestopSphere = 70,
        BlueprintScreen = 80,
        BlueprintsInvalid = 90,
        BlueprintsOk = 100,
        BootShockwave = 110,
        CaptainAirstrikeAltAreaIndicatorInner = 120,
        CaptainAirstrikeAltAreaIndicatorOuter = 130,
        CaptainAirstrikeAreaIndicator = 140,
        CaptainSupplyDropAreaIndicator2 = 150,
        CaptainSupplyDropAreaIndicatorOuter = 160,
        ChefAlwaysOnTop = 170,
        ChildStarCore = 180,
        ChildStarGlow = 190,
        ChipProjectile = 200,
        ClayBubble = 210,
        CombinerEnergyFade = 220,
        CoreCage2 = 230,
        DefectiveUnitDenialSphereNoise = 240,
        DefectiveUnitDetonateSphereEnergy = 250,
        DefectiveUnitDetonateSphereEnergyPers = 260,
        DefectiveUnitDetonateSphereNoise = 270,
        DefectiveUnitDetonateSpherePulse = 280,
        DefectiveUnitNullifySpherePulse = 290,
        DroneBrokenGeneric = 300,
        NullifierArmor = 310,
        NullifierGemPortal = 320,
        NullifierGemPortal3 = 330,
        NullifierBlackholeZoneAreaIndicator = 340,
        NullifierExplosionAreaIndicatorHard = 350,
        RadarTowerAreaIndicator = 360,
        ShockDamageAuraGlass = 370,
        TeamAreaIndicatorFullMonster = 380,
        TeamAreaIndicatorFullPlayer = 390,
        TeamAreaIndicatorIntersectionMonster = 400,
        TeamAreaIndicatorIntersectionPlayer = 410,
        TimeCrystalAreaIndicator = 420,
        VoidDeathBombAreaIndicatorBack = 430,
        VoidDeathBombAreaIndicatorFront = 440,
        VoidSurvivorBlasterSphereAreaIndicator = 450,
        VoidSurvivorBlasterSphereAreaIndicatorCorrupted = 460
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
        public bool salvageExistingProperties = false;

        [Header("Shader Attributes")]
        public List<ShaderTextureRule> textureRules = new();
        public List<ShaderColourRule> colourRules = new();
        public List<ShaderVectorRule> vectorRules = new();
        public List<ShaderKeywordRule> keywordRules = new();
        public List<ShaderFloatRule> floatRules = new();
        public List<ShaderIntRule> intRules = new();
    }
}
