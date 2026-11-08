# OrgPlusChartReader

Readers for organization chart files (`.opx` / `.ops`) created by the [Microsoft Office Organization Chart add-in](https://support.microsoft.com/en-us/office/install-the-microsoft-office-organization-chart-add-in-39353e37-f8df-4a3e-9925-6be1a75087e5).

This project was created as an experiment into reverse-engineering and unknown file format reading. As such, it is incomplete and serves its purpose best as an unofficial documentation of the "opx" file format. See [Notes.md](Notes.md) for what is known about the format.

There are two implementations:

| Implementation | Location | Status |
| --- | --- | --- |
| C# (.NET Standard 2.0) | [Explorer/OrgPlusChartReader](Explorer/OrgPlusChartReader) | Current. Specialised records, validation and tests. |
| Swift | [Sources](Sources) | Original implementation. Builds, but is not kept in step with the C# reader. |

## C# usage

```csharp
using OrgPlusChartReader;

using var stream = File.OpenRead("<path-to-file>.opx");
var chart = new OrganizationChart(stream); // The stream is left open.

foreach (var record in chart.Records)
{
    Console.WriteLine(record.Id);
}
```

Malformed files, including truncated ones, throw `InvalidDataException`. Records are returned as specific types (for example `BorderRecord`) where the format is understood, and as the base `OrganizationChartRecord` otherwise. Sections are `OrganizationChartSubstream`s that contain their child records.

Other projects in [Explorer](Explorer):

- `OrgPlusChartDumper` prints the structure of a file: `dotnet run --project Explorer/OrgPlusChartDumper -- <path>`
- `OrgPlusChartExplorer` is a Windows Forms app that shows a file as a tree and diffs two files.
- `OrgPlusChartReader.Tests` runs the reader over every file in [Samples](Samples): `dotnet test Explorer/OrgPlusChartReader.Tests`

## Swift usage

Add the following line to your project's SwiftPM dependencies:
```swift
.package(url: "https://github.com/hughbe/OrgPlusChartReader", from: "1.0.0"),
```

```swift
import OrgPlusChartReader

let data = try Data(contentsOf: URL(fileURLWithPath: "<path-to-file>.opx"))
let chart = try OrganizationChart(data: data)
print(chart.records)
```

The Swift tests need XCTest (a full Xcode install on macOS): `swift test`.
