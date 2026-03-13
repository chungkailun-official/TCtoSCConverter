# TC to SC Converter

`TC to SC Converter` is a Windows Forms desktop application and reusable `.NET Framework 4.8` library for offline Traditional Chinese to Simplified Chinese conversion.

## Projects

- `TcToScCoverter`: Windows Forms desktop application for importing, pasting, converting, copying, exporting, and drag-dropping text files.
- `TcScTextConverterLib`: reusable offline conversion library backed by embedded OpenCC-style dictionaries.

## Build

```powershell
dotnet build TcToScCoverter\TcToScCoverter.csproj
dotnet build TcScTextConverterLib\TcScTextConverterLib.csproj
```

## Run the desktop app

```powershell
dotnet run --project TcToScCoverter\TcToScCoverter.csproj
```
