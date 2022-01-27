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
../octo_tmp_delete pack --basePath .\Build\Api --id="QueueService.Api" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.DbDeploy\QueueService.DbDeploy.csproj --output .\Build\DbDeploy --configuration Release
../octo_tmp_delete pack --basePath .\Build\DbDeploy --id="QueueService.DbDeploy" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.Runner\QueueService.Runner.csproj --output .\Build\Runner --configuration Release
../octo_tmp_delete pack --basePath .\Build\Runner --id="QueueService.Runner" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.Api.Tests.Integration\QueueService.Api.Tests.Integration.csproj --output .\Build\Api.Tests.Integration --configuration Release
../octo_tmp_delete pack --basePath .\Build\Api.Tests.Integration --id="QueueService.Api.Tests.Integration" --version=$newVersion --outFolder=.\Build

dotnet publish .\QueueService.Runner.Tests\QueueService.Runner.Tests.csproj --output .\Build\Runner.Tests --configuration Release
../octo_tmp_delete pack --basePath .\Build\Runner.Tests --id="QueueService.Runner.Tests" --version=$newVersion --outFolder=.\Build

Write-Host "Press enter to publish (or close to not publish)" -BackgroundColor Black -ForegroundColor Cyan
Read-Host

$packages = gci ".\Build\*.nupkg"
foreach ($package in $packages){
	$packageLocation = ".\Build\" + $package.Name
	#Note to work this needs OCTOPUS_CLI_API_KEY and OCTOPUS_CLI_SERVER set in Environment variabvles in windows
	../octo_tmp_delete push --overwrite-mode=OverwriteExisting --package=$packageLocation
}


$newVersion | Set-Content .\BuildVersion.txt

Read-Host -Prompt "Enter to exit"