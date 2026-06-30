## Repository layout

```
/CanaryLabsSamples.sln     Single solution containing every .NET project (grouped by version)
/DLLs/                     Drop required Canary DLLs here (see "Dependencies" below)
  .put_dlls_here           Placeholder
/V23/  /V24/  /V26/        Canary versions
```

Each project folder is named for the Canary service and client technology it demonstrates
(Views = read, Store-and-Forward = write), and its `.csproj` matches the folder name, e.g.
`/V26/ViewsNETClient/ViewsNETClient.csproj`.

## Dependencies

The samples depend on two kinds of assemblies:

1. **NuGet packages** These are declared as `<PackageReference>` in each `.csproj` with pinned versions.**
   > Depending on the exact version of Canary, the versions of these nuget packages may need adjusted. Look to the console output when running the .NET projects to see errors that may arise from mismatched versions.

2. **Canary assemblies** are **not** on NuGet. Copy them out of a Canary installation into the `DLLs` folder.

### The `DLLs` folder

`DLLs/` ships empty. Copy the `Canary.*` DLLs listed for the project you want
to build directly into this folder. Every `.csproj` references them with a relative path such as
`..\..\DLLs\Canary.Utility.LogCore.dll`. The DLLs come from a Canary installation (the
`Views` and `StoreAndForward` / `Admin` folders of the install directory, or the corresponding product
build output). The exact list per project is below.

## Troubleshooting (.NET clients)

If a .NET client fails to build or run, it is almost always a dependency-version mismatch between the
NuGet packages pinned in the `.csproj` and the Canary DLLs you copied into `DLLs/`.

- **Minor Canary versions may need different NuGet package versions.** The `<PackageReference>` versions
  in each `.csproj` are pinned to one Canary release; a different minor version may expect different ones.
  Visual Studio will flag the conflict (Error List / build output, e.g. assembly-version conflict warnings), 
  usually naming the version it expects — follow that guidance and update the
  `<PackageReference>` version accordingly, then rebuild.
- **Check the console output.** The sample clients catch exceptions and print the full inner-exception
  chain to the console at runtime, so run the project and read the console for the specific error.
- **Check the Canary Message Log.** If the build output and console don't reveal the cause, open the
  Canary Message Log (in the Canary Admin) for server-side errors that the client isn't relaying.

---

# V26

.NET 8 console samples (and a class-library plugin) for Canary 26.

### ViewsNETClient
Reads data through the Views gRPC API: browse nodes/tags, current value, raw data, and aggregate
(processed) data. Demonstrates both manual connection/CCI management and the `ViewsClientProvider`
helper.

- **Copy into `DLLs/`** (from the Canary Admin folder of the installation; proto files are under `API\Protos\Views`):
  - Canary.Calculations.Batch.Grpc.Common.dll
  - Canary.Utility.CanaryClientHelper.dll
  - Canary.Utility.CanaryShared.dll
  - Canary.Utility.GrpcHelper.dll
  - Canary.Utility.JsonCore.dll
  - Canary.Utility.LogCore.dll
  - Canary.Utility.PathCore.dll
  - Canary.Utility.PerformanceTracker.dll
  - Canary.Utility.ProtobufSharedTypes.dll
  - Canary.Utility.SecurityCore.dll
  - Canary.Utility.SettingsCore.dll
  - Canary.Views.Grpc.Api.dll
  - Canary.Views.Grpc.Common.dll

### StoreAndForwardNETClient
Writes data through the Store-and-Forward (SAF) gRPC API using both the raw client and the
extension-method helpers. Subscribes to the session `OnLogEntry` event and prints log entries to the
console so issues are visible in real time. Exceptions print their full inner-exception chain.

- **Copy into `DLLs/`** (from the Canary Admin folder of the installation; proto files are under `API\Protos\StoreAndForward`):
  - Canary.DiagnosticHealthSystem.Shared.dll (present in V26.1+)
  - Canary.StoreAndForward2.Grpc.Api.dll
  - Canary.StoreAndForward2.Grpc.Api.Helper.dll
  - Canary.Utility.CanaryClientHelper.dll
  - Canary.Utility.CanaryShared.dll
  - Canary.Utility.GrpcHelper.dll
  - Canary.Utility.JsonCore.dll
  - Canary.Utility.LogCore.dll
  - Canary.Utility.PathCore.dll
  - Canary.Utility.ProtobufSharedTypes.dll
  - Canary.Utility.SecurityCore.dll
  - Canary.Utility.SettingsCore.dll
  - Canary.Utility.TaskCore.dll

### CustomCalculationFunctionPlugin
A class library that implements a custom Calculations function plugin (works on Canary 26.3+).
See `Custom_Function_Plugin_Guide.md` in the project for the full development and deployment guide.

