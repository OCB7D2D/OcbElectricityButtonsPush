@echo off

call MC7D2D ElectricityButtonsPush.dll ^
/reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" ^
Harmony\*.cs Library\*.cs && ^
echo Successfully compiled ElectricityButtonsPush.dll

pause