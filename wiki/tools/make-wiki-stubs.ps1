<#
.SYNOPSIS
Rewrites every page of the legacy GitHub wiki into a short "this page has moved" stub.

.DESCRIPTION
The documentation moved from the GitHub wiki to https://dotnet.github.io/aspnet-api-versioning.
GitHub wikis cannot issue HTTP redirects: there is no _redirects or .htaccess, and GitHub's
sanitizer strips meta-refresh, <script>, and <link rel="canonical">. The next best thing is a
thin stub per page pointing at the new site, so stale inbound links from articles, training
material, and blog posts land somewhere useful instead of on stale duplicate content.

The wiki is a git repository, so all pages are rewritten in one pass and pushed as one commit.

Dry run by default. Nothing is written without -Apply, and the script never pushes; it prints
the git commands for you to run yourself.

.PARAMETER WikiPath
Path to a clone of https://github.com/dotnet/aspnet-api-versioning.wiki.git. Clone with full
history - a shallow clone (--depth 1) can be rejected on push with "shallow update not allowed".

.PARAMETER MapPath
Path to wiki-redirect-map.tsv. Tab-separated; a row whose columns are space-separated instead
of tab-separated will fail with a "malformed row" error rather than silently misfiring.

.EXAMPLE
  git clone https://github.com/dotnet/aspnet-api-versioning.wiki.git
  ./make-wiki-stubs.ps1 -WikiPath ./aspnet-api-versioning.wiki -MapPath ./wiki-redirect-map.tsv
  ./make-wiki-stubs.ps1 -WikiPath ./aspnet-api-versioning.wiki -MapPath ./wiki-redirect-map.tsv -Apply

.NOTES
To preview exactly how GitHub will render a stub (its sanitizer differs from local previewers):

  $md = Get-Content -Raw ./aspnet-api-versioning.wiki/Home.md
  $body = @{ text = $md; mode = 'gfm' } | ConvertTo-Json -Compress
  $tmp = [System.IO.Path]::GetTempFileName()
  [System.IO.File]::WriteAllText($tmp, $body, [System.Text.UTF8Encoding]::new($false))
  gh api --method POST /markdown --input $tmp
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$WikiPath,
    [Parameter(Mandatory = $true)][string]$MapPath,
    [string]$BaseUrl = 'https://dotnet.github.io/aspnet-api-versioning',
    [switch]$Apply
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')

if (-not (Test-Path -LiteralPath $WikiPath)) { throw "wiki path not found: $WikiPath" }
if (-not (Test-Path -LiteralPath $MapPath))  { throw "map not found: $MapPath" }

# --- load the mapping -------------------------------------------------------
$map = @{}
foreach ($line in Get-Content -LiteralPath $MapPath) {
    if ($line -match '^\s*(#|$)') { continue }
    $cols = $line -split "`t"
    if ($cols.Count -lt 2) { throw "malformed row (expected 2-3 tab-separated columns): $line" }
    $map[$cols[0].Trim()] = @{
        Core   = $cols[1].Trim()
        AspNet = if ($cols.Count -ge 3) { $cols[2].Trim() } else { '-' }
    }
}

# --- reconcile the map against what is actually in the wiki -----------------
$pages = Get-ChildItem -LiteralPath $WikiPath -Filter *.md -File | Sort-Object Name
$onDisk = $pages | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_.Name) }

$unmapped = $onDisk | Where-Object { -not $map.ContainsKey($_) }
$stale    = $map.Keys | Where-Object { $_ -notin $onDisk }

if ($unmapped) { Write-Warning "wiki pages with no mapping (will be SKIPPED):`n  $($unmapped -join "`n  ")" }
if ($stale)    { Write-Warning "mapping rows with no matching wiki page:`n  $($stale -join "`n  ")" }

# --- build the stubs --------------------------------------------------------
function New-Stub {
    param([string]$Name, [hashtable]$Target)

    $title = ($Name -replace '-', ' ')
    $sb = [System.Text.StringBuilder]::new()

    if ($Target.Core -eq '(root)') {
        [void]$sb.AppendLine("# Moved")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("The documentation has moved to a new site:")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("## [$BaseUrl/]($BaseUrl/)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("This wiki is no longer maintained.")
        return $sb.ToString()
    }

    [void]$sb.AppendLine("# Moved")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("This page has moved to the new documentation site.")
    [void]$sb.AppendLine()

    $hasCore = $Target.Core -ne '-' -and $Target.Core
    $hasNet  = $Target.AspNet -ne '-' -and $Target.AspNet

    if ($hasCore -and $hasNet) {
        [void]$sb.AppendLine("- **ASP.NET Core** &rarr; [$title]($BaseUrl/$($Target.Core))")
        [void]$sb.AppendLine("- **ASP.NET Web API** &rarr; [$title]($BaseUrl/$($Target.AspNet))")
    } elseif ($hasCore) {
        [void]$sb.AppendLine("**[$title]($BaseUrl/$($Target.Core))** (ASP.NET Core)")
    } elseif ($hasNet) {
        [void]$sb.AppendLine("**[$title]($BaseUrl/$($Target.AspNet))** (ASP.NET Web API)")
    } else {
        throw "no target for $Name"
    }

    [void]$sb.AppendLine()
    [void]$sb.AppendLine("---")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("This wiki is no longer maintained. Browse the full documentation at <$BaseUrl/>.")
    return $sb.ToString()
}

$written = 0
foreach ($p in $pages) {
    $name = [System.IO.Path]::GetFileNameWithoutExtension($p.Name)
    if (-not $map.ContainsKey($name)) { continue }

    $stub = New-Stub -Name $name -Target $map[$name]

    if ($Apply) {
        # LF endings, no BOM - matches how GitHub stores wiki content
        $utf8 = [System.Text.UTF8Encoding]::new($false)
        [System.IO.File]::WriteAllText($p.FullName, ($stub -replace "`r`n", "`n"), $utf8)
    } else {
        Write-Output "--- $($p.Name) ".PadRight(72, '-')
        Write-Output $stub.TrimEnd()
        Write-Output ''
    }
    $written++
}

Write-Output ''
if ($Apply) {
    Write-Output "rewrote $written page(s) in $WikiPath"
    Write-Output ''
    Write-Output 'Review, then push yourself:'
    Write-Output "  cd $WikiPath"
    Write-Output '  git diff --stat'
    Write-Output '  git add -A && git commit -m "Redirect wiki to https://dotnet.github.io/aspnet-api-versioning"'
    Write-Output '  git push'
} else {
    Write-Output "DRY RUN - $written page(s) would be rewritten. Re-run with -Apply to write them."
}