- **Copy into `DLLs/`:**
  - Canary.Calculations.FunctionPlugin.Base.dll  *(from `C:\Program Files\Canary\Calculations\`)*

---

# V24

.NET 8 / gRPC samples for Canary 24.x.

### ViewsNETClient
Reads data through the Views gRPC API.

- **Copy into `DLLs/`** (from the `Views` folder of the Canary installation; proto files are under `API\Protos\Views`):
  - Canary.Utility.CanaryClientHelper.dll
  - Canary.Utility.CanaryShared.dll
  - Canary.Utility.GrpcHelper.dll
  - Canary.Utility.JsonCore.dll
  - Canary.Utility.LogCore.dll
  - Canary.Utility.PathCore.dll
  - Canary.Utility.PerformanceTracker.dll
  - Canary.Utility.ProtobufSharedTypes.dll
  - Canary.Utility.SecurityCore.dll
  - Canary.Utility.SettingsCore.dll
  - Canary.Views.Grpc.Api.dll
  - Canary.Views.Grpc.Common.dll

### StoreAndForwardNETClient
Writes data through the Store-and-Forward gRPC API.

- **Copy into `DLLs/`** (from the Canary Admin folder of the installation; proto files are under `API\Protos\StoreAndForward`):
  - Canary.StoreAndForward2.Grpc.Api.dll
  - Canary.StoreAndForward2.Grpc.Api.Helper.dll
  - Canary.Utility.CanaryClientHelper.dll
  - Canary.Utility.CanaryShared.dll
  - Canary.Utility.GrpcHelper.dll
  - Canary.Utility.JsonCore.dll
  - Canary.Utility.LogCore.dll
  - Canary.Utility.PathCore.dll
  - Canary.Utility.ProtobufSharedTypes.dll
  - Canary.Utility.SecurityCore.dll
  - Canary.Utility.SettingsCore.dll
  - Canary.Utility.TaskCore.dll

---

# V23

Legacy samples for Canary 23.x. The .NET projects target .NET Framework 4.7.1 and the WCF web
service.

### ViewsNETClient (.NET Framework)
Reads data via the legacy `CanaryWebServiceHelper` web-service client.

- **Copy into `DLLs/`** (from the Canary installation directory):
  - CanaryWebServiceHelper.dll

### SenderNETClient (.NET Framework)
Writes data via the legacy Sender helper (`SAF_Helper.dll`). Includes the `Store & Forward API (& Helper).pdf` reference under `Documentation/`.

- **Copy into `DLLs/`** (from the Canary installation directory):
  - SAF_Helper.dll

### ParquetExport (.NET Framework)
Exports historian data to Apache Parquet.

- **Copy into `DLLs/`:**
  - CanaryWebServiceHelper.dll
- Other dependencies (Parquet.Net, IronSnappy, System.* support packages) are restored via the project's `packages.config`.

### Python
`ExportToCSV.py` — reads aggregated/raw/last-value data via the Views Web API (HTTP/JSON) and writes a CSV. No DLLs required (Python 3 + `requests`). API reference: [readapi.canarylabs.com](https://readapi.canarylabs.com/).

### PowerShell
`ExportToCSV.ps1`, `ExportToCSV2.ps1`, `ExportToCSV3.ps1` — read data via the Views Web API and export to CSV. `DiagnosticTags.txt` is sample tag input used by the scripts. No DLLs required.

### R
`ExportToCSV.R` — reads data via the Views Web API and exports to CSV. No DLLs required.

### ViewsWebApi
Browser/JavaScript demo (`data_retrieval_demo.html`) plus `data_retrieval_documentation.html` for the Views read Web API. No DLLs required.

### SenderWebApi
Browser/JavaScript demo (`data_storage_demo.html`) plus `data_storage_documentation.html` for the Sender write Web API. No DLLs required. API reference: [writeapi.canarylabs.com](https://writeapi.canarylabs.com).

### NodeRed
Two Node-RED flows that read Modbus and store to the Canary historian via the Sender Web API. No DLLs required.

**`ModbusClient`** — Canary Labs Node-RED storage example
- Tested with Node-RED v0.20.3; depends on `node-red-contrib-modbus` (v4.1.3 tested)
- Reads coils, inputs, holding registers, and input registers from a Modbus device and logs to the Canary historian via the Sender Web API
- Does **not** support client buffering — if the connection to the Sender API is lost, data may be lost

Configuration:
1. Import `CanaryModbusStorage.json` into Node-RED
2. Configure the modbus read nodes with the registers to read
3. Configure the Log Coils/Inputs/Holding Registers/Input Registers functions to map tag names to register indexes
4. Point the `CanaryStoreData`, `GetCanaryUserToken`, and `GetCanarySessionToken` node URLs at your endpoint (default localhost); leave the paths alone
5. Set the historian name in the `Format SessionToken JSON` node

The flow uses anonymous authentication. If the anonymous endpoint is off, switch those URLs to https and set username/password in the `Format UserToken JSON` node.

**`ModbusClient_StoreAndForward`** — adds client buffering via a local SQLite database
- Also depends on `node-red-node-sqlite` (v0.3.6 tested)
- Buffers data by writing/reading to a local SQLite database, so data survives a lost connection to the Sender API

Configuration:
1. Import `CanaryModbusStoreAndForward.json` into Node-RED
2. Use `buffer.sql` to create the SQLite buffer table on disk
3. Point the `Store to buffer`, `Read buffer`, `Update Sent Status`, and `Purge Sent Records` nodes at the SQLite database
4. Configure the modbus read nodes and the Log functions as above
5. Point the `TestTokens`, `RevokeSessionToken`, `RevokeUserToken`, `GetCanaryUserToken`, `GetCanarySessionToken`, and `CanaryStoreData` node URLs at your endpoint (default localhost)
6. Set the historian name in the `Format SessionToken JSON` node

Same anonymous-authentication note as above.

---

## Other Canary Labs help links

- Help Center (Knowledge Base): [https://helpcenter.canarylabs.com/](https://helpcenter.canarylabs.com/)
- Read Web API reference: [https://readapi.canarylabs.com/](https://readapi.canarylabs.com/)
- Write Web API reference: [https://writeapi.canarylabs.com](https://writeapi.canarylabs.com)
