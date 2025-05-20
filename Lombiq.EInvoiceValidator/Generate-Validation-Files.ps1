## Config
$repoUrl = "https://github.com/ConnectingEurope/eInvoicing-EN16931.git"
$eInvoiceRepositoryTargetDir = ".\eInvoicing-EN16931"
$depth = 1

# Remove any previous clone (optional)
if (Test-Path $eInvoiceRepositoryTargetDir) {
    Write-Host "Removing existing directory: $eInvoiceRepositoryTargetDir"
    Remove-Item -Recurse -Force $eInvoiceRepositoryTargetDir
}

# Clone the repo (shallow clone)
Write-Host "Cloning latest release from $repoUrl..."
git clone $repoUrl $eInvoiceRepositoryTargetDir

# Change to repo directory
Set-Location $eInvoiceRepositoryTargetDir

# Fetch latest tag
$latestTag = git describe --tags $(git rev-list --tags --max-count=1)

Write-Host "Checking out latest tag: $latestTag"
git fetch --depth=$depth origin tag $latestTag
git checkout tags/$latestTag

# Go back to original path
Set-Location $PSScriptRoot

Write-Host "Checked out latest release to: $eInvoiceRepositoryTargetDir"


xslt3 "-t" "-xsl:$eInvoiceRepositoryTargetDir/cii/xslt/EN16931-CII-validation.xslt" "-export:SaxonJsStylesheets/EN16931-CII-validation.sef.json" "-nogo" "-relocate:on" "-ns:##html5"
xslt3 "-t" "-xsl:$eInvoiceRepositoryTargetDir/ubl/xslt/EN16931-UBL-validation.xslt" "-export:SaxonJsStylesheets/EN16931-UBL-validation.sef.json" "-nogo" "-relocate:on" "-ns:##html5"

# Copy all cii *.xsd files to SchemaFiles
$ciiXsdFolder = "$eInvoiceRepositoryTargetDir/cii/schema/D16B SCRDM (Subset)/uncoupled clm/CII/uncefact/data/standard"

$ciiXsdFiles = Get-ChildItem -Path $ciiXsdFolder -Filter *.xsd -Recurse
$targetCiiXsdFolder = "$PSScriptRoot/SchemaFiles/CII"
if (-not (Test-Path $targetCiiXsdFolder)) {
    New-Item -ItemType Directory -Path $targetCiiXsdFolder
}
foreach ($file in $ciiXsdFiles) {
    $targetFile = Join-Path -Path $targetCiiXsdFolder -ChildPath $file.Name
    Copy-Item -Path $file.FullName -Destination $targetFile
}

# Get UBL files from http://docs.oasis-open.org/ubl/os-UBL-2.1/UBL-2.1.zip and unzip it
$ublZipUrl = "http://docs.oasis-open.org/ubl/os-UBL-2.1/UBL-2.1.zip"
$ublZipPath = "$PSScriptRoot/UBL-2.1.zip"
if (-not (Test-Path $ublZipPath)) {
    Write-Host "Downloading UBL 2.1 zip file..."
    Invoke-WebRequest -Uri $ublZipUrl -OutFile $ublZipPath
}
if (-not (Test-Path "$PSScriptRoot/UBL-2.1")) {
    Write-Host "Unzipping UBL 2.1 zip file..."
    Expand-Archive -Path $ublZipPath -DestinationPath "$PSScriptRoot/UBL-2.1"
}

# Copy all UBL *.xsd files to SchemaFiles, but keep the folder structure
$ublXsdFolder = "$PSScriptRoot/UBL-2.1/xsd"
$ublXsdFiles = Get-ChildItem -Path $ublXsdFolder -Filter *.xsd -Recurse
$targetUblXsdFolder = "$PSScriptRoot/SchemaFiles/UBL"
if (-not (Test-Path $targetUblXsdFolder)) {
    New-Item -ItemType Directory -Path $targetUblXsdFolder
}

# Copy files recursively but keep folder structure in $ublXsdFolder
foreach ($file in $ublXsdFiles) {
    $relativePath = $file.FullName.Substring($ublXsdFolder.Length).TrimStart('\','/')
    $targetFile = Join-Path -Path $targetUblXsdFolder -ChildPath $relativePath
    $targetDir = Split-Path -Path $targetFile -Parent
    if (-not (Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
    }
    Copy-Item -Path $file.FullName -Destination $targetFile -Force
}

# Cleanup downloaded files and unzipped folders
Remove-Item -Path $ublZipPath -Force
Remove-Item -Path "$PSScriptRoot/UBL-2.1" -Recurse -Force
Remove-Item -Path $eInvoiceRepositoryTargetDir -Recurse -Force
