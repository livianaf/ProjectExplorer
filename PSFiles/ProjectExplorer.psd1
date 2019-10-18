@{
# Script module or binary module file associated with this manifest
ModuleToProcess = 'ProjectExplorer.psm1'

# Version number of this module.
ModuleVersion = '1.0'

# ID used to uniquely identify this module
GUID = 'ga303c6c-4d07-9bac-e6fe-ace6a97723f8'

# Author of this module
Author = 'Pablo Moreno'

# Company or vendor of this module
CompanyName = 'HPE'

# Copyright statement for this module
Copyright = '2019'

# Description of the functionality provided by this module
Description = "This modules shows a list of projects with their functions in a tree view in the ISE and enables the user to jump to the function' entry point, go back and search references. Based on 'Raimund Andree' 'FunctionExplorer' Addon."

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '3.0'

# Name of the Windows PowerShell host required by this module
PowerShellHostName = 'Windows PowerShell ISE Host'

# Minimum version of the Windows PowerShell host required by this module
PowerShellHostVersion = ''

# Minimum version of the .NET Framework required by this module
DotNetFrameworkVersion = '4.5'

# Minimum version of the common language runtime (CLR) required by this module
CLRVersion = '4.5'

# Processor architecture (None, X86, Amd64, IA64) required by this module
ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# for example 'ActiveDirectory'
RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# for example 'System.Management.Configuration'
RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module
ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @()

# Modules to import as nested modules of the module specified in ModuleToProcess
NestedModules = @()

# Functions to export from this module
#FunctionsToExport = ''

# Cmdlets to export from this module
#CmdletsToExport = ''

# Variables to export from this module
#VariablesToExport = ''

# Aliases to export from this module
#AliasesToExport = '*'

# List of all modules packaged with this module
ModuleList = @('ProjectExplorer.psm1')

# List of all files packaged with this module
FileList = @('ProjectExplorer.psm1', 'ProjectExplorer.psd1', 'ProjectExplorer')

# Private data to pass to the module specified in ModuleToProcess
PrivateData = ''
}