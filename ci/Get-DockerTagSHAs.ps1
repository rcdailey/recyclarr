$output = skopeo --override-os linux list-tags docker://ghcr.io/recyclarr/recyclarr
$tags = ($output | convertfrom-json).Tags |where-object { $_ -notmatch "dev" }

$shas = @()
foreach ($tag in $tags) {
    # "Obtain SHAs for tag $tag"
    $manifests = docker manifest inspect ghcr.io/recyclarr/recyclarr:$tag | ConvertFrom-Json
    $manifests.manifests | ForEach-Object { $shas += $_.digest }
}

Write-Host $($shas | Select-Object -Unique)
