import XCTest
@testable import OrgPlusChartReader

final class DumpFileTests: XCTestCase {
    func testDumpOpx() throws {
        for (name, fileExtension) in [
            ("SimpleChart", "opx"),
            ("Text", "opx"),
            ("Thickness6", "opx"),
            ("PurpleBackground", "opx"),
            ("CustomFont", "opx")
        ] {
            let data = try getData(name: name, fileExtension: fileExtension)
            let chart = try OrganizationChart(data: data)
            print(chart.records)
        }
    }

    static var allTests = [
        ("testDumpOpx", testDumpOpx),
    ]
}
