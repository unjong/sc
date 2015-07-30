Clear-Host

$Directory = "D:\720.YUHS\SharedData\_DocData\81.전체화면분석\SP"
$SearchFileFilter = "*EmrUnitDF*"

 #%{ $_.FullName } 
$Files = Get-ChildItem -Path $Directory -Filter $SearchFileFilter -Recurse | Select-Object -ExpandProperty FullName
if ($Files.Count -gt 1)
{
    $SelectedPath =  $Files | Out-GridView -Title "Search Results" -Pass
}
else
{
    $SelectedPath= $Files
}

if ($SelectedPath.Length -lt 1) { exit }

#$objExcel = New-Object -ComObject Excel.Application
#$WorkBook = $objExcel.Workbooks.Open($SelectedPath)

$xl = New-Object -comobject Excel.Application
# Show Excel
$xl.visible = $true
$xl.DisplayAlerts = $False
# Create a workbook
$wb = $xl.Workbooks.open($SelectedPath) 