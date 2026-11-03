param(
    [Parameter(Mandatory = $true)][string] $AssemblyPath
)

$ErrorActionPreference = 'Stop'

function Assert-NoReparsePoints($item)
{
    if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)
    {
        throw "Cannot clean a build folder containing a symbolic link or junction: '$($item.FullName)'."
    }

    if ($item.PSIsContainer)
    {
        foreach ($child in Get-ChildItem -LiteralPath $item.FullName -Force)
        {
            Assert-NoReparsePoints $child
        }
    }
}

try
{
    $repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..\..')).ProviderPath
    $buildPath = Join-Path $repoRoot 'Build'
    $assembly = Get-Item -LiteralPath $AssemblyPath
    $buildPrefix = $buildPath + [IO.Path]::DirectorySeparatorChar
    if ($assembly.FullName.StartsWith($buildPrefix, [StringComparison]::OrdinalIgnoreCase))
    {
        throw 'The compiled DLL must be outside the build folder that will be cleared.'
    }

    $changelogPath = Join-Path $repoRoot 'CHANGELOG.md'
    $manifestPath = Join-Path $repoRoot 'manifest.json'

    # Validate every required input before clearing any existing package files
    $files = @(
        $changelogPath
        (Join-Path $repoRoot 'icon.png')
        $manifestPath
        (Join-Path $repoRoot 'README.md')
        (Join-Path $repoRoot 'Bundle Export\faithfulbundle')
        (Join-Path $repoRoot 'Bundle Export\faithfulbundle.manifest')
        $assembly.FullName
        (Join-Path $assembly.DirectoryName 'Faithful.Shared.dll')
    )
    foreach ($file in $files)
    {
        if (!(Test-Path -LiteralPath $file -PathType Leaf))
        {
            throw "Missing package file: '$file'."
        }
    }

    $manifestVersion = (Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json).version_number
    if ($manifestVersion -notmatch '\A[0-9]+\.[0-9]+\.[0-9]+\z')
    {
        throw 'manifest.json must contain a three-part numeric version_number for the upload ZIP.'
    }

    $changelog = [IO.File]::ReadAllText($changelogPath)
    $foldStart = '<details><summary><strong>Old Changelogs</strong></summary>'
    # Unwrap the old manual fold if it is still present in the source
    $legacyFold = '(?ms)^' + [regex]::Escape($foldStart) + '[ \t]*\r?\n(?:\r?\n)?(?<entries>.*?)^</details>[ \t\r\n]*\z'
    $packageChangelog = [regex]::Replace($changelog, $legacyFold, '${entries}')
    $releases = [regex]::Matches($packageChangelog, '(?mi)^[ \t]{0,3}#{1,6}[ \t]+v?([0-9]+\.[0-9]+\.[0-9]+)(?=[ \t\r\n]|$)')
    if ($releases.Count -eq 0)
    {
        Write-Output "Faithful packaging : warning FTH002: No release heading found in CHANGELOG.md. Add a heading such as '### v$manifestVersion' to match manifest.json."
    }
    elseif ($releases[0].Groups[1].Value -ne $manifestVersion)
    {
        Write-Output "Faithful packaging : warning FTH002: Latest CHANGELOG.md entry is v$($releases[0].Groups[1].Value), but manifest.json is v$manifestVersion."
    }

    if ($releases.Count -gt 20)
    {
        $newline = [regex]::Match($packageChangelog, '\r?\n').Value
        $foldIndex = $releases[20].Index
        $recent = $packageChangelog.Substring(0, $foldIndex).TrimEnd("`r", "`n")
        $older = $packageChangelog.Substring($foldIndex).TrimEnd("`r", "`n")
        $packageChangelog = ($recent, $foldStart, $older, '</details>' -join ($newline + $newline)) + $newline
    }

    $languages = Join-Path $repoRoot 'Languages'
    if (!(Test-Path -LiteralPath $languages -PathType Container))
    {
        throw "Missing language folder: '$languages'."
    }

    if (Test-Path -LiteralPath $buildPath)
    {
        $build = Get-Item -LiteralPath $buildPath -Force
        if (!$build.PSIsContainer -or $build.FullName -ne $buildPath)
        {
            throw "Expected the package directory at '$buildPath'."
        }

        Assert-NoReparsePoints $build
        foreach ($child in Get-ChildItem -LiteralPath $buildPath -Force)
        {
            Remove-Item -LiteralPath $child.FullName -Recurse -Force
        }
    }

    $plugins = Join-Path $buildPath 'plugins'
    [void][IO.Directory]::CreateDirectory($plugins)
    foreach ($file in $files)
    {
        Copy-Item -LiteralPath $file -Destination $buildPath
    }
    if ($packageChangelog -cne $changelog)
    {
        [IO.File]::WriteAllText((Join-Path $buildPath 'CHANGELOG.md'), $packageChangelog)
    }
    Copy-Item -LiteralPath $languages -Destination $plugins -Recurse
    Write-Output "Assembled Faithful package in '$buildPath'."

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packagedPath = Join-Path $repoRoot 'Build (packaged)'
    [void][IO.Directory]::CreateDirectory($packagedPath)
    $zipName = "Faithful v$manifestVersion.zip"
    $zipPath = Join-Path $packagedPath $zipName
    # Write to a temporary file beside the final ZIP so only a complete archive is published
    $tempZip = Join-Path $packagedPath ('faithful-' + [guid]::NewGuid().ToString('N') + '.tmp')
    try
    {
        $zip = [IO.Compression.ZipFile]::Open($tempZip, [IO.Compression.ZipArchiveMode]::Create)
        try
        {
            foreach ($file in Get-ChildItem -LiteralPath $buildPath -Recurse -File -Force)
            {
                $entryName = $file.FullName.Substring($buildPrefix.Length).Replace('\', '/')
                [void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $entryName)
            }
        }
        finally
        {
            $zip.Dispose()
        }
        Move-Item -LiteralPath $tempZip -Destination $zipPath -Force
    }
    finally
    {
        if (Test-Path -LiteralPath $tempZip -PathType Leaf)
        {
            Remove-Item -LiteralPath $tempZip
        }
    }
    Write-Output "Created ZIP package: '$zipPath'."
    if ($releases.Count -eq 0 -or $releases[0].Groups[1].Value -ne $manifestVersion)
    {
        Write-Output 'Faithful packaging : warning FTH002: ZIP package created, but the changelog does not match the manifest version. Update CHANGELOG.md before uploading.'
    }
}
catch
{
    Write-Output "Faithful packaging : error FTH003: $($_.Exception.Message)"
    exit 1
}
