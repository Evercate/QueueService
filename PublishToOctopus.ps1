$lastVersion = "";


try{

	if([System.IO.File]::Exists(".\BuildVersion.txt"))
	{
		$lastVersion = [version](Get-Content -Path .\BuildVersion.txt -First 1)
	}

}
catch { 
	Write-Host $_ -BackgroundColor Black -ForegroundColor Red
	Write-Host "Incorrect format of BuildVersion.txt - First line should have format Major.Minor.Build so for instance 1.0.2"
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



$apiSentryProjectName = "queue-api"
$runnerSentryProjectName = "queue-runner"

$sentryOrganizationName = ""
$sentryAuthToken = ""


if([System.IO.File]::Exists(".\SentryToken.txt"))
{
	$sentryAuthToken = Get-Content -Path .\SentryToken.txt -First 1
}
else 
{
	Write-Host $_ -BackgroundColor Black -ForegroundColor Red
	Write-Host "There must be a SentryToken.txt in the same directory as this build script that contains only the auth token to sentry. Note that the auth token must have project:releases or project:write permissions"
	Read-Host -Prompt "Enter to exit"
	exit
}

if([System.IO.File]::Exists(".\SentryOrganizationName.txt"))
{
	$sentryOrganizationName = Get-Content -Path .\SentryOrganizationName.txt -First 1
}
else 
{
	Write-Host $_ -BackgroundColor Black -ForegroundColor Red
	Write-Host "There must be a SentryOrganizationName.txt in the same directory as this build script that contains only the sentry organization name."
	Read-Host -Prompt "Enter to exit"
	exit
}



if (Test-Path ".\Build") 
{ 
	Remove-Item .\Build -recurse -force
}

md -Force .\Build | Out-Null
md -Force .\Build\Api | Out-Null
md -Force .\Build\Runner | Out-Null
md -Force .\Build\DbDeploy | Out-Null
md -Force .\Build\Api.Tests.Integration | Out-Null
md -Force .\Build\Runner.Tests | Out-Null


dotnet publish .\QueueService.Api\QueueService.Api.csproj --output .\Build\Api --configuration Release
dotnet octo pack --basePath .\Build\Api --id="QueueService.Api" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.DbDeploy\QueueService.DbDeploy.csproj --output .\Build\DbDeploy --configuration Release
dotnet octo pack --basePath .\Build\DbDeploy --id="QueueService.DbDeploy" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.Runner\QueueService.Runner.csproj --output .\Build\Runner --configuration Release
dotnet octo pack --basePath .\Build\Runner --id="QueueService.Runner" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.Api.Tests.Integration\QueueService.Api.Tests.Integration.csproj --output .\Build\Api.Tests.Integration --configuration Release
dotnet octo pack --basePath .\Build\Api.Tests.Integration --id="QueueService.Api.Tests.Integration" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.Runner.Tests\QueueService.Runner.Tests.csproj --output .\Build\Runner.Tests --configuration Release
dotnet octo pack --basePath .\Build\Runner.Tests --id="QueueService.Runner.Tests" --version=$newVersion --outFolder=.\Build

Write-Host "Press enter to publish (or close to not publish)" -BackgroundColor Black -ForegroundColor Cyan
Read-Host

sentry-cli upload-dif --auth-token $sentryAuthToken -o $sentryOrganizationName -p $apiSentryProjectName .\Build\Api --include-sources .\QueueService.Api
sentry-cli upload-dif --auth-token $sentryAuthToken -o $sentryOrganizationName -p $runnerSentryProjectName .\Build\Runner --include-sources .\QueueService.Runner

$packages = gci ".\Build\*.nupkg"
foreach ($package in $packages){
	$packageLocation = ".\Build\" + $package.Name
	#Note to work this needs OCTOPUS_CLI_API_KEY and OCTOPUS_CLI_SERVER set in Environment variabvles in windows
	dotnet octo push --overwrite-mode=OverwriteExisting --package=$packageLocation
}


$newVersion | Set-Content .\BuildVersion.txt

Read-Host -Prompt "Enter to exit"