//
//  OrganizationChartDumper.swift
//  
//
//  Created by Hugh Bellamy on 23/01/2021.
//

import DataStream
import Foundation
import OrgPlusChartReader

public struct OrganizationChartDumper {
    public static func dumpChart(filePath: String) throws {
        let data = try Data(contentsOf: URL(fileURLWithPath: filePath))
        var dataStream = DataStream(data)
        guard try dataStream.readString(count: 4, encoding: .ascii)! == "UOCF" else {
            throw OrgPlusChartReaderror.corrupted
        }
    
        while dataStream.remainingCount > 0 {
            var (id, data) = try dataStream.readOrganizationChartRecord()
            print("Record: \(id.hexString)")
            
            let bytes = try data.readBytes(count: data.count)
            print("Data: \(bytes.hexString)")
        }
    }
}
