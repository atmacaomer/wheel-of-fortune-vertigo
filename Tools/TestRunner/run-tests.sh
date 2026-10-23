#!/usr/bin/env bash
# Compiles Scripts/Gameplay + Tests/EditMode with Unity's bundled Roslyn and runs them with Runner.cs.
# Works while the Unity editor is open. In Unity itself use Window > General > Test Runner.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
UNITY_DATA="${UNITY_DATA:-/c/Program Files/Unity/Hub/Editor/6000.3.23f1/Editor/Data}"
DOTNET="$UNITY_DATA/NetCoreRuntime/dotnet.exe"
FRAMEWORK_DIR="$(ls -d "$UNITY_DATA"/NetCoreRuntime/shared/Microsoft.NETCore.App/* | head -1)"
NUNIT="$(find "$ROOT/Library/PackageCache" -path '*com.unity.ext.nunit*' -name nunit.framework.dll | head -1)"
OUT="${TMPDIR:-/tmp}/vertigo-tests"
mkdir -p "$OUT"

REFS=()
# Managed reference assemblies only (the folder also holds native api-ms-win-*/clr DLLs).
for dll in "$FRAMEWORK_DIR"/System*.dll "$FRAMEWORK_DIR"/mscorlib.dll "$FRAMEWORK_DIR"/netstandard.dll; do
  case "$(basename "$dll")" in System.Private.CoreLib.dll|*.Native.dll) ;; *) REFS+=("-r:$dll") ;; esac
done
REFS+=("-r:$FRAMEWORK_DIR/System.Private.CoreLib.dll")

"$DOTNET" "$UNITY_DATA/DotNetSdkRoslyn/csc.dll" -nologo -noconfig -nostdlib -target:exe -nowarn:1701,1702 \
  -out:"$OUT/Tests.dll" "${REFS[@]}" -r:"$NUNIT" \
  "$ROOT"/Assets/_Project/Scripts/Gameplay/*.cs "$ROOT"/Assets/_Project/Tests/EditMode/*.cs "$ROOT/Tools/TestRunner/Runner.cs"

cp "$NUNIT" "$OUT/"
FW_VERSION="$(basename "$FRAMEWORK_DIR")"
cat > "$OUT/Tests.runtimeconfig.json" <<EOF
{ "runtimeOptions": { "tfm": "net6.0", "framework": { "name": "Microsoft.NETCore.App", "version": "$FW_VERSION" } } }
EOF
"$DOTNET" "$OUT/Tests.dll"
