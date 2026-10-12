param([string]$Dotnet='dotnet',[string]$Iscc='ISCC.exe')
$ErrorActionPreference='Stop'
Push-Location $PSScriptRoot
try {
 & $Dotnet run --project tests -c Release
 if($LASTEXITCODE -ne 0){throw 'Tests failed'}
 & $Dotnet publish src/KeyboardSound -c Release -r win-x64 --self-contained true -o publish
 if($LASTEXITCODE -ne 0){throw 'Publish failed'}
 & $Iscc installer/KeyboardSound.iss
 if($LASTEXITCODE -ne 0){throw 'Installer failed'}
} finally { Pop-Location }
