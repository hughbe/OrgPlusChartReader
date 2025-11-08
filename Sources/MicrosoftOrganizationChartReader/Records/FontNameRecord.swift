//
//  FontNameRecord.swift
//  
//
//  Created by Hugh Bellamy on 20/01/2021.
//

import DataStream

public struct FontNameRecord {
    public let index: UInt16
    public let name: String
    
    public init(dataStream: inout DataStream) throws {
        self.index = try dataStream.read(endianess: .littleEndian)
        
        guard let name = try dataStream.readAsciiString() else {
            throw OrgPlusChartReaderror.corrupted
        }
        
        self.name = name
    }
}
