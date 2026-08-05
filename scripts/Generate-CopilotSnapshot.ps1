param(
    [Parameter(Mandatory = $false)]
    [string]$InterfacePath = "",

    [Parameter(Mandatory = $false)]
    [string]$EntityPath = "",

    [Parameter(Mandatory = $false)]
    [string[]]$Files = @(),

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = "CopilotSnapshot.md"
)

$ErrorActionPreference = "Continue"
$Root = (Get-Location).Path
$GeneratedOn = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

function Add-Line {
    param([string]$Text = "")
    Add-Content -Path $OutputPath -Value $Text
}

function Add-Section {
    param([string]$Title)
    Add-Line ""
    Add-Line "## $Title"
    Add-Line ""
}

function Add-CodeBlock {
    param(
        [string]$Language,
        [AllowNull()][object[]]$Content
    )

    Add-Line "``````$Language"

    if ($null -ne $Content) {
        foreach ($line in $Content) {
            Add-Line ([string]$line)
        }
    }

    Add-Line "``````"
}

function Get-RelativePathSafe {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    try {
        $resolved = (Resolve-Path -Path $Path -ErrorAction Stop).Path
        if ($resolved.StartsWith($Root)) {
            return $resolved.Substring($Root.Length).TrimStart('\', '/')
        }

        return $resolved
    }
    catch {
        return $Path
    }
}

function Get-LanguageFromPath {
    param([string]$Path)

    $extension = [System.IO.Path]::GetExtension($Path).ToLowerInvariant()

    switch ($extension) {
        ".cs" { return "csharp" }
        ".razor" { return "razor" }
        ".json" { return "json" }
        ".sql" { return "sql" }
        ".ps1" { return "powershell" }
        ".md" { return "markdown" }
        ".xml" { return "xml" }
        ".config" { return "xml" }
        ".csproj" { return "xml" }
        ".sln" { return "text" }
        ".slnx" { return "xml" }
        default { return "text" }
    }
}

function Add-FileContent {
    param(
        [string]$Title,
        [string]$Path
    )

    Add-Section $Title

    if ([string]::IsNullOrWhiteSpace($Path)) {
        Add-Line "No file path provided."
        return
    }

    if (-not (Test-Path -Path $Path)) {
        Add-Line "File not found:"
        Add-CodeBlock -Language "text" -Content @($Path)
        return
    }

    $relative = Get-RelativePathSafe -Path $Path
    $language = Get-LanguageFromPath -Path $Path

    Add-Line "**File:** ``$relative``"
    Add-Line ""

    $content = Get-Content -Path $Path -ErrorAction SilentlyContinue
    Add-CodeBlock -Language $language -Content $content
}

function Get-CleanTree {
    param(
        [string]$Path,
        [string]$Indent = ""
    )

    $excludedDirectories = @(
        "bin",
        "obj",
        ".git",
        ".vs",
        ".idea",
        "node_modules",
        "TestResults",
        "packages"
    )

    $excludedFiles = @(
        "CopilotSnapshot.md",
        "SolutionTree.txt",
        "BuildOutput.txt",
        "GitStatus.txt",
        "GitHistory.txt"
    )

    $items = Get-ChildItem -Path $Path -Force -ErrorAction SilentlyContinue |
        Where-Object {
            if ($_.PSIsContainer -and ($excludedDirectories -contains $_.Name)) {
                return $false
            }

            if (-not $_.PSIsContainer -and ($excludedFiles -contains $_.Name)) {
                return $false
            }

            return $true
        } |
        Sort-Object @{ Expression = "PSIsContainer"; Descending = $true }, Name

    foreach ($item in $items) {
        if ($item.PSIsContainer) {
            "$Indent+-- $($item.Name)"
            Get-CleanTree -Path $item.FullName -Indent "$Indent    "
        }
        else {
            "$Indent|-- $($item.Name)"
        }
    }
}

if (Test-Path -Path $OutputPath) {
    Remove-Item -Path $OutputPath -Force
}

Add-Line "# Copilot Development Snapshot"
Add-Line ""
Add-Line "- Generated On: $GeneratedOn"
Add-Line "- Solution Root: $Root"

Add-Section "0. Snapshot Inputs"
if ([string]::IsNullOrWhiteSpace($InterfacePath)) {
    Add-Line "- Interface Path: Not provided"
}
else {
    Add-Line "- Interface Path: ``$(Get-RelativePathSafe -Path $InterfacePath)``"
}

if ([string]::IsNullOrWhiteSpace($EntityPath)) {
    Add-Line "- Entity Path: Not provided"
}
else {
    Add-Line "- Entity Path: ``$(Get-RelativePathSafe -Path $EntityPath)``"
}

if ($Files.Count -eq 0) {
    Add-Line "- Additional Files: None provided"
}
else {
    Add-Line "- Additional Files:"
    foreach ($file in $Files) {
        Add-Line "  - ``$(Get-RelativePathSafe -Path $file)``"
    }
}

Add-Section "1. Current Solution Tree"
$tree = Get-CleanTree -Path $Root
Add-CodeBlock -Language "text" -Content $tree

Add-Section "2. Current Build Status"
$buildOutput = dotnet build 2>&1
Add-CodeBlock -Language "text" -Content $buildOutput

Add-Section "3. Git Status"
try {
    $gitStatus = git status --short 2>&1
    Add-CodeBlock -Language "text" -Content $gitStatus
}
catch {
    Add-CodeBlock -Language "text" -Content @("Git status unavailable.")
}

Add-Section "4. Recent Git History"
try {
    $gitHistory = git log --oneline -10 2>&1
    Add-CodeBlock -Language "text" -Content $gitHistory
}
catch {
    Add-CodeBlock -Language "text" -Content @("Git history unavailable.")
}

Add-Section "5. Git Changed Files"
try {
    Add-Line "### Unstaged Changes"
    Add-Line ""
    $unstaged = git diff --name-only 2>&1
    Add-CodeBlock -Language "text" -Content $unstaged

    Add-Line ""
    Add-Line "### Staged Changes"
    Add-Line ""
    $staged = git diff --cached --name-only 2>&1
    Add-CodeBlock -Language "text" -Content $staged
}
catch {
    Add-CodeBlock -Language "text" -Content @("Git changed file information unavailable.")
}

Add-Section "6. Git Diff Summary"
try {
    $diffSummary = git diff --stat 2>&1
    Add-CodeBlock -Language "text" -Content $diffSummary
}
catch {
    Add-CodeBlock -Language "text" -Content @("Git diff summary unavailable.")
}

Add-FileContent -Title "7. Interface Being Implemented" -Path $InterfacePath
Add-FileContent -Title "8. Entity Used By That Interface" -Path $EntityPath

Add-Section "9. Additional Source Files"
if ($Files.Count -eq 0) {
    Add-Line "No additional files provided."
}
else {
    $index = 1
    foreach ($file in $Files) {
        Add-FileContent -Title "9.$index Additional File" -Path $file
        $index++
    }
}

Add-Section "10. Suggested PCC Prompt Context"
Add-Line "Use this snapshot when asking for PCC changes."
Add-Line ""
Add-Line "Recommended prompt format:"
Add-Line ""
Add-CodeBlock -Language "text" -Content @(
    "PCC <FileName>",
    "",
    "Use the attached CopilotSnapshot.md as the source of truth.",
    "Generate the complete file only.",
    "Do not assume missing entity, interface, repository, or Razor properties."
)

Write-Host ""
Write-Host "Copilot snapshot created: $OutputPath"
Write-Host "Open it with: notepad $OutputPath"
Write-Host ""
