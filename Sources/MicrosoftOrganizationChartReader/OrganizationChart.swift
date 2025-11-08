//
//  OrganizationChart.swift
//
//
//  Created by Hugh Bellamy on 30/11/2020.
//

import DataStream
import Foundation

public enum OrganizationChartRecord {
    /// 0x0010 record
    case boxTextPartCount(_: UInt16)
    
    /// 0x0011 record
    case boxTextPartIndex(_: BoxTextPartIndex)
    
    /// 0x0012 record
    case boxTextPartType(_: BoxTextPartType)

    /// 0x0020 record
    case bodyInfo(_: BodyInfoRecord)
    
    /// 0x0025 record
    case fontName(_: FontNameRecord)
    
    /// 0x0026 record
    case titlePartIndex(_: TitlePartIndex)
    
    /// 0x0041 record
    case level(_: LevelRecord)
    
    /// 0x0042 record
    case unknown0x0042(_: Unknown0x0042Record)
    
    /// 0x0050 record
    case boxIndex(_: UInt16)
    
    /// 0x0053 record
    case border(_: BorderRecord)
    
    /// 0x0055 record
    case connector(_: ConnectorRecord)
    
    /// 0x0056 record
    case line(_: LineRecord)
    
    /// 0x0060 record
    case text(_: String)
    
    ///  0x0061 record
    case textStyle(_: TextStyleRecord)
    
    /// 0x0083 record
    case shape(_: ShapeRecord)
    
    /// 0x0084 record
    case shapeColor(_: ShapeColorRecord)
    
    /// 0x0088 record
    case selected(_: UInt16)
    
    /// 0x0092 record
    case shadow(_: ShadowRecord)
    
    /// 0x009C record
    case fontIndex(_: FontIndexRecord)
    
    /// 0x0FFD record
    case globalInfo(_: GlobalInfoRecord)
    
    /// 0x0FFF record
    case footer(_: FooterRecord)

    /// 0x4001 section
    case parentSection(_: OrganizationChartParentSection)
    
    /// 0x4002 section
    case containerSection(_: OrganizationChartContainerSection)
    
    /// 0x4003 section
    case canvasSection(_: OrganizationChartCanvasSection)
    
    /// 0x4005 section
    case globalInfoSection(_: OrganizationChartGlobalInfoSection)
    
    /// 0x4006 section
    case levelsSection(_: OrganizationChartLevelsSection)
    
    /// 0x4007 section
    case levelSection(_: OrganizationChartLevelSection)
    
    /// 0x4008 section
    case boxTextPartsSection(_: OrganizationChartBoxTextPartsSection)
    
    /// 0x4009 section
    case titleSection(_: OrganizationChartTitleSection)
    
    /// 0x400B section
    case fontSection(_: OrganizationChartFontSection)
    
    /// 0x400C section
    case linesSection(_: OrganizationChartLinesSection)
    
    /// 0x400E section
    case shapesSection(_: OrganizationChartShapesSection)
    
    /// 0x4010 section
    case textStyleSection(_: OrganizationChartTextStyleSection)
    
    /// 0x4012 section
    case connectorsSection(_: OrganizationChartConnectorsSection)
    
    /// Unknown record
    case unknownRecord(id: UInt16, data: DataStream)
    
    /// Unknown section
    case unknownSection(id: UInt16, records: [OrganizationChartRecord])
    
