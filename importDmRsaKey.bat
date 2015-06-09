@echo on
rem this script must be ran under the administrator
cd \WINDOWS\Microsoft.Net\Framework\v2.0.*
aspnet_regiis -pi "DisplayMonkeyRsaKeyContainer" "c:\temp\DisplayMonkeyRsaKeyContainer.xml"
pause
