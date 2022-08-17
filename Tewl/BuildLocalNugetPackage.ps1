$ErrorActionPreference = "Stop"

$now = Get-Date
$version = [string]::Format("{0}.{1}.{2}.{3}$OctoVersionSuffix",$now.Year, $now.Month, $now.Day,$now.Hour*100+$now.Minute)
echo "Version is $version"

nuget.exe pack Tewl.nuspec -Prop Configuration=Debug -Version $version -Suffix alpha