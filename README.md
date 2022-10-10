# Project structure:
### IronBrew2 - The obfuscator core
### IronBrew2 CLI - A CLI app that call the obfuscation method inside the core
### Lua - Some Lua testing stuff
# To use:
### 1. Compile IronBrew2 core and you should get ``IronBrew2.dll`` at ``IronBrew2\bin\Release\netcoreapp2.0\``
### 2. Copy paste the dll file into ``IronBrew2 CLI\bin\Debug\netcoreapp2.0\``
### 3. Run ``dotnet "IronBrew2 CLI.dll" "<your_lua_name>.lua"``
### 4. The output file should be located at ``IronBrew2 CLI\bin\Debug\netcoreapp2.0\output\<your_lua_name>.lua``
