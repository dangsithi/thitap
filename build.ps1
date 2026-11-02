# Build ThiTap bằng csc.exe có sẵn trong Windows (.NET Framework 4.x) — không cần cài .NET SDK.
#
#   .\build.ps1            build app        -> bin\ThiTap.exe
#   .\build.ps1 -Test      build và chạy bộ kiểm thử lõi Telex
#   .\build.ps1 -Run       build rồi chạy app

param(
    [switch]$Test,
    [switch]$Run
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path $csc)) {
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
}
if (-not (Test-Path $csc)) {
    throw 'Không tìm thấy csc.exe của .NET Framework 4.x.'
}

# csc.exe đọc mã nguồn theo bảng mã ANSI khi file không có BOM, nên phải bảo đảm mọi file .cs là UTF-8 có BOM.
Get-ChildItem -Path (Join-Path $root 'src'), (Join-Path $root 'tools') -Recurse -Filter *.cs |
    ForEach-Object {
        $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
        $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
        if (-not $hasBom) {
            $text = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8)
            [System.IO.File]::WriteAllText($_.FullName, $text, (New-Object System.Text.UTF8Encoding($true)))
        }
    }

$bin = Join-Path $root 'bin'
New-Item -ItemType Directory -Force -Path $bin | Out-Null

$engineSources = @(Get-ChildItem (Join-Path $root 'src\Engine') -Filter *.cs | ForEach-Object { $_.FullName })
$inputSources = @(Get-ChildItem (Join-Path $root 'src\Input') -Filter *.cs | ForEach-Object { $_.FullName })
$appSources = @(Get-ChildItem (Join-Path $root 'src') -Filter *.cs | ForEach-Object { $_.FullName })

$references = @(
    '/r:System.dll'
    '/r:System.Core.dll'
    '/r:System.Drawing.dll'
    '/r:System.Windows.Forms.dll'
)

if ($Test) {
    $testExe = Join-Path $bin 'TelexTest.exe'
    & $csc /nologo /target:exe /out:$testExe $references $engineSources (Join-Path $root 'tools\TelexTest.cs')
    if ($LASTEXITCODE -ne 0) { throw "Biên dịch bộ kiểm thử thất bại (mã $LASTEXITCODE)." }

    & $testExe
    exit $LASTEXITCODE
}

$exe = Join-Path $bin 'ThiTap.exe'

# Khai báo hiểu DPI (màn hình 125% / 150% không bị phóng to làm nhoè chữ).
$manifestPath = Join-Path $root 'app.manifest'
if (-not (Test-Path $manifestPath)) { throw "Không tìm thấy $manifestPath." }

# Icon nhúng trong file exe: sinh lại khi chưa có hoặc khi thiết kế icon đã thay đổi.
$iconPath = Join-Path $root 'assets\ThiTap.ico'
$iconSources = @((Join-Path $root 'src\IconDesign.cs'), (Join-Path $root 'tools\MakeIcon.cs'))

$needIcon = -not (Test-Path $iconPath)
if (-not $needIcon) {
    $iconTime = (Get-Item $iconPath).LastWriteTimeUtc
    foreach ($source in $iconSources) {
        if ((Get-Item $source).LastWriteTimeUtc -gt $iconTime) { $needIcon = $true }
    }
}

if ($needIcon) {
    New-Item -ItemType Directory -Force -Path (Join-Path $root 'assets') | Out-Null
    $makeIconExe = Join-Path $bin 'MakeIcon.exe'
    & $csc /nologo /target:exe /out:$makeIconExe /r:System.dll /r:System.Drawing.dll (Join-Path $root 'src\IconDesign.cs') (Join-Path $root 'tools\MakeIcon.cs')
    if ($LASTEXITCODE -ne 0) { throw "Không biên dịch được công cụ tạo icon (mã $LASTEXITCODE)." }

    & $makeIconExe $iconPath
    if (-not (Test-Path $iconPath)) { throw "Không tạo được $iconPath." }
}

& $csc /nologo /target:winexe /optimize+ /win32icon:$iconPath /win32manifest:$manifestPath /out:$exe $references $appSources $engineSources $inputSources
if ($LASTEXITCODE -ne 0) { throw "Biên dịch thất bại (mã $LASTEXITCODE)." }

Write-Host "Đã build: $exe"

if ($Run) {
    Start-Process -FilePath $exe
}
