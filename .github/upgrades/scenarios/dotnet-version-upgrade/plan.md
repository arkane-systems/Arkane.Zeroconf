# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade solution from .NET 7 to .NET 10.0 (LTS)
**Scope**: 2 projects, ~2,780 LOC, straightforward upgrade with no API breaking changes

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 2 projects, both on .NET 7, shallow dependency graph, low complexity with no code changes required.

## Tasks

### 01-prerequisites: Validate Prerequisites

Verify .NET 10 SDK is installed and check for global.json constraints that might restrict SDK version.

**Done when**: .NET 10 SDK confirmed available and no global.json conflicts detected

---

### 02-update-project-tfms: Update Target Frameworks

Update TargetFramework in both project files from .NET 7 to .NET 10:
- `Arkane.ZeroConf\Arkane.ZeroConf.csproj`: netcoreapp7.0 → net10.0
- `azclient\azclient.csproj`: net7.0 → net10.0

**Done when**: Both projects specify net10.0 as target framework

---

### 03-update-packages: Update Package References

Update NuGet packages across all projects:
- Remove redundant package reference: System.Data.DataSetExtensions (functionality included in .NET 10 framework)
- Verify remaining packages (Microsoft.CSharp, UpgradeAssistant.Extensions.Default.Analyzers) remain compatible

**Done when**: All package references updated, redundant packages removed, restore succeeds

---

### 04-build-and-validate: Build and Validate Solution

Build the full solution targeting .NET 10 and fix any compilation errors if they arise (none expected based on assessment).

**Done when**: Solution builds successfully with 0 errors and 0 warnings

---

### 05-run-tests: Run All Tests

Execute all tests to verify functionality is preserved after the upgrade.

**Done when**: All tests pass with no regressions
