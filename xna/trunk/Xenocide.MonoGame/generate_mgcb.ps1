# Generates Content.mgcb from the original XNA 3.0 Content.contentproj
param(
    [string]$ContentProj = "Xenocide\Content\Content.contentproj",
    [string]$OutputMgcb = "Xenocide.MonoGame\Content\Content.mgcb"
)

[xml]$proj = Get-Content $ContentProj
$lines = [System.Collections.Generic.List[string]]::new()

$lines.Add("#----------------------------- Global Properties ----------------------------#")
$lines.Add("/outputDir:bin/`$(Platform)")
$lines.Add("/intermediateDir:obj/`$(Platform)")
$lines.Add("/platform:DesktopGL")
$lines.Add("/config:")
$lines.Add("/profile:Reach")
$lines.Add("/compress:False")
$lines.Add("")
$lines.Add("#-------------------------------- References --------------------------------#")
$lines.Add("")
$lines.Add("#---------------------------------- Content ---------------------------------#")
$lines.Add("")

$items = @()
foreach ($group in $proj.Project.ItemGroup) {
    if ($group.Compile) { $items += $group.Compile }
    if ($group.None) { $items += $group.None }
}

$seen = @{}
foreach ($item in $items) {
    $include = $item.Include
    if ($seen.ContainsKey($include)) { continue }
    $seen[$include] = $true

    $importer = $item.Importer
    $processor = $item.Processor
    $srcRel = $include -replace '\\', '/'

    $lines.Add("#begin $srcRel")

    if ($importer -eq "EffectImporter") {
        $lines.Add("/importer:EffectImporter")
        $lines.Add("/processor:EffectProcessor")
        $lines.Add("/build:$srcRel")
    } elseif ($importer -eq "XImporter") {
        $lines.Add("/importer:XImporter")
        $lines.Add("/processor:ModelProcessor")
        $lines.Add("/build:$srcRel")
    } elseif ($importer -eq "FbxImporter") {
        $lines.Add("/importer:FbxImporter")
        $lines.Add("/processor:ModelProcessor")
        $lines.Add("/build:$srcRel")
    } elseif ($importer -eq "TextureImporter") {
        $lines.Add("/importer:TextureImporter")
        $lines.Add("/processor:TextureProcessor")
        $lines.Add("/build:$srcRel")
    } elseif ($importer -eq "FontDescriptionImporter") {
        $lines.Add("/importer:FontDescriptionImporter")
        $lines.Add("/processor:FontDescriptionProcessor")
        $lines.Add("/build:$srcRel")
    } elseif ($srcRel -match '\.ogg$') {
        # Audio files - use OggImporter with SongProcessor for music
        if ($srcRel -match 'Music/') {
            $lines.Add("/importer:OggImporter")
            $lines.Add("/processor:SongProcessor")
            $lines.Add("/processorParam:Quality=Best")
        } else {
            $lines.Add("/importer:OggImporter")
            $lines.Add("/processor:SoundEffectProcessor")
            $lines.Add("/processorParam:Quality=Best")
        }
        $lines.Add("/build:$srcRel")
    } else {
        # Copy-only (XML data, schemas, layouts, text files)
        $lines.Add("/copy:$srcRel")
    }

    $lines.Add("")
}

$lines | Out-File -FilePath $OutputMgcb -Encoding utf8
Write-Host "Generated $OutputMgcb with $($seen.Count) items"
