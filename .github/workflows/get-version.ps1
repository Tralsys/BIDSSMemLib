param(
  [string]$ProjectName
)

[xml]$csproj = (Get-Content ./$ProjectName/$ProjectName.csproj)

[string]$Version = ([xml]$csproj).Project.PropertyGroup.Version

if ([string]::IsNullOrEmpty($Version)) {
  Write-Output "Default version was set"
  $Version = '1.0.0'
}
else {
  $VersionCore, $PreReleaseEtc = $Version.Split('-', 2)
  $Major, $Minor, $Build, $Revision = $VersionCore.Split('.', 4)

  if ([string]::IsNullOrEmpty($Major)) {
    Write-Error "Major Version Not Found"
    exit 1
  }

  $Minor = [string]::IsNullOrEmpty($Minor) ? 0 : [int]$Minor
  $Build = [string]::IsNullOrEmpty($Build) ? 0 : [int]$Build
  $Revision = [string]::IsNullOrEmpty($Revision) ? 0 : [int]$Revision

  if (![string]::IsNullOrEmpty($PreReleaseEtc)) {
    $PreReleaseEtc = "-$PreReleaseEtc"
  }

  $Revision = ($Revision -le 0) ? '' : ".$Revision"
  
  $Version = "$Major.$Minor.$Build$Revision$PreReleaseEtc"
}

Write-Output "Version ... '$Version'"
Write-Output "VERSION=$Version" >> $env:GITHUB_OUTPUT

[string]$AssemblyName = ([xml]$csproj).Project.PropertyGroup.AssemblyName
if ([string]::IsNullOrEmpty($AssemblyName)) {
  $AssemblyName = $ProjectName
}
else {
  $AssemblyName = $AssemblyName.Trim()
}

Write-Output "Assembly Name ... '$AssemblyName'"
Write-Output "ASSEMBLY_NAME=$Assemblyname" >> $env:GITHUB_OUTPUT
