# Phase 1: XNA 3.0 → 4.0 / MonoGame API conversion script
# Applies mechanical transformations to all .cs files in Source/

$src = Join-Path $PSScriptRoot "Source"
$files = Get-ChildItem -LiteralPath $src -Recurse -Filter "*.cs"

Write-Host "Found $($files.Count) .cs files"

function Replace-InFile($path, $old, $new) {
    $content = [System.IO.File]::ReadAllText($path)
    if ($content.Contains($old)) {
        $content = $content.Replace($old, $new)
        [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
        return $true
    }
    return $false
}

function Replace-RegexInFile($path, $pattern, $replacement) {
    $content = [System.IO.File]::ReadAllText($path)
    if ($content -match $pattern) {
        $content = [regex]::Replace($content, $pattern, $replacement)
        [System.IO.File]::WriteAllText($path, $content, [System.Text.Encoding]::UTF8)
        return $true
    }
    return $false
}

# ============================================================
# 1. Namespace changes
# ============================================================
Write-Host "=== 1. Namespace changes ==="

# Remove GamerServices using (replaced with System.IO for storage)
foreach ($f in $files) {
    $r = Replace-InFile $f.FullName "using Microsoft.Xna.Framework.GamerServices;" ""
    if ($r) { Write-Host "  Removed GamerServices from $($f.Name)" }
}

# Remove Storage using (replaced with System.IO)
foreach ($f in $files) {
    $r = Replace-InFile $f.FullName "using Microsoft.Xna.Framework.Storage;" ""
    if ($r) { Write-Host "  Removed Storage from $($f.Name)" }
}

# ============================================================
# 2. Effect API changes
# ============================================================
Write-Host "=== 2. Effect API changes ==="

foreach ($f in $files) {
    $changed = $false
    $content = [System.IO.File]::ReadAllText($f.FullName)

    # Remove effect.Begin() and effect.End()
    if ($content -match '(?m)^\s*effect\.Begin\(\);?\s*$') {
        $content = [regex]::Replace($content, '(?m)^\s*effect\.Begin\(\);?\s*$', '', 'Singleline')
        $changed = $true
    }
    if ($content -match '(?m)^\s*effect\.End\(\);?\s*$') {
        $content = [regex]::Replace($content, '(?m)^\s*effect\.End\(\);?\s*$', '', 'Singleline')
        $changed = $true
    }
    # Replace techniques[0].Passes[0].Begin() with Pass.Apply()
    if ($content -match 'effect\.Techniques\[0\]\.Passes\[0\]\.Begin\(\)') {
        $content = $content.Replace("effect.Techniques[0].Passes[0].Begin()", "effect.CurrentTechnique.Passes[0].Apply()")
        $changed = $true
    }
    # Remove techniques[0].Passes[0].End()
    if ($content -match 'effect\.Techniques\[0\]\.Passes\[0\]\.End\(\)') {
        $content = $content.Replace("effect.Techniques[0].Passes[0].End()", "")
        $changed = $true
    }
    if ($changed) {
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Effect API changes in $($f.Name)"
    }
}

# ============================================================
# 3. RenderState → State objects
# ============================================================
Write-Host "=== 3. RenderState → State objects ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    $changed = $false

    if ($content -match 'device\.RenderState\.CullMode\s*=\s*CullMode\.CullCounterClockwiseFace') {
        $content = $content.Replace("device.RenderState.CullMode = CullMode.CullCounterClockwiseFace", 'device.RasterizerState = RasterizerState.CullCounterClockwise')
        $changed = $true
    }
    if ($content -match 'device\.RenderState\.CullMode\s*=\s*CullMode\.None') {
        $content = $content.Replace("device.RenderState.CullMode = CullMode.None", 'device.RasterizerState = RasterizerState.CullNone')
        $changed = $true
    }
    if ($content -match 'device\.RenderState\.DepthBufferEnable\s*=\s*true') {
        $content = $content.Replace("device.RenderState.DepthBufferEnable = true", 'device.DepthStencilState = DepthStencilState.Default')
        $changed = $true
    }
    if ($content -match 'device\.RenderState\.DepthBufferWriteEnable\s*=\s*true') {
        $content = $content.Replace("device.RenderState.DepthBufferWriteEnable = true", '')
        $changed = $true
    }
    if ($changed) {
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  RenderState → state objects in $($f.Name)"
    }
}

# ============================================================
# 4. VertexDeclaration assignment removal (device.VertexDeclaration = ...)
# ============================================================
Write-Host "=== 4. Remove device.VertexDeclaration assignments ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'device\.VertexDeclaration\s*=') {
        $content = [regex]::Replace($content, '(?m)^\s*device\.VertexDeclaration\s*=.*;\s*$', '', 'Singleline')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Removed device.VertexDeclaration in $($f.Name)"
    }
}

# ============================================================
# 5. Vertices[0].SetSource → SetVertexBuffer
# ============================================================
Write-Host "=== 5. Vertices[0].SetSource → SetVertexBuffer ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'Vertices\[0\]\.SetSource\(') {
        # XNA 3.0: device.Vertices[0].SetSource(vertexBuffer, offset, stride);
        # XNA 4.0: device.SetVertexBuffer(vertexBuffer);
        $content = [regex]::Replace($content, 'device\.Vertices\[0\]\.SetSource\(([^,]+),.*?\)', 'device.SetVertexBuffer($1)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  SetSource→SetVertexBuffer in $($f.Name)"
    }
}

