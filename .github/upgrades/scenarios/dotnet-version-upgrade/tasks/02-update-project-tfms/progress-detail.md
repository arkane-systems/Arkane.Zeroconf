# Target Framework Updates Applied

## Modified Files
- `Arkane.ZeroConf\Arkane.ZeroConf.csproj`: netcoreapp7.0 → **net10.0**
- `azclient\azclient.csproj`: net7.0 → **net10.0**

## Changes
Both projects now target .NET 10.0 (LTS). Singular `TargetFramework` property preserved (no multi-targeting conversion).

## Next Steps
Package restore and removal of redundant packages in next task.
