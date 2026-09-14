# Captures the Xenocide game window to a PNG for UI review.
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File tools/screenshot.ps1 [-Name main-menu]
#
# The game must already be running. Output goes to Screenshots/<timestamp>[-name].png.

param(
    [string]$Name = ""
)

Add-Type -AssemblyName System.Drawing

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class Win32Rect {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);
}
"@

$proc = Get-Process -Name "Xenocide.MonoGame" -ErrorAction SilentlyContinue |
        Where-Object { $_.MainWindowHandle -ne 0 } |
        Select-Object -First 1

if (-not $proc) {
    Write-Error "Xenocide.MonoGame is not running (no window found). Launch it first."
    exit 1
}

$rect = New-Object Win32Rect+RECT
[void][Win32Rect]::GetWindowRect($proc.MainWindowHandle, [ref]$rect)
$width = $rect.Right - $rect.Left
$height = $rect.Bottom - $rect.Top

$dir = Join-Path (Split-Path $PSScriptRoot -Parent) "Screenshots"
New-Item -ItemType Directory -Force -Path $dir | Out-Null

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$suffix = if ($Name) { "-$Name" } else { "" }
$out = Join-Path $dir "$stamp$suffix.png"

$bmp = New-Object System.Drawing.Bitmap $width, $height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, (New-Object System.Drawing.Size $width, $height))
$bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$bmp.Dispose()

Write-Output "Saved $out (${width}x${height})"
