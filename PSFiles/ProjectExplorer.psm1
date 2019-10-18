#____________________________________________________________________________________________________________________
if ($host.Name -ne "Windows PowerShell ISE Host"){
    Write-Warning "This module does only run inside PowerShell ISE"
    return
    }

#region Save-AllISEFiles
#____________________________________________________________________________________________________________________
#Saves all ISE Files except for untitled files. If You have multiple PowerShellTabs, saves files in all tabs.
function Save-AllIseFiles{
    foreach($tab in $psISE.PowerShellTabs){
        foreach($file in $tab.Files){
            if(!$file.IsUntitled){
                $file.Save()
                }
            }
        }
    }
#____________________________________________________________________________________________________________________
function Close-AllIseFiles{ $psise.CurrentPowerShellTab.Files.Clear() }
#____________________________________________________________________________________________________________________
function Close-AllButThisIseFiles{
    $filesToRemove = $psISE.CurrentPowerShellTab.Files | Where-Object { $_ -ne $psISE.CurrentFile }
    $filesToRemove | ForEach-Object { $psISE.CurrentPowerShellTab.Files.Remove($_, $true) }
    }
#____________________________________________________________________________________________________________________
# This line will add a new option in the Add-ons menu to save all ISE files with the Ctrl+Shift+S shortcut. 
# If you try to run it a second time it will complain that Ctrl+Shift+S is already in use
$psISE.CurrentPowerShellTab.AddOnsMenu.Submenus.Add("Save All",{Save-AllISEFiles},"Ctrl+Shift+S")
#endregion Save-AllISEFiles
#____________________________________________________________________________________________________________________
$null = $psISE.CurrentPowerShellTab.AddOnsMenu.Submenus.Add("Go To Definition",{ $null = (($psISE.CurrentPowerShellTab.VerticalAddOnTools | Where-Object { $_.Name -eq 'ProjectExplorer' }).Control.GoToDefinition()) }, "F12")
$null = $psISE.CurrentPowerShellTab.AddOnsMenu.Submenus.Add(“Go Back”, { $null = (($psISE.CurrentPowerShellTab.VerticalAddOnTools | Where-Object { $_.Name -eq 'ProjectExplorer' }).Control.GoBack()) }, “Shift+F12”)
$null = $psISE.CurrentPowerShellTab.AddOnsMenu.Submenus.Add(“References”, { ($psISE.CurrentPowerShellTab.VerticalAddOnTools | Where-Object { $_.Name -eq 'ProjectExplorer' }).Control.GetReferences() }, “F7”)

$psISE.CurrentPowerShellTab.AddOnsMenu.Submenus.Add("Close All",{ Close-AllIseFiles }, $null)
$psISE.CurrentPowerShellTab.AddOnsMenu.Submenus.Add("Close All but this",{ Close-AllButThisIseFiles }, $null)

Add-Type -Path $PSScriptRoot\ProjectExplorer.dll -PassThru
$typeProjectExplorer = [IseAddons.ProjectExplorer]
$psISE.CurrentPowerShellTab.VerticalAddOnTools.Add("ProjectExplorer", $typeProjectExplorer, $true)