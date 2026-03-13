# KMB Route Downloader

WinForms desktop application targeting `.NET Framework 4.8` that downloads KMB route, stop, and route-stop data from the official KMB transport APIs and exports a `route.csv` file in this format:

```csv
bus_number,stop_name,direction,stop_id,stop_name_sc,direction_sc
```

## Build

```powershell
dotnet build
```

## Run

```powershell
dotnet run
```

The application defaults the export file to `route.csv` in the application folder, and you can change it with the `Browse...` button.
