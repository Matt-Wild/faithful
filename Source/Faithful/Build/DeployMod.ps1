$ErrorActionPreference = 'Stop'

function Assert-OrdinaryItem($item)
{
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)
    {
        throw "Symbolic links and junctions are not allowed: '$($item.FullName)'."
    }
}

function Assert-SafeToClear($directory)
{
    foreach ($item in Get-ChildItem -LiteralPath $directory.FullName -Force)
    {
        Assert-OrdinaryItem $item
        if ($item.PSIsContainer) { Assert-SafeToClear $item }
    }
}

try
{
    $settingsPath = Join-Path $PSScriptRoot '..\Faithful.Local.props'
    if (!(Test-Path -LiteralPath $settingsPath))
    {
        $template = @'
<Project>
  <PropertyGroup>
    <!-- Absolute path to the existing Faithful mod folder containing Faithful.dll. -->
    <FaithfulDeployDir></FaithfulDeployDir>
  </PropertyGroup>
</Project>
'@
        New-Item -ItemType File -Path $settingsPath -Value ($template + [Environment]::NewLine) | Out-Null
    }

    [xml] $settings = Get-Content -LiteralPath $settingsPath -Raw
    $setting = $settings.SelectSingleNode('/Project/PropertyGroup/FaithfulDeployDir')
    $configuredPath = if ($null -ne $setting) { $setting.InnerText.Trim() } else { '' }
    if (!$configuredPath)
    {
        throw 'FaithfulDeployDir is empty in Faithful.Local.props.'
    }
    if ($configuredPath -notmatch '^[A-Za-z]:[\\/][^:]*$')
    {
        throw 'FaithfulDeployDir must be an absolute local folder path.'
    }

    $target = Get-Item -LiteralPath ([IO.Path]::GetFullPath($configuredPath)) -Force
    if (!$target.PSIsContainer -or !$target.Parent -or $target.Name -in @('plugins', 'BepInEx'))
    {
        throw 'Choose the existing Faithful mod folder, not a drive root or shared BepInEx directory.'
    }

    $repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).ProviderPath.TrimEnd('\')
    $targetPath = $target.FullName.TrimEnd('\')
    $comparison = [StringComparison]::OrdinalIgnoreCase
    if ($targetPath -eq $repoRoot -or $targetPath.StartsWith($repoRoot + '\', $comparison) -or
        $repoRoot.StartsWith($targetPath + '\', $comparison))
    {
        throw 'FaithfulDeployDir must not overlap the repository.'
    }
    for ($directory = $target; $null -ne $directory; $directory = $directory.Parent)
    {
        Assert-OrdinaryItem $directory
    }
    if (!(Test-Path -LiteralPath (Join-Path $targetPath 'Faithful.dll') -PathType Leaf))
    {
        throw "No previous Faithful.dll found in '$targetPath'."
    }
    Assert-SafeToClear $target
}
catch
{
    Write-Output "Faithful deployment : warning FTH001: Skipped deployment. $($_.Exception.Message)"
    Write-Output '  Setup:   Open Source/Faithful/Faithful.Local.props (local to your machine).'
    Write-Output '  Setting: <FaithfulDeployDir>D:\MyTestProfile\BepInEx\plugins\Faithful</FaithfulDeployDir>'
    Write-Output '  Target:  Use your existing Faithful mod folder containing Faithful.dll, then build again.'
    return
}

try
{
    $packagePath = Join-Path $repoRoot 'Build'
    if (!(Test-Path -LiteralPath (Join-Path $packagePath 'Faithful.dll') -PathType Leaf))
    {
        throw "No assembled Faithful.dll found in '$packagePath'."
    }

    $packageContents = @(Get-ChildItem -LiteralPath $packagePath -Force)
    foreach ($item in Get-ChildItem -LiteralPath $targetPath -Force)
    {
        Remove-Item -LiteralPath $item.FullName -Recurse -Force
    }
    foreach ($item in $packageContents)
    {
        Copy-Item -LiteralPath $item.FullName -Destination $targetPath -Recurse -Force
    }
    Write-Output "Deployed Faithful to '$targetPath'."
}
catch
{
    [Console]::Error.WriteLine("Faithful deployment failed: $($_.Exception.Message)")
    exit 1
}
