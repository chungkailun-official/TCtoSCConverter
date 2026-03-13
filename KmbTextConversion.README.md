# KmbTextConversion

Reusable offline Traditional Chinese / Simplified Chinese conversion library for `.NET Framework 4.8`.

## Projects

- `KmbTextConversion`: reusable library with embedded OpenCC-style dictionaries plus KMB overrides.
- `KmbTextConversion.Demo`: tiny console app for quick conversion tests.
- `KmbTextConversion.DictionaryRefresh`: regenerates `KmbTextConversion/Data/kmb_tc_to_sc.tsv` and `kmb_char_tc_to_sc.tsv` from exported route CSV files.
- `KmbTextConversion.Tests`: lightweight test runner for converter and dictionary-refresh behavior.

## Build

```powershell
dotnet build
```

## Demo

```powershell
dotnet run --project KmbTextConversion.Demo -- t2s "瞭解電腦程式與滑鼠"
dotnet run --project KmbTextConversion.Demo -- s2t "了解电脑程序与鼠标"
```

## Refresh KMB dictionaries

```powershell
dotnet run --project KmbTextConversion.DictionaryRefresh -- "C:\path\to\route-csv-folder"
```

Optional second argument: output folder for generated TSV files.

## Run tests

```powershell
dotnet run --project KmbTextConversion.Tests
```
