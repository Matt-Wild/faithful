param(
    [Parameter(Mandatory = $true)][string] $AssemblyPath,
    [Parameter(Mandatory = $true)][string] $ManifestPath
)

$ErrorActionPreference = 'Stop'

try
{
    # Read metadata without loading the plugin or its Unity dependencies
    [void][Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot '..\NetworkWeaver\Unity.Cecil.dll'))
    $assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($AssemblyPath)
    try
    {
        $plugins = @($assembly.MainModule.Types | ForEach-Object { $_.CustomAttributes } |
            Where-Object { $_.AttributeType.FullName -eq 'BepInEx.BepInPlugin' })
        if ($plugins.Count -ne 1 -or $plugins[0].ConstructorArguments.Count -ne 3)
        {
            throw 'Expected exactly one BepInPlugin attribute with a version in the built assembly.'
        }

        $version = [string] $plugins[0].ConstructorArguments[2].Value
        if ($version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+$')
        {
            throw "Plugin version '$version' must contain three numeric components for the Thunderstore manifest."
        }
    }
    finally
    {
        $assembly.Dispose()
    }

    $manifestText = [IO.File]::ReadAllText($ManifestPath)
    $manifest = $manifestText | ConvertFrom-Json
    if ($null -eq $manifest -or $manifest.version_number -isnot [string])
    {
        throw "Manifest '$ManifestPath' must have a string version_number property."
    }

    # Replace only the version value, retaining indentation, line endings and other fields
    $versionPattern = [regex] '(?<prefix>"version_number"\s*:\s*")[^"\\]*(?:\\.[^"\\]*)*(?<suffix>")'
    if ($versionPattern.Matches($manifestText).Count -ne 1)
    {
        throw "Manifest '$ManifestPath' must have exactly one version_number property."
    }

    if ($manifest.version_number -eq $version)
    {
        Write-Output 'Skipped manifest update: the manifest version already matches the DLL version.'
        return
    }

    $updatedManifest = $versionPattern.Replace($manifestText, [Text.RegularExpressions.MatchEvaluator] {
        param($match)
        $match.Groups['prefix'].Value + $version + $match.Groups['suffix'].Value
    })

    $manifestBytes = [IO.File]::ReadAllBytes($ManifestPath)
    $hasUtf8Bom = $manifestBytes.Length -ge 3 -and $manifestBytes[0] -eq 0xEF -and
        $manifestBytes[1] -eq 0xBB -and $manifestBytes[2] -eq 0xBF
    [IO.File]::WriteAllText($ManifestPath, $updatedManifest, (New-Object Text.UTF8Encoding($hasUtf8Bom)))
    Write-Output "Manifest version updated to $version."
}
catch
{
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
