$lastVersion = "";

$nuspecContent = Get-Content -Path ".\QueueService.Api.Client\QueueService.Api.Client.nuspec" -raw


$versionMatch = [regex]::Match($nuspecContent,'<version>(.*)</version>')

if(!$versionMatch.Success)
{
    throw 'Previous version not found in nuspec file. Make sure it looks like this <version>x.x.x</version>'
}

$lastVersionString = $versionMatch.Groups[1].Value

try{

	$lastVersion = [version]($lastVersionString)

}
catch { 
	Write-Host $_ -BackgroundColor Black -ForegroundColor Red
	Write-Host "Incorrect format of version in nuspec file"
	Read-Host -Prompt "Enter to exit"
	exit
}

$automaticVersionToUse = "1.0.0"
if(![string]::IsNullOrEmpty($lastVersion))
{
	$automaticVersionToUse = "{0}.{1}.{2}" -f $lastVersion.Major, $lastVersion.Minor, ($lastVersion.Build + 1)
}

$manualVersion = Read-Host -Prompt "OPTIONAL: Manual version number (Press enter for automatic - $automaticVersionToUse)"

$newVersion = $automaticVersionToUse;
if(![string]::IsNullOrEmpty($manualVersion))
{
	try{
		$splitManual = [System.Version]::Parse($manualVersion)
	}
	catch { 
		Write-Host $_ -BackgroundColor Black -ForegroundColor Red
		Write-Host "Incorrect format of given version - Should have format Major.Minor.Build so for instance 1.0.2"
		Read-Host -Prompt "Enter to exit"
		exit
	}
	$newVersion = $manualVersion;	
}


$releaseNotes = Read-Host -Prompt "Release notes"

$releaseNotesMatch = [regex]::Match($nuspecContent,'<releaseNotes>.*</releaseNotes>')

if(!$releaseNotesMatch.Success)
{
    throw 'release notes not found in nuspec file. Make sure it looks like this <releaseNotes>xxxxxxxx</releaseNotes>'
}

$lastReleaseNotesString = $releaseNotesMatch.Value

Set-Content -Path ".\QueueService.Api.Client\QueueService.Api.Client.nuspec" -Value $($nuspecContent -replace $("<version>$($lastVersionString)</version>"),$("<version>$($newVersion)</version>") -replace $lastReleaseNotesString,"<releaseNotes>$($releaseNotes)</releaseNotes>")

dotnet build ".\QueueService.Api.Model\QueueService.Api.Model.csproj" --configuration Release
dotnet build ".\QueueService.Api.Client\QueueService.Api.Client.csproj" --configuration Release

..\nuget.exe pack .\QueueService.Api.Client\QueueService.Api.Client.csproj -Properties Configuration=Release

Write-Host "Press enter to push to nuget server (or close to not push)" -BackgroundColor Black -ForegroundColor Cyan
Read-Host

$nugetApiKey = ""

if([System.IO.File]::Exists(".\NugetKey.txt"))
{
	$nugetApiKey = Get-Content -Path .\NugetKey.txt -First 1
}
else 
{
	Write-Host $_ -BackgroundColor Black -ForegroundColor Red
	Write-Host "There must be a NugetKey.txt in the same directory as this build script that contains only the api key to the nuget server"
	Read-Host -Prompt "Enter to exit"
	exit
}

$packages = gci ".\*.nupkg"
foreach ($package in $packages){
	$packageLocation = ".\" + $package.Name
	..\nuget.exe push $packageLocation $nugetApiKey -Source https://api.nuget.org/v3/index.json
	
	Remove-Item –path $packageLocation
}


Read-Host -Prompt "Enter to exit"