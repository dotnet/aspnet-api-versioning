# Wiki Tools

Tooling for the legacy GitHub wiki, which was superseded by
<https://dotnet.github.io/aspnet-api-versioning>.

The wiki is intentionally kept online rather than disabled. Articles, training material, and
blog posts link to it, and those links should keep resolving. Every page has been replaced with
a short stub pointing at the equivalent page on the new site.

## Why stubs instead of redirects

GitHub wikis cannot issue an HTTP redirect. There is no `_redirects` or `.htaccess`, and
GitHub's Markdown sanitizer strips `<meta http-equiv="refresh">`, `<script>`, and
`<link rel="canonical">`. A stub page is the only available "soft redirect": it keeps human
visitors moving to the right place, and search engines derank the thin duplicate pages over
time. It does not transfer ranking signal the way a 301 would.

## Files

| File | Purpose |
|:-----|:--------|
| `wiki-redirect-map.tsv` | Old wiki page &rarr; new site path(s). Tab-separated. |
| `make-wiki-stubs.ps1`   | Rewrites every wiki page into a stub from that map. |
| `Test-Links.ps1`        | Validates relative links and anchors in the built site. |

## Link checking

`Test-Links.ps1` runs in CI as the **Check Links** step of `.github/workflows/gh-pages.yml`,
between the build and the deploy, so a broken link fails the workflow instead of shipping.

mdBook rewrites `.md` links to `.html` but never verifies the target exists &mdash; a link to a
renamed or deleted page builds cleanly and 404s in production. The script resolves every
relative `href`/`src` in the generated HTML against the filesystem and checks that each
`#fragment` matches a real `id` on the target page.

Passing `-MapPath` also verifies every target in `wiki-redirect-map.tsv`. If the site is
restructured, the build fails with the stale entries listed, rather than the legacy wiki
quietly pointing at pages that no longer exist.

```powershell
cd wiki
mdbook build
./tools/Test-Links.ps1 -Book ./book -MapPath ./tools/wiki-redirect-map.tsv
```

It exits non-zero when anything is broken, and reports `MISSING FILE` or `MISSING ANCHOR` per
link. mdBook's `404.html` is skipped: its links are deliberately site-absolute via `<base href>`
and cannot be resolved on disk.

## Usage

The wiki is a git repository. Clone it with **full history** &mdash; a shallow clone can be
rejected on push with `shallow update not allowed`:

```powershell
git clone https://github.com/dotnet/aspnet-api-versioning.wiki.git

# preview (default: writes nothing)
./make-wiki-stubs.ps1 -WikiPath ./aspnet-api-versioning.wiki -MapPath ./wiki-redirect-map.tsv

# write the stubs
./make-wiki-stubs.ps1 -WikiPath ./aspnet-api-versioning.wiki -MapPath ./wiki-redirect-map.tsv -Apply
```

The script never pushes. Review the diff and push yourself:

```powershell
cd ./aspnet-api-versioning.wiki
git diff --stat
git add -A && git commit -m "Redirect wiki to https://dotnet.github.io/aspnet-api-versioning"
git push
```

## Maintaining the map

If the documentation site is restructured, update `wiki-redirect-map.tsv` and re-run the
script. The map is reconciled against the wiki on every run: pages present in the wiki but
missing from the map are reported and skipped, and map rows with no matching page are reported
as stale.

Columns are `OldPage`, `AspNetCorePath`, `AspNetPath`. Use `-` when a topic exists for only one
platform, and `(root)` to point at the site root. Paths are relative to the site base URL and
may include a fragment, for example `aspnet-core/docs/odata-options.html#query-options`.

The separator must be a literal tab. An editor that expands tabs to spaces will cause a
`malformed row` error rather than a silent misfire.

## Previewing a stub

Local Markdown previewers do not use GitHub's sanitizer. To see exactly what the wiki will
render:

```powershell
$md = Get-Content -Raw ./aspnet-api-versioning.wiki/Home.md
$body = @{ text = $md; mode = 'gfm' } | ConvertTo-Json -Compress
$tmp = [System.IO.Path]::GetTempFileName()
[System.IO.File]::WriteAllText($tmp, $body, [System.Text.UTF8Encoding]::new($false))
gh api --method POST /markdown --input $tmp
```
