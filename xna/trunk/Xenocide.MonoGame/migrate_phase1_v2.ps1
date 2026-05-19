# Phase 1 v2: Safe mechanical replacements only
$src = "Xenocide.MonoGame\Source"
$files = Get-ChildItem -LiteralPath $src -Recurse -Filter "*.cs"

Write-Host "Found $($files.Count) .cs files"

function ReplaceInFile($path, $old, $new) {
    $content = [System.IO.File]::ReadAllText($path)
    if ($content.Contains($old)) {
        $content = $content.Replace($old, $new)
        [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
        return $true
    }
    return $false
}

# ============================================================
# 1. Namespace changes
# ============================================================
Write-Host "=== 1. Namespace changes ==="
foreach ($f in $files) {
    ReplaceInFile $f.FullName "using Microsoft.Xna.Framework.GamerServices;" ""
    ReplaceInFile $f.FullName "using Microsoft.Xna.Framework.Storage;" ""
}

# ============================================================
# 2. Effect API: remove Begin/End lines
# ============================================================
Write-Host "=== 2. Effect API ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $changed = $false
    # Remove effect.Begin(); and effect.End();
    $pattern = '(?m)^\s*(effect\.Begin\(\);?|effect\.End\(\);?)\s*$'
    if ($content -match $pattern) {
        $content = [regex]::Replace($content, $pattern, '', 'Singleline')
        $changed = $true
    }
    # Replace pass.Begin() with .Apply()
    if ($content -match '\.Passes\[\d\]\.Begin\(\)') {
        $content = $content.Replace(".Passes[0].Begin()", ".Passes[0].Apply()")
        $changed = $true
    }
    # Remove pass.End()
    if ($content -match '\.Passes\[\d\]\.End\(\)') {
        $content = $content.Replace(".Passes[0].End()", "")
        $changed = $true
    }
    if ($changed) {
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Effect changes in $($f.Name)"
    }
}

# ============================================================
# 3. RenderState -> state objects
# ============================================================
Write-Host "=== 3. RenderState ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $changed = $false
    if ($content -match 'device\.RenderState\.CullMode\s*=\s*CullMode\.CullCounterClockwiseFace') {
        $content = $content.Replace("device.RenderState.CullMode = CullMode.CullCounterClockwiseFace", "device.RasterizerState = RasterizerState.CullCounterClockwise")
        $changed = $true
    }
    if ($content -match 'device\.RenderState\.CullMode\s*=\s*CullMode\.None') {
        $content = $content.Replace("device.RenderState.CullMode = CullMode.None", "device.RasterizerState = RasterizerState.CullNone")
        $changed = $true
    }
    if ($content -match 'device\.RenderState\.DepthBufferEnable\s*=\s*true') {
        $content = $content.Replace("device.RenderState.DepthBufferEnable = true", "device.DepthStencilState = DepthStencilState.Default")
        $changed = $true
    }
    if ($content -match 'device\.RenderState\.DepthBufferWriteEnable\s*=\s*true') {
        $content = $content.Replace("device.RenderState.DepthBufferWriteEnable = true", "")
        $changed = $true
    }
    if ($changed) {
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  RenderState in $($f.Name)"
    }
}

# ============================================================
# 4. device.VertexDeclaration = -> remove line
# ============================================================
Write-Host "=== 4. Remove device.VertexDeclaration assignments ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match '(?m)^\s*device\.VertexDeclaration\s*=.*;\s*$') {
        $content = [regex]::Replace($content, '(?m)^\s*device\.VertexDeclaration\s*=.*;\s*$', '', 'Singleline')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Removed vertex decl in $($f.Name)"
    }
}

# ============================================================
# 5. Vertices[0].SetSource -> SetVertexBuffer
# ============================================================
Write-Host "=== 5. SetSource -> SetVertexBuffer ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'device\.Vertices\[0\]\.SetSource\(') {
        $content = [regex]::Replace($content, 'device\.Vertices\[0\]\.SetSource\(([^,]+),.*?\)', 'device.SetVertexBuffer($1)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  SetSource in $($f.Name)"
    }
}

