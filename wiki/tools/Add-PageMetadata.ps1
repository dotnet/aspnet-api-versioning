<#
.SYNOPSIS
Injects per-page description, canonical, and Open Graph URL metadata into a built mdBook site.

.DESCRIPTION
mdBook's description is book-level only: [book] description in book.toml applies to every page,
and the HTML renderer exposes no per-page equivalent. A preprocessor cannot supply one either --
mdBook hands a preprocessor (PreprocessorContext, Book) and takes back only Book, so it can
rewrite chapter content but not the config the renderer reads for the page head.

So pages declare their own description as an HTML comment in the markdown:

    <!-- description: Route requests to the right API version using the URL path. -->

Markdown passes the comment through verbatim, it renders invisibly, and mdBook's search indexer
ignores it. Crucially the comment lives in the *including* page rather than the shared partial,
so the aspnet/ and aspnet-core/ pages that share a body via {{#include ../shared/...}} still get
distinct descriptions.

This script runs after `mdbook build`. For every page it lifts that comment into <head> as
name="description", og:description, and twitter:description, and derives the absolute page URL for
og:url and <link rel="canonical"> -- which the Handlebars template cannot do, because mdBook
registers no string helpers and {{path}} is the source .md path.

Rewriting is idempotent: existing tags it owns are removed before the fresh ones are inserted, so
running twice is harmless.

.PARAMETER Book
Path to the generated book directory (the mdbook build output).

.PARAMETER BaseUrl
Absolute origin the site is served from, used to build og:url and canonical.

.PARAMETER Require
Fail the build when a page has no description comment. Off by default so descriptions can be
adopted a page at a time; pages without one simply get no description tags.

.EXAMPLE
  ./Add-PageMetadata.ps1 -Book ./wiki/book
  ./Add-PageMetadata.ps1 -Book ./wiki/book -Require
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$Book,
    [string]$BaseUrl = 'https://dotnet.github.io/aspnet-api-versioning',
    [switch]$Require
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $Book)) { throw "book directory not found: $Book" }
$Book = (Resolve-Path $Book).Path
$BaseUrl = $BaseUrl.TrimEnd('/')

$pages = Get-ChildItem -LiteralPath $Book -Recurse -Filter *.html -File
if (-not $pages) { throw "no HTML found under $Book - did mdbook build run?" }

# Recommended upper bound before search engines truncate the snippet.
$maxLength = 160

$missing = New-Object System.Collections.Generic.List[string]
$long = New-Object System.Collections.Generic.List[object]
$written = 0

foreach ($p in $pages) {
    $rel = $p.FullName.Substring($Book.Length).TrimStart('\', '/').Replace('\', '/')

    # 404.html is served for arbitrary URLs, so no single canonical applies. print.html
    # concatenates every chapter, which would pick up the first page's description.
    # toc.html is the sidebar fragment mdBook generates, not a navigable page.
    if ($rel -eq '404.html' -or $rel -eq 'print.html' -or $rel -eq 'toc.html') { continue }

    $html = Get-Content -Raw -LiteralPath $p.FullName

    # Prefer the directory form so a page has one canonical spelling, not two.
    if ($rel -eq 'index.html') {
        $url = "$BaseUrl/"
    } elseif ($rel.EndsWith('/index.html')) {
        $url = "$BaseUrl/" + $rel.Substring(0, $rel.Length - 'index.html'.Length)
    } else {
        $url = "$BaseUrl/$rel"
    }

    $description = $null
    $m = [regex]::Match($html, '<!--\s*description:\s*(?<text>[\s\S]*?)\s*-->')
    if ($m.Success) {
        # Collapse any wrapping the author used to keep the source line readable.
        $description = [regex]::Replace($m.Groups['text'].Value, '\s+', ' ').Trim()
    }

    if ([string]::IsNullOrWhiteSpace($description)) {
        $missing.Add($rel)
        $description = $null
    } elseif ($description.Length -gt $maxLength) {
        $long.Add([pscustomobject]@{ Page = $rel; Length = $description.Length })
    }

    # --- drop the tags this script owns, so a re-run replaces rather than duplicates ---
    $html = [regex]::Replace($html, '[ \t]*<meta\s+name="description"[^>]*>\r?\n?', '')
    $html = [regex]::Replace($html, '[ \t]*<meta\s+property="og:description"[^>]*>\r?\n?', '')
    $html = [regex]::Replace($html, '[ \t]*<meta\s+name="twitter:description"[^>]*>\r?\n?', '')
    $html = [regex]::Replace($html, '[ \t]*<meta\s+property="og:url"[^>]*>\r?\n?', '')
    $html = [regex]::Replace($html, '[ \t]*<link\s+rel="canonical"[^>]*>\r?\n?', '')

    $tags = New-Object System.Collections.Generic.List[string]
    if ($description) {
        $escaped = [System.Net.WebUtility]::HtmlEncode($description)
        $tags.Add("<meta name=""description"" content=""$escaped"">")
        $tags.Add("<meta property=""og:description"" content=""$escaped"">")
        $tags.Add("<meta name=""twitter:description"" content=""$escaped"">")
    }
    $tags.Add("<meta property=""og:url"" content=""$url"">")
    $tags.Add("<link rel=""canonical"" href=""$url"">")

    $block = ($tags | ForEach-Object { "        $_" }) -join "`n"

    if ($html -notmatch '</head>') { throw "no </head> in $rel" }
    $html = [regex]::Replace($html, '</head>', "$block`n    </head>", 1)

    # -NoNewline: the content already carries its own trailing newline.
    Set-Content -LiteralPath $p.FullName -Value $html -Encoding utf8NoBOM -NoNewline
    $written++
}

Write-Output "wrote metadata to $written page(s); base url $BaseUrl"

if ($long.Count -gt 0) {
    Write-Output ''
    Write-Output "LONG DESCRIPTIONS (over $maxLength chars, will be truncated in results): $($long.Count)"
    foreach ($l in ($long | Sort-Object -Property Length -Descending)) {
        Write-Output ("  {0} ({1} chars)" -f $l.Page, $l.Length)
    }
}

if ($missing.Count -gt 0) {
    Write-Output ''
    Write-Output "PAGES WITHOUT A DESCRIPTION: $($missing.Count)"
    Write-Output '  (add <!-- description: ... --> to the page markdown)'
    foreach ($x in ($missing | Sort-Object)) { Write-Output "  $x" }
    if ($Require) { exit 1 }
}
