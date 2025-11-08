//
//  DataStream+ReadOrganizationChartRecord.swift
//  
//
//  Created by Hugh Bellamy on 24/01/2021.
//

import DataStream

public extension DataStream {
    mutating func readOrganizationChartRecord() throws -> (id: UInt16, data: DataStream) {
        let id: UInt16 = try read(endianess: .littleEndian)

        let data: DataStream
        if id >= 0x4000 {
            data = DataStream([])
        } else {
            let size: UInt16 = try read(endianess: .littleEndian)
            guard size <= remainingCount else {
                throw OrgPlusChartReaderror.corrupted
            }
            
            data = DataStream(slicing: self, startIndex: position, count: Int(size))
            position += Int(size)
        }

        return (id, data)
    }
}
