Stop-Process -Name "Flow.Launcher" -Force
dotnet publish -o "$env:LOCALAPPDATA\FlowLauncher\app-1.16.2\Plugins\Flow.Launcher.Plugin.RDP"
Start-Process "$env:LOCALAPPDATA\FlowLauncher\Flow.Launcher.exe"