    public init(id: UInt16, data: DataStream, dataStream: inout DataStream) throws {
        func unknownSection() throws -> OrganizationChartRecord {
            var records: [OrganizationChartRecord] = []
            
            while true {
                let (nextRecordId, nextRecordData) = try dataStream.readOrganizationChartRecord()
                guard nextRecordId != id + 0x2000 else {
                    break
                }

                records.append(try OrganizationChartRecord(id: nextRecordId, data: nextRecordData, dataStream: &dataStream))
            }
            
            return .unknownSection(id: id, records: records)
        }
        
        guard let knownId = OrganizationChartRecordId(rawValue: id) else {
            #if DEBUG
            fatalError("NYI: \(id.hexString)")
            #else
            return .unknownRecord(id: id, data: data)
            #endif
        }
        
        var data = data
        switch knownId {
        /// Records
        case .unknown0x0001:
            /// 0x0001 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0002:
            /// 0x0002 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0003:
            /// 0x0003 record
            self = .unknownRecord(id: id, data: data)
        case .boxTextPartCount:
            /// 0x0010 record
            self = .boxTextPartCount(try data.read(endianess: .littleEndian))
        case .boxTextPartIndex:
            /// 0x0011 record
            self = .boxTextPartIndex(try BoxTextPartIndex(dataStream: &data))
        case .boxTextPartType:
            /// 0x0012 record
            self = .boxTextPartType(try BoxTextPartType(dataStream: &data))
        case .bodyInfo:
            /// 0x0020 record
            self = .bodyInfo(try BodyInfoRecord(dataStream: &data))
        case .unknown0x0021:
            /// 0x0021 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0022:
            /// 0x0022 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0023:
            /// 0x0023 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0024:
            /// 0x0024 record
            self = .unknownRecord(id: id, data: data)
        case .fontName:
            /// 0x0025 record
            self = .fontName(try FontNameRecord(dataStream: &data))
        case .titlePartIndex:
            /// 0x0026 record
            self = .titlePartIndex(try TitlePartIndex(dataStream: &data))
        case .level:
            /// 0x0041 record
            self = .level(try LevelRecord(dataStream: &data))
        case .unknown0x0042:
            /// 0x0042 record
            self = .unknownRecord(id: id, data: data)
        case .boxIndex:
            /// 0x0050 record
            self = .boxIndex(try data.read(endianess: .littleEndian))
        case .unknown0x0051:
            /// 0x0051 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0052:
            /// 0x0052 record
            self = .unknownRecord(id: id, data: data)
        case .border:
            /// 0x0053 record
            self = .border(try BorderRecord(dataStream: &data))
        case .unknown0x0054:
            /// 0x0054 record
            self = .unknownRecord(id: id, data: data)
        case .connector:
            /// 0x0055 record
            self = .connector(try ConnectorRecord(dataStream: &data))
        case .line:
            /// 0x0056 record
            self = .line(try LineRecord(dataStream: &data))
        case .text:
            /// 0x0060 record
            self = .text(try data.readAsciiString()!)
        case .textStyle:
            /// 0x0061 record
            self = .textStyle(try TextStyleRecord(dataStream: &data))
        case .shape:
            /// 0x0083 record
            self = .shape(try ShapeRecord(dataStream: &data))
        case .shapeColor:
            /// 0x0084 record
            self = .shapeColor(try ShapeColorRecord(dataStream: &data))
        case .selected:
            /// 0x0088 record
            self = .selected(try data.read(endianess: .littleEndian))
        case .shadow:
            /// 0x0092 record
            self = .shadow(try ShadowRecord(dataStream: &data))
        case .unknown0x0093:
            /// 0x0093 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0094:
            /// 0x0094 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0095:
            /// 0x0095 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0096:
            /// 0x0096 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0097:
            /// 0x0097 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x0098:
            /// 0x0098 record
            self = .unknownRecord(id: id, data: data)
        case .unknown0x009B:
            /// 0x009B record
            self = .unknownRecord(id: id, data: data)
        case .fontIndex:
            /// 0x009C record
            self = .fontIndex(try FontIndexRecord(dataStream: &data))
        case .globalInfo:
            /// 0x0FFD record
            self = .globalInfo(try GlobalInfoRecord(dataStream: &data))
        case .footer:
            /// 0x0FFF record
            self = .footer(try FooterRecord(dataStream: &data))

        /// Sections
        case .parentSectionStart:
            /// 0x4001 section
            self = .parentSection(try OrganizationChartParentSection(dataStream: &dataStream))
        case .containerSectionStart:
            /// 0x4002 section
            self = .containerSection(try OrganizationChartContainerSection(dataStream: &dataStream))
        case .canvasSectionStart:
            /// 0x4003 section
            self = .canvasSection(try OrganizationChartCanvasSection(dataStream: &dataStream))
        case .globalInfoSectionStart:
            /// 0x4005 section
            self = .globalInfoSection(try OrganizationChartGlobalInfoSection(dataStream: &dataStream))
        case .levelsSectionStart:
            /// 0x4006 section
            self = .levelsSection(try OrganizationChartLevelsSection(dataStream: &dataStream))
        case .levelSectionStart:
            /// 0x4007 section
            self = .levelSection(try OrganizationChartLevelSection(dataStream: &dataStream))
        case .boxTextPartsSectionStart:
            /// 0x4008 section
            self = .boxTextPartsSection(try OrganizationChartBoxTextPartsSection(dataStream: &dataStream))
        case .titleSectionStart:
            /// 0x4009 section
            self = .titleSection(try OrganizationChartTitleSection(dataStream: &dataStream))
        case .unknown0x400A:
            /// 0x400A section
            self = try unknownSection()
        case .fontSectionStart:
            /// 0x400B section
            self = .fontSection(try OrganizationChartFontSection(dataStream: &dataStream))
        case .linesSectionStart:
            /// 0x400C section
            self = .linesSection(try OrganizationChartLinesSection(dataStream: &dataStream))
        case .shapesSectionStart:
            /// 0x400E section
            self = .shapesSection(try OrganizationChartShapesSection(dataStream: &dataStream))
        case .textStyleSectionStart:
            /// 0x40010 section
            self = .textStyleSection(try OrganizationChartTextStyleSection(dataStream: &dataStream))
        case .connectorsSectionStart:
            /// 0x4012 section
            self = .connectorsSection(try OrganizationChartConnectorsSection(dataStream: &dataStream))
        case .parentSectionEnd,
             .containerSectionEnd,
             .canvasSectionEnd,
             .globalInfoSectionEnd,
             .levelsSectionEnd,
             .levelSectionEnd,
             .boxTextPartsSectionEnd,
             .titleSectionEnd,
             .unknown0x600A,
             .fontSectionEnd,
             .linesSectionEnd,
             .shapesSectionEnd,
             .textStyleSectionEnd,
             .connectorsSectionEnd:
            throw OrgPlusChartReaderror.corrupted
        }
    }
}

public struct OrganizationChart {
    public let records: [OrganizationChartRecord]

    public init(data: Data) throws {
        var dataStream = DataStream(data)
        try self.init(dataStream: &dataStream)
    }
    
    public init(dataStream: inout DataStream) throws {
        guard let signature = try dataStream.readString(count: 4, encoding: .ascii) else {
            throw OrgPlusChartReaderror.corrupted
        }
        guard signature == "UOCF" else {
            throw OrgPlusChartReaderror.corrupted
        }
        
        var records: [OrganizationChartRecord]  = []
        while dataStream.remainingCount > 0 {
            let (id, data) = try dataStream.readOrganizationChartRecord()
            let record = try OrganizationChartRecord(id: id, data: data, dataStream: &dataStream)
            records.append(record)
        }

        self.records = records
        
        guard dataStream.remainingCount == 0 else {
            throw OrgPlusChartReaderror.corrupted
        }
    }
}