# Special case: Xenocide.cs with null SetSource
$fXenocide = Get-ChildItem -LiteralPath $src -Filter "Xenocide.cs" -Recurse
if ($fXenocide) {
    $content = [System.IO.File]::ReadAllText($fXenocide[0].FullName)
    if ($content -match 'SetSource\(null, 0, 0\)') {
        $content = $content.Replace("graphics.GraphicsDevice.Vertices[0].SetSource(null, 0, 0)", "")
        [System.IO.File]::WriteAllText($fXenocide[0].FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Removed null SetSource in Xenocide.cs"
    }
}

# ============================================================
# 6. device.Indices = → device.SetIndices()
# ============================================================
Write-Host "=== 6. device.Indices = → device.SetIndices() ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'device\.Indices\s*=') {
        $content = [regex]::Replace($content, 'device\.Indices\s*=\s*(\w+)', 'device.SetIndices($1)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  device.Indices→SetIndices in $($f.Name)"
    }
}

# ============================================================
# 7. VertexBuffer constructor: remove typeof(...) → use vertex declaration
# ============================================================
Write-Host "=== 7. VertexBuffer constructor ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'new VertexBuffer\(') {
        # XNA 3.0: new VertexBuffer(device, typeof(VertexPositionNormalTexture), count, BufferUsage.WriteOnly)
        # XNA 4.0: new VertexBuffer(device, VertexPositionNormalTexture.VertexDeclaration, count, BufferUsage.WriteOnly)
        $content = [regex]::Replace($content, 
            'new VertexBuffer\(device,\s*typeof\((\w+)\),\s*(\d+),\s*BufferUsage\.(\w+)\)',
            'new VertexBuffer(device, $1.VertexDeclaration, $2, BufferUsage.$3)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  VertexBuffer ctor in $($f.Name)"
    }
}

# ============================================================
# 8. IndexBuffer constructor: typeof(short) → SixteenBits, typeof(int) → ThirtyTwoBits
# ============================================================
Write-Host "=== 8. IndexBuffer constructor ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'new IndexBuffer\(') {
        $content = $content.Replace('new IndexBuffer(device, typeof(short),', 'new IndexBuffer(device, IndexElementSize.SixteenBits,')
        $content = $content.Replace('new IndexBuffer(device, typeof(int),', 'new IndexBuffer(device, IndexElementSize.ThirtyTwoBits,')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  IndexBuffer ctor in $($f.Name)"
    }
}

# ============================================================
# 9. BasicEffect(device, null) → BasicEffect(device)
# ============================================================
Write-Host "=== 9. BasicEffect constructor ==="

foreach ($f in $files) {
    $r = Replace-InFile $f.FullName "new BasicEffect(device, null)" "new BasicEffect(device)"
    if ($r) { Write-Host "  BasicEffect ctor in $($f.Name)" }
}

# ============================================================
# 10. Texture2D.FromFile → Texture2D.FromStream
# ============================================================
Write-Host "=== 10. Texture2D.FromFile → FromStream ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'Texture2D\.FromFile\(') {
        # Texture2D.FromFile(device, path) → Texture2D.FromStream(device, File.OpenRead(path))
        $content = [regex]::Replace($content, 
            'Texture2D\.FromFile\(([^,]+),\s*@"([^"]+)"\)',
            'using (var stream = File.OpenRead(@"$2")) { Texture2D.FromStream($1, stream) }')
        # Also handle non-verbatim strings
        $content = [regex]::Replace($content, 
            'Texture2D\.FromFile\(([^,]+),\s*"([^"]+)"\)',
            'using (var stream = File.OpenRead("$2")) { Texture2D.FromStream($1, stream) }')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  Texture2D.FromFile→FromStream in $($f.Name)"
    }
}

# ============================================================
# 11. SpriteBatch constructor - remove GraphicsDevice arg if needed
#    Actually SpriteBatch(device) is valid in 4.0 too - leave as-is
# ============================================================
Write-Host "=== 11. SpriteBatch constructor (no change needed) ==="

# ============================================================
# 12. Fix CullMode enum values
# ============================================================
Write-Host "=== 12. CullMode enum ==="

foreach ($f in $files) {
    $r = Replace-InFile $f.FullName "CullMode.CullCounterClockwiseFace" "CullMode.CullCounterClockwise"
    if ($r) { Write-Host "  CullMode in $($f.Name)" }
}

# ============================================================
# 13. VertexDeclaration constructor: remove device arg
# ============================================================
Write-Host "=== 13. VertexDeclaration constructor ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    if ($content -match 'new VertexDeclaration\(') {
        # XNA 3.0: new VertexDeclaration(device, elements) or new VertexDeclaration(elements)
        # XNA 4.0: new VertexDeclaration(elements)  -- remove device param
        # Match: new VertexDeclaration(device, elements) with various whitespace
        $content = [regex]::Replace($content, 
            'new VertexDeclaration\(([^,]+),\s*(VertexPosition\w+\.VertexElements|VertexElement\[\])\)',
            'new VertexDeclaration($2)')
        [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "  VertexDeclaration ctor in $($f.Name)"
    }
}

# ============================================================
# 14. Remove empty lines left by removals (cleanup)
# ============================================================
Write-Host "=== 14. Cleanup empty lines ==="

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    # Remove lines that are now just whitespace (after removal of statements)
    $content = [regex]::Replace($content, '(?m)^\s*\n', '')
    [System.IO.File]::WriteAllText($f.FullName, $content, [System.Text.Encoding]::UTF8)
}

Write-Host ""
Write-Host "Migration script complete!"
