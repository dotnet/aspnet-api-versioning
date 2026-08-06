<#
.SYNOPSIS
Validates relative links and anchors in a built mdBook site, and optionally the
legacy wiki redirect map.

.DESCRIPTION
mdBook rewrites .md links to .html but does not verify the target exists, so a link to a
page that was renamed or removed builds cleanly and 404s in production. This script walks
the generated HTML, resolves every relative href/src against the filesystem, and verifies
that any #fragment matches a real id on the target page.

When -MapPath is supplied, it also confirms every target in the wiki redirect map still
resolves. If the documentation is restructured, this fails the build instead of silently
leaving the legacy wiki pointing at pages that no longer exist.

Exits 1 when anything is broken so it can gate a deployment.

.PARAMETER Book
Path to the generated book directory (the mdbook build output).

.PARAMETER MapPath
Optional path to wiki-redirect-map.tsv. When given, every mapped target is verified.

.EXAMPLE
  ./Test-Links.ps1 -Book ./wiki/book
  ./Test-Links.ps1 -Book ./wiki/book -MapPath ./wiki/tools/wiki-redirect-map.tsv
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Book,
    [string]$MapPath
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Book)) { throw "book directory not found: $Book" }
$Book = (Resolve-Path $Book).Path

$pages = Get-ChildItem -LiteralPath $Book -Recurse -Filter *.html -File
if (-not $pages) { throw "no HTML found under $Book - did mdbook build run?" }

# --- index every anchor id in the generated site ---------------------------
$ids = @{}
foreach ($p in $pages) {
    $text = Get-Content -Raw -LiteralPath $p.FullName
    $set = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($m in [regex]::Matches($text, '\bid="([^"]+)"')) { [void]$set.Add($m.Groups[1].Value) }
    $ids[$p.FullName.ToLowerInvariant()] = $set
}

$broken = New-Object System.Collections.Generic.List[object]
$seen = [System.Collections.Generic.HashSet[string]]::new()
$checked = 0

foreach ($p in $pages) {
    $relPage = $p.FullName.Substring($Book.Length).TrimStart('\', '/').Replace('\', '/')

    # mdBook's 404 page is deliberately site-absolute via <base href>; its links
    # cannot be resolved on disk and are not a defect.
    if ($relPage -eq '404.html') { continue }

    $text = Get-Content -Raw -LiteralPath $p.FullName
    foreach ($m in [regex]::Matches($text, '(?:href|src)="([^"]+)"')) {
        $url = [System.Net.WebUtility]::HtmlDecode($m.Groups[1].Value)

        # relative links only: skip scheme-qualified, protocol-relative,
        # root-absolute, and same-page anchors
        if ($url -match '^([a-zA-Z][a-zA-Z0-9+.-]*:|//|/|#)') { continue }

        $parts = $url.Split('#', 2)
        $path = [System.Uri]::UnescapeDataString($parts[0])
        $frag = if ($parts.Count -gt 1) { [System.Uri]::UnescapeDataString($parts[1]) } else { '' }
        if ([string]::IsNullOrEmpty($path)) { continue }

        $checked++
        $target = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($p.DirectoryName, $path))

        $why = $null
        if (-not (Test-Path -LiteralPath $target)) {
            $why = 'MISSING FILE'
        } elseif ($frag -and $target.EndsWith('.html')) {
            $key = $target.ToLowerInvariant()
            if (-not ($ids.ContainsKey($key) -and $ids[$key].Contains($frag))) { $why = 'MISSING ANCHOR' }
        }

        if ($why) {
            $k = "$relPage|$url"
            if ($seen.Add($k)) { $broken.Add([pscustomobject]@{ Why = $why; Page = $relPage; Url = $url }) }
        }
    }
}

Write-Output "checked $checked relative link(s) across $($pages.Count) page(s)"

# --- validate the legacy wiki redirect map ---------------------------------
$mapBroken = New-Object System.Collections.Generic.List[object]
if ($MapPath) {
    if (-not (Test-Path -LiteralPath $MapPath)) { throw "map not found: $MapPath" }
    $mapChecked = 0
    foreach ($line in Get-Content -LiteralPath $MapPath) {
        if ($line -match '^\s*(#|$)') { continue }
        $cols = $line -split "`t"
        if ($cols.Count -lt 2) { throw "malformed row (expected 2-3 tab-separated columns): $line" }
        $page = $cols[0].Trim()
        foreach ($t in @($cols[1], $(if ($cols.Count -ge 3) { $cols[2] } else { '-' }))) {
            $t = $t.Trim()
            if ($t -eq '-' -or $t -eq '(root)' -or -not $t) { continue }
            $mapChecked++
            $file = ($t -split '#', 2)[0]
            if (-not (Test-Path -LiteralPath (Join-Path $Book $file))) {
                $mapBroken.Add([pscustomobject]@{ Page = $page; Url = $t })
            }
        }
    }
    Write-Output "checked $mapChecked wiki redirect target(s)"
}

# --- report ----------------------------------------------------------------
$failed = $false

if ($broken.Count -gt 0) {
    $failed = $true
    Write-Output ''
    Write-Output "BROKEN LINKS: $($broken.Count)"
    foreach ($b in ($broken | Sort-Object Why, Page, Url)) {
        Write-Output ("  [{0}] {1} -> {2}" -f $b.Why, $b.Page, $b.Url)
    }
}

if ($mapBroken.Count -gt 0) {
    $failed = $true
    Write-Output ''
    Write-Output "STALE WIKI REDIRECT TARGETS: $($mapBroken.Count)"
    Write-Output '  (update wiki-redirect-map.tsv and re-run make-wiki-stubs.ps1)'
    foreach ($b in $mapBroken) {
        Write-Output ("  {0} -> {1}" -f $b.Page, $b.Url)
    }
}

if ($failed) { exit 1 }

Write-Output 'OK - no broken relative links'
