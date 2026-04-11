# Projects and dependencies analysis

This document provides a comprehensive overview of the projects and their dependencies in the context of upgrading to .NETCoreApp,Version=v10.0.

## Table of Contents

- [Executive Summary](#executive-Summary)
  - [Highlevel Metrics](#highlevel-metrics)
  - [Projects Compatibility](#projects-compatibility)
  - [Package Compatibility](#package-compatibility)
  - [API Compatibility](#api-compatibility)
- [Aggregate NuGet packages details](#aggregate-nuget-packages-details)
- [Top API Migration Challenges](#top-api-migration-challenges)
  - [Technologies and Features](#technologies-and-features)
  - [Most Frequent API Issues](#most-frequent-api-issues)
- [Projects Relationship Graph](#projects-relationship-graph)
- [Project Details](#project-details)

  - [Arkane.ZeroConf\Arkane.ZeroConf.csproj](#arkanezeroconfarkanezeroconfcsproj)
  - [azclient\azclient.csproj](#azclientazclientcsproj)


## Executive Summary

### Highlevel Metrics

| Metric | Count | Status |
| :--- | :---: | :--- |
| Total Projects | 2 | All require upgrade |
| Total NuGet Packages | 3 | All compatible |
| Total Code Files | 39 |  |
| Total Code Files with Incidents | 2 |  |
| Total Lines of Code | 2780 |  |
| Total Number of Issues | 3 |  |
| Estimated LOC to modify | 0+ | at least 0.0% of codebase |

### Projects Compatibility

| Project | Target Framework | Difficulty | Package Issues | API Issues | Est. LOC Impact | Description |
| :--- | :---: | :---: | :---: | :---: | :---: | :--- |
| [Arkane.ZeroConf\Arkane.ZeroConf.csproj](#arkanezeroconfarkanezeroconfcsproj) | netcoreapp7.0 | 🟢 Low | 0 | 0 |  | ClassLibrary, Sdk Style = True |
| [azclient\azclient.csproj](#azclientazclientcsproj) | net7.0 | 🟢 Low | 1 | 0 |  | DotNetCoreApp, Sdk Style = True |

### Package Compatibility

| Status | Count | Percentage |
| :--- | :---: | :---: |
| ✅ Compatible | 3 | 100.0% |
| ⚠️ Incompatible | 0 | 0.0% |
| 🔄 Upgrade Recommended | 0 | 0.0% |
| ***Total NuGet Packages*** | ***3*** | ***100%*** |

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1786 |  |
| ***Total APIs Analyzed*** | ***1786*** |  |

## Aggregate NuGet packages details

| Package | Current Version | Suggested Version | Projects | Description |
| :--- | :---: | :---: | :--- | :--- |
| Microsoft.CSharp | 4.7.0 |  | [azclient.csproj](#azclientazclientcsproj) | ✅Compatible |
| Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers | 0.4.421302 |  | [azclient.csproj](#azclientazclientcsproj) | ✅Compatible |
| System.Data.DataSetExtensions | 4.5.0 |  | [azclient.csproj](#azclientazclientcsproj) | NuGet package functionality is included with framework reference |

## Top API Migration Challenges

### Technologies and Features

| Technology | Issues | Percentage | Migration Path |
| :--- | :---: | :---: | :--- |

### Most Frequent API Issues

| API | Count | Percentage | Category |
| :--- | :---: | :---: | :--- |

## Projects Relationship Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart LR
    P1["<b>📦&nbsp;azclient.csproj</b><br/><small>net7.0</small>"]
    P2["<b>📦&nbsp;Arkane.ZeroConf.csproj</b><br/><small>netcoreapp7.0</small>"]
    P1 --> P2
    click P1 "#azclientazclientcsproj"
    click P2 "#arkanezeroconfarkanezeroconfcsproj"

```

## Project Details

<a id="arkanezeroconfarkanezeroconfcsproj"></a>
### Arkane.ZeroConf\Arkane.ZeroConf.csproj

#### Project Info

- **Current Target Framework:** netcoreapp7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** ClassLibrary
- **Dependencies**: 0
- **Dependants**: 1
- **Number of Files**: 37
- **Number of Files with Incidents**: 1
- **Lines of Code**: 2426
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph upstream["Dependants (1)"]
        P1["<b>📦&nbsp;azclient.csproj</b><br/><small>net7.0</small>"]
        click P1 "#azclientazclientcsproj"
    end
    subgraph current["Arkane.ZeroConf.csproj"]
        MAIN["<b>📦&nbsp;Arkane.ZeroConf.csproj</b><br/><small>netcoreapp7.0</small>"]
        click MAIN "#arkanezeroconfarkanezeroconfcsproj"
    end
    P1 --> MAIN

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 1448 |  |
| ***Total APIs Analyzed*** | ***1448*** |  |

<a id="azclientazclientcsproj"></a>
### azclient\azclient.csproj

#### Project Info

- **Current Target Framework:** net7.0
- **Proposed Target Framework:** net10.0
- **SDK-style**: True
- **Project Kind:** DotNetCoreApp
- **Dependencies**: 1
- **Dependants**: 0
- **Number of Files**: 2
- **Number of Files with Incidents**: 1
- **Lines of Code**: 354
- **Estimated LOC to modify**: 0+ (at least 0.0% of the project)

#### Dependency Graph

Legend:
📦 SDK-style project
⚙️ Classic project

```mermaid
flowchart TB
    subgraph current["azclient.csproj"]
        MAIN["<b>📦&nbsp;azclient.csproj</b><br/><small>net7.0</small>"]
        click MAIN "#azclientazclientcsproj"
    end
    subgraph downstream["Dependencies (1"]
        P2["<b>📦&nbsp;Arkane.ZeroConf.csproj</b><br/><small>netcoreapp7.0</small>"]
        click P2 "#arkanezeroconfarkanezeroconfcsproj"
    end
    MAIN --> P2

```

### API Compatibility

| Category | Count | Impact |
| :--- | :---: | :--- |
| 🔴 Binary Incompatible | 0 | High - Require code changes |
| 🟡 Source Incompatible | 0 | Medium - Needs re-compilation and potential conflicting API error fixing |
| 🔵 Behavioral change | 0 | Low - Behavioral changes that may require testing at runtime |
| ✅ Compatible | 338 |  |
| ***Total APIs Analyzed*** | ***338*** |  |

