@testable import OrgPlusChartReader
import XCTest

final class OrganizationChartTests: XCTestCase {
    func testExample() throws {
        do {
            let data = try getData(name: "SimpleChart", fileExtension: "opx")
            let file = try OrganizationChart(data: data)
            print(file)
        }
    }

    static var allTests = [
        ("testExample", testExample),
    ]
}