# ============================================================
# 6. device.Indices = -> device.SetIndices()
# ============================================================
Write-Host "=== 6. device.Indices -> SetIndices ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'device\.Indices\s*=') {
        $content = [regex]::Replace($content, 'device\.Indices\s*=\s*(\w+)', 'device.SetIndices($1)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  SetIndices in $($f.Name)"
    }
}

# ============================================================
# 7. VertexBuffer constructor: typeof -> .VertexDeclaration
# ============================================================
Write-Host "=== 7. VertexBuffer constructor ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'new VertexBuffer\(') {
        $content = [regex]::Replace($content,
            'new VertexBuffer\(device,\s*typeof\((\w+)\),\s*(\d+),\s*BufferUsage\.(\w+)\)',
            'new VertexBuffer(device, $1.VertexDeclaration, $2, BufferUsage.$3)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  VertexBuffer in $($f.Name)"
    }
}

# ============================================================
# 8. IndexBuffer constructor: typeof -> IndexElementSize
# ============================================================
Write-Host "=== 8. IndexBuffer constructor ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'new IndexBuffer\(') {
        $content = $content.Replace('new IndexBuffer(device, typeof(short),', 'new IndexBuffer(device, IndexElementSize.SixteenBits,')
        $content = $content.Replace('new IndexBuffer(device, typeof(int),', 'new IndexBuffer(device, IndexElementSize.ThirtyTwoBits,')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  IndexBuffer in $($f.Name)"
    }
}

# ============================================================
# 9. BasicEffect(device, null) -> BasicEffect(device)
# ============================================================
Write-Host "=== 9. BasicEffect constructor ==="
foreach ($f in $files) {
    $r = ReplaceInFile $f.FullName "new BasicEffect(device, null)" "new BasicEffect(device)"
    if ($r) { Write-Host "  BasicEffect in $($f.Name)" }
}

# ============================================================
# 10. CullMode.CullCounterClockwiseFace -> CullMode.CullCounterClockwise
# ============================================================
Write-Host "=== 10. CullMode enum ==="
foreach ($f in $files) {
    $r = ReplaceInFile $f.FullName "CullMode.CullCounterClockwiseFace" "CullMode.CullCounterClockwise"
    if ($r) { Write-Host "  CullMode in $($f.Name)" }
}

# ============================================================
# 11. VertexDeclaration constructor: remove device param
# ============================================================
Write-Host "=== 11. VertexDeclaration constructor ==="
foreach ($f in $files) {
    # Match: new VertexDeclaration(device, elements)
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'new VertexDeclaration\(device,') {
        $content = $content.Replace('new VertexDeclaration(device,', 'new VertexDeclaration(')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  VertexDeclaration in $($f.Name)"
    }
}

# ============================================================
# 12. SpriteBatch.Begin: SpriteBlendMode -> SpriteSortMode, etc.
# ============================================================
Write-Host "=== 12. SpriteBatch.Begin fixes ==="
foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $changed = $false
    if ($content -match 'SpriteBlendMode\.') {
        $content = $content.Replace("SpriteBlendMode.AlphaBlend", "SpriteSortMode.Immediate")
        $content = $content.Replace("SpriteBlendMode.Additive", "SpriteSortMode.Immediate")
        $changed = $true
    }
    if ($changed) {
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  SpriteBatch.Begin in $($f.Name)"
    }
}

Write-Host "`nDone with safe replacements. Now fix these manually:"
Write-Host "  1. Texture2D.FromFile in EarthGlobe.cs, SkyBox.cs, TextureAtlas.cs, EquipSoldierScene.cs, GeoBitmap.cs"
Write-Host "  2. VertexDeclaration constructor in SkyBox.cs (custom inline VertexElement[])"
Write-Host "  3. StorageContainer in FileUtil.cs, GameOptions.cs, StaticTables.cs, CreditsScreen.cs, LoadSaveGameScreen.cs"
Write-Host "  4. Draw methods (verify they weren't emptied)"
