cd ..
if( Test-Path "Latest Package" ) { Remove-Item "Latest Package" -Recurse }

$packageId = 'Ewl'
& "Solution Files\nuget" install $packageId -DependencyVersion Ignore -ExcludeVersion -NonInteractive -OutputDirectory "Latest Package" -PackageSaveMode nuspec -PreRelease

cd "Latest Package\$packageId\tools\Development Utility"
& .\EnterpriseWebLibrary.DevelopmentUtility ..\..\..\.. UpdateDependentLogic