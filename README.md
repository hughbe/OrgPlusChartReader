# OrgPlusChartReader

Swift reader for organization chart files created by the [Microsoft Office Organization Chart add-in](https://support.microsoft.com/en-us/office/install-the-microsoft-office-organization-chart-add-in-39353e37-f8df-4a3e-9925-6be1a75087e5).

This project was created as an experiment into reverse-engineering and unknown file format reading. As such, it is incomplete and serves its purpose best as an unofficial documentation of the "opx" file format.

## Example Usage

Add the following line to your project's SwiftPM dependencies:
```swift
.package(url: "https://github.com/hughbe/OpxReader", from: "1.0.0"),
```

```swift
import OpxReader

let data = Data(contentsOfFile: "<path-to-file>.opx")!
let chart = try OrganizationChart(data: data)
print(chart.records)
```
