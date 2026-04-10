# MtgoDecklistScraper

A .NET console app that scrapes Magic: The Gathering Online decklists from [mtgo.com/decklists](https://www.mtgo.com/decklists) and saves each event as a JSON file organised by year and month.

## Usage

Run for the current month's events:

```bash
dotnet run --project src/MtgoDecklistScraperNet
```

Run for a specific month (both `--year` and `--month` must be supplied together):

```bash
dotnet run --project src/MtgoDecklistScraperNet -- --year 2026 --month 4
```

## Output layout

```
output/
├── months.json                              # ["2026/03", "2026/04", ...]
├── 2026/
│   ├── 03/
│   │   ├── events.json                      # ["modern-challenge-...json", ...]
│   │   ├── modern-challenge-64-2026-03-31...json
│   │   └── ...
│   └── 04/
│       └── ...
```

Each event JSON contains the full deck data: event name, format, date, and every player's main deck and sideboard with card names, quantities, and MTGO card IDs.
`months.json` is an index of the months that exist.
Each month directory has a file `events.json` that lists all the event files available for that month.

# JSON Models

`src/MtgoDecklistModels` Contains a Class library with models for the event/deck JSON data.
