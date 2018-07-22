$assemblyInfo = [IO.File]::ReadAllText("./Squalr/Squalr/Properties/AssemblyInfo.cs")

$title = $assemblyInfo | Select-String -Pattern 'AssemblyTitle\(".+"\)' -AllMatches | % { $_.Matches } | % { $_.Value } | %{$_.split('"')[1]}

$version = $assemblyInfo | Select-String -Pattern 'AssemblyVersion\(".+"\)' -AllMatches | % { $_.Matches } | % { $_.Value } | %{$_.split('"')[1]}

$description = $assemblyInfo | Select-String -Pattern 'AssemblyDescription\(".+"\)' -AllMatches | % { $_.Matches } | % { $_.Value } | %{$_.split('"')[1]}

$nuspec = [IO.File]::ReadAllText("Squalr/Squalr.nuspec")

$nuspec = $nuspec.Replace('$id$', $title)
$nuspec = $nuspec.Replace('$title$', $title)
$nuspec = $nuspec.Replace('$version$', $version)
$nuspec = $nuspec.Replace('$description$', $description)

[IO.File]::WriteAllText("Squalr/SqualrTmp.nuspec", $nuspec)

$sourceRoot = "Squalr/bin/Release/*"
$destinationRoot = "lib/net45"
$exclude = @('*.pdb')

Remove-Item $destinationRoot -Force -Recurse
New-Item -ItemType Directory -Force -Path $destinationRoot
Copy-Item -Path $sourceRoot -Recurse -Destination $destinationRoot -Container -Force -Exclude $exclude

$nugetFile= "SqualrTmp.nuspec"

Invoke-Expression "nuget pack $($nugetFile) -Properties Configuration=Release"

$squirrel= "packages\squirrel.windows.1.8.0\tools\Squirrel.exe"
$package = "Squalr.2.3.5.nupkg"

Invoke-Expression "$squirrel --releasify $package -n /a /f SqualrCert.pfx /fd sha256 /tr http://timestamp.digicert.com /td sha256"

Remove-Item $nugetFile -Force