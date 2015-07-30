Clear-Host

$FromDir = "\\10.10.11.20\NugetPackages\Deploy"
$ToDir = "D:\720.YUHS\Useverance3\src\Common\artifacts\Debug"
$Filter = "*.dll"
#Error	53	Metadata file 'D:\720.YUHS\Useverance3\src\Common\artifacts\Debug\His3.Hp.Client.Com.dll' could not be found	D:\720.YUHS\Useverance3\src\Sp\Client\Blo\CSC	His3.Sp.Client.Blo
#Error	54	Metadata file 'D:\720.YUHS\Useverance3\src\Common\artifacts\Debug\His3.Sp.Client.Com.dll' could not be found	D:\720.YUHS\Useverance3\src\Sp\Client\Blo\CSC	His3.Sp.Client.Blo

$sw = [System.Diagnostics.Stopwatch]::startNew()
$sw.Start()

Get-ChildItem -path $FromDir -recurse -Include $Filter | 
    where { $_.FullName } | 
    Foreach-Object { 
    $_
    Copy-Item $_ -Destination $ToDir
    }

#Foreach-Object -Begin { If($_ -eq $null){ continue }} -Process { Copy-Item $_ -Destination $ToDir }
#11.8747187

#XCOPY "\\10.10.11.20\NugetPackages\Deploy\*.dll" "D:\720.YUHS\Useverance3\src\Common\artifacts\Debug" /S /I /Y 
#14.4612212

$Sw.Stop()
$Sw.Elapsed.TotalSeconds

