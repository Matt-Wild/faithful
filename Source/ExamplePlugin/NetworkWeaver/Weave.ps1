param(
    [Parameter(Mandatory = $true)][string] $AssemblyPath,
    [string] $SymbolsPath,
    [Parameter(Mandatory = $true)][string] $ReferenceList,
    [Parameter(Mandatory = $true)][string] $WorkingDirectory
)

$ErrorActionPreference = 'Stop'

try
{
    $references = @(Get-Content -LiteralPath $ReferenceList | Where-Object { $_ })
    $unityPath = $references | Where-Object { [IO.Path]::GetFileName($_) -eq 'UnityEngine.CoreModule.dll' }
    $unetPath = $references | Where-Object { [IO.Path]::GetFileName($_) -eq 'com.unity.multiplayer-hlapi.Runtime.dll' }
    if (!$unityPath -or !$unetPath)
    {
        throw 'UNetWeaver could not find the Unity and UNet build references.'
    }

    $inputDirectory = Join-Path $WorkingDirectory 'Input'
    $outputDirectory = Join-Path $WorkingDirectory 'Output'
    $assemblyName = [IO.Path]::GetFileName($AssemblyPath)
    $symbolsName = [IO.Path]::ChangeExtension($assemblyName, '.pdb')

    # Always weave a fresh copy so repeated builds cannot patch the same assembly twice
    foreach ($directory in @($inputDirectory, $outputDirectory))
    {
        [void][IO.Directory]::CreateDirectory($directory)
        foreach ($name in @($assemblyName, $symbolsName, "$assemblyName.mdb"))
        {
            $path = Join-Path $directory $name
            if (Test-Path -LiteralPath $path)
            {
                Remove-Item -LiteralPath $path -Force
            }
        }
    }

    $inputAssembly = Join-Path $inputDirectory $assemblyName
    Copy-Item -LiteralPath $AssemblyPath -Destination $inputAssembly
    if ($SymbolsPath)
    {
        Copy-Item -LiteralPath $SymbolsPath -Destination (Join-Path $inputDirectory $symbolsName)
    }

    foreach ($name in @('Unity.Cecil.dll', 'Unity.Cecil.Pdb.dll', 'Unity.Cecil.Mdb.dll'))
    {
        [void][Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot $name))
    }
    $weaver = [Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot 'Unity.UNetWeaver.exe'))
    [Unity.UNetWeaver.Log]::WarningMethod = [Action[string]] { param($message) [Console]::WriteLine("UNetWeaver : warning : $message") }
    [Unity.UNetWeaver.Log]::ErrorMethod = [Action[string]] { param($message) [Console]::Error.WriteLine("UNetWeaver : error : $message") }

    # The console entry point only accepts one reference folder and does not report failure through its exit code
    [string[]] $referenceDirectories = @($references | ForEach-Object { [IO.Path]::GetDirectoryName($_) } | Sort-Object -Unique)
    $weave = $weaver.GetType('Unity.UNetWeaver.Weaver').GetMethod('WeaveAssemblies')
    $success = $weave.Invoke($null, @([string[]] @($inputAssembly), $referenceDirectories, $null, [string] $outputDirectory, [string] $unityPath, [string] $unetPath))
    if (!$success -or !(Test-Path -LiteralPath (Join-Path $outputDirectory $assemblyName)))
    {
        throw 'UNetWeaver failed to patch the assembly. See the build output for details.'
    }
    if ($SymbolsPath -and !(Test-Path -LiteralPath (Join-Path $outputDirectory $symbolsName)))
    {
        throw 'UNetWeaver did not produce matching debug symbols.'
    }
}
catch
{
    [Console]::Error.WriteLine($_.Exception.ToString())
    exit 1
}
