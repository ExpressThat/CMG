param(
    [int]$Port = 9831,
    [string]$IdleTimeout = "30m"
)

$cmg = Join-Path $PSScriptRoot "..\bin\Debug\net10.0\CMG.exe"
if (-not (Test-Path -LiteralPath $cmg)) {
    dotnet build (Join-Path $PSScriptRoot "..\CMG.csproj") /p:UseSharedCompilation=false
}

try {
    & $cmg browser --port $Port launch --headless --idle-timeout $IdleTimeout
    & $cmg browser --port $Port lease status
    & $cmg browser --port $Port control navigation setContent "<main><h1>Renewable agent browser</h1><p>The lease is refreshed by browser activity.</p></main>"
    & $cmg browser --port $Port lease keepAlive
    & $cmg browser --port $Port lease status
    & $cmg browser --port $Port lease disable
}
finally {
    & $cmg browser --port $Port close
}
