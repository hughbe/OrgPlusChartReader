# OPX / OPS File Format Specification

Unofficial specification of the binary file format used by the Microsoft Organization Chart add-in (`.opx` charts and `.ops` style files). The format was licensed from Banner Blue Software's **Org Plus** product (hence "UOCF" and the name of this project).

Nothing here comes from official documentation. It was worked out from two sources:

- **The samples**: 109 files in [`Samples`](Samples) and [`Tests`](Tests), plus `TEMPLATE.MSO`, the template new charts start from.
- **The program**: the save and open code in `ORGCHART.EXE` (internal name `MSOCht32.EXE`, built 17 October 1995), extracted from [`Samples/msoc2.exe`](Samples/msoc2.exe). See §8.

The sample `.opx` files were saved by a **different build** of Org Chart from that executable. That build writes no banner (§4.2), puts 8 rather than 4 at `0x0FFD` offset 0, and only writes `0x0089` when a printer has been set up. None of these differences matters to the 1995 program when it opens a file. Everything else in the samples matches what the 1995 code writes.

### How to read this document

The format has one writer and one reader that can be studied (the 1995 program), and they don't treat every record the same way. Three cases come up throughout:

- **Written and read**: the normal case.
- **Written but not read**: the program saves the value but ignores it when opening a file. These are marked "ignored on read".
- **Read but not written**: the program accepts the record or value from files made by Org Plus, but never produces it. These are marked "read only".

§4.9 lists what the reader does with each record, and §6.1 describes what the program changes when it opens a chart that another Org Plus edition saved.

### Original terminology

The program's error messages and exported function names give the original names for many parts of the format:

| Original name | This document |
|---|---|
| OPX opcode | record |
| CRC | the footer checksums |
| GLOBALINFO | `0x0FFD` |
| global field names | the global `0x4008` field definitions |
| view | the canvas section `0x4003` |
| view info | `0x0020` PageInfo |
| VIEWUNITS | `0x0021` |
| label setup | `0x0023` BoxTemplate |
| peer group | `0x0041` Group |
| position | a box |
| silent position | box type 4: an invisible box that ends an assistant or co-manager group (§5.8) |
| print time code | a `0x0FFC` record in a box's text (§4.3) |
| amount field | a numeric field (label type 3) |
| titles and footers | the six chart title parts (`0x4009`) |
| free object | a drawing shape |
| bitmap data | the picture records |
| aux line, auxiliary line | a user-drawn connector (`0x0055`) |
| connecting line | a line from a parent box to a box in a group (`0x400C`) |
| background box, background line | drawing shapes (`0x400E`) |
| image | a picture shape |

## Contents

1. [Conventions](#1-conventions)
2. [Physical layout](#2-physical-layout)
3. [Logical structure](#3-logical-structure)
4. [Record reference](#4-record-reference)
5. [Enumerations](#5-enumerations)
6. [Quirks and hazards](#6-quirks-and-hazards)
7. [Open questions](#7-open-questions)
8. [Methodology](#8-methodology)

---

## 1. Conventions

- All integers are **little-endian**.
- Types: `u8`, `u16`, `i16`, `u32`, `i32`. Offsets in field tables are relative to the start of the record **payload**, after the 4-byte record header.
- **cb**: many fixed-size records start with a `u16` that repeats the payload length. These are marked `cb` and always equal the record length.
- **String**: text ending in a NUL byte. The record length includes the NUL. ✅ The code page is set by `0x0020` offset 14: 1 means ANSI (Windows-1252), and 0 means the OEM (DOS) code page, which the program converts with `OemToCharA`. Every sample uses ANSI and contains only ASCII.
- **Confidence**. Each field is marked:
  - ✅ **confirmed**: read from the program's save code, or varies in controlled samples exactly as described
  - 🟡 **probable**: consistent with the evidence but not pinned down
  - ❓ **unknown**: only observed values are given
- Records marked **(program only)** are written by the program's save code but appear in no sample.

### 1.1 Colours

✅ The program holds a colour as a 32-bit value `0xHHBBGGRR`: a Windows `COLORREF` plus a high byte `HH`. `HH = 0xFF` means "use the default colour". Colours are written in two ways:

| Encoding | Used by | Byte 0 | Byte 1 | Byte 2 | Byte 3 |
|---|---|---|---|---|---|
| **RBG4** | box fill, border, shadow, line, shape fill, background | red | **blue** | **green** | flag |
| **RGB4** | text colours (text-style properties `0x03`, `0x0A`, `0x0B`) | red | green | blue | `HH` |

The RBG4 byte order is a quirk of the program's writer. It copies byte 0, byte 2, byte 1 of the colour value. The samples agree: yellow is `ff 00 ff`, purple is `80 80 00`, and the 3D Gold fill is `c0 00 c0`.

The RBG4 flag byte is 1 when `HH = 0xFF` (box fill, shape fill, background) and 0 otherwise. When the flag is 1 the writer stores the program's default fill colour in the three colour bytes. Border, shadow and line records always write 0 there. Most RBG4 colours are followed somewhere in the record by a `u16` holding `HH`. That's why a default fill reads `ff ff ff 01 … ff 00`, and a shape border's "9" is its `HH`.

✅ **Reading.** The reader rebuilds a colour from the three colour bytes. For box fills, the background and palette entries, a flag byte of 1 means "default colour". It never reads an `HH` word: the ones in `0x0020`, `0x0051`, `0x0053`, `0x0056` and `0x0084` are ignored on read. A reader only needs the flag byte.

### 1.2 Coordinates

✅ Coordinates and distances are in **1/VIEWUNITS inch**. VIEWUNITS is set by `0x0021` (§4.4), and is 144 in every file the program writes, so the unit is 1/144 inch: half a point. When reading, the program converts each measurement to its internal 1/144-inch unit with `value × 144 / VIEWUNITS`. This also explains the line thicknesses: the default of 2 is 1 point, and the largest, 12, is 6 points.

Drawing coordinates are measured from the top-left of the page, with y increasing downwards.

---

## 2. Physical layout

```
+-----------------------------+
| "UOCF"  (4 bytes)           |  signature
+-----------------------------+
| records and sections ...    |  header records, then the root section 0x4001
+-----------------------------+
| slack bytes (usually none)  |
+-----------------------------+
| footer record 0x0FFF        |  last 14 bytes of the file
+-----------------------------+
```

### 2.1 Signature

✅ The file starts with the four ASCII bytes `UOCF` (`55 4F 43 46`). The program compares them case-insensitively when opening a file.

### 2.2 Items

After the signature, the file is a sequence of items. Each item starts with a `u16` id:

| id range | Kind | Layout after the id |
|---|---|---|
| `0x0000`–`0x3FFF` | **record** | `u16 length`, then `length` payload bytes |
| `0x4000`–`0x5FFF` | **section start** | nothing; child items follow |
| `0x6000`–`0x7FFF` | **section end** | nothing; the id is always `start id + 0x2000` |

✅ The program writes every item with one function, `WriteRecord(file, id, length, data)`. It writes the id, then the length and the data only when `length` is non-zero. Section markers are calls with `length = 0`, so a record can never be empty. Sections nest, and an end marker always closes the innermost open section.

✅ When reading, the program treats any id with bit `0x4000` set as a section marker.

✅ **Extended length.** A record length of `0xFFFF` is an escape: the real length follows as a `u32`, and the payload comes after that. The 1995 program never writes it. On reading, it rejects any extended length above `0xFFFE` with "MS Organization Chart does not support OPX opcodes with lengths greater than 64K", so the escape is presumably for other versions that allow larger records.

✅ Reading stops at the end marker of the root section (`0x6001`). The footer is found from the end of the file, not by continuing to parse.

✅ **Unknown records are skipped.** Every section reader is a loop that reads an item, handles the ids it knows and ignores the rest, until it sees its own end marker. A record in the "wrong" section is therefore ignored, not rejected. The reader also doesn't check a record's length against the layout it expects: it reads the fields it wants from a 64 KB buffer that holds the payload.

✅ **Limits** enforced by the reader:

| Limit | Value |
|---|---|
| record length | `0xFFFE` bytes |
| fields | 30 (indexes 0–29); a field index outside this range is ignored |
| field label | 255 characters |
| text of a box field, title part or text box | 29 000 characters (`0x7148`), including its style runs; longer text is cut |
| palette entries | 255 |
| lines in a box template | 30 |
| fields placed by a box template | 30 |

The program's own editing limits are 100 boxes in a group, 50 levels and a chart of 22 × 17 inches.

### 2.3 Footer and checksum

✅ The last 14 bytes of the file are the footer record:

| Offset in file | Type | Value |
|---|---|---|
| `F + 0` | u16 | id `0x0FFF` |
| `F + 2` | u16 | length `10` |
| `F + 4` | u16 | cb `10` |
| `F + 6` | u16 | **sum**: checksum 1 |
| `F + 8` | u16 | **rotxor**: checksum 2 |
| `F + 10` | u32 | `F`, the offset of the footer (the file length minus 14) |

✅ To write the footer, the program re-reads everything it has written, `[0, F)`, in `0xFFFE`-byte chunks, including the signature and any slack bytes. It then computes:

```c
uint16_t sum = 0, rotxor = 0;
for (size_t i = 0; i < F; i++) {
    sum    += data[i];                                   // modulo 2^16
    rotxor ^= data[i];
    rotxor  = (uint16_t)((rotxor << 1) | (rotxor >> 15)); // rotate left by 1
}
```

Both checksums match in every sample.

✅ **The program enforces the checksums.** When opening a file, right after checking the signature, it:
1. treats the last 14 bytes as the footer, based on the file length;
2. recomputes both checksums over everything before them;
3. checks that the record there is `0x0FFF`;
4. compares both checksums.

If any check fails, it refuses the file with "File appears to be damaged (Bad CRC)." It never reads the footer's file-size field.

A reader that wants to be more forgiving than the original can warn instead of refusing.

### 2.4 Slack bytes

✅ The root section is normally followed straight away by the footer. `Simple.opx` and `Box/Name1.opx` have extra bytes in between. They are leftovers from an earlier, longer save of the same file, and include a stale footer (and in `Simple.opx`, part of an older document). The program overwrites the file in place without truncating it, then writes the footer at the end. Readers must ignore slack bytes but include them in the checksum.

---

## 3. Logical structure

The tree below shows everything the program writes, in order. `[ ]` marks optional items and `*` marks items that repeat.

```
"UOCF"
 0x0D0A  Banner                          absent from the sample .opx files (§4.2)
 0x0001  Password
 0x0003  VersionA
 0x0002  VersionB
 0x4001  Root section
  0x4002  Document section
   0x4005  Global info section
    0x0010  FieldCount
    0x4008  Field definitions section
     (0x0011 FieldIndex, 0x0012 FieldKind, 0x0060 Text, [pictures], [field extras])*
    0x0FFD  GlobalInfo
   0x4003  Canvas section
    0x0021  ViewUnits
   [0x0090]                              program only
    0x0020  PageInfo (view info)
   [0x0091]                              program only
    0x0098  Recalculation
    0x0022  PageSetup
    0x0089  DevMode                      optional in the sample build
    0x009B  FitToPages
    0x400A  Box templates section
     0x0023*  BoxTemplate
    0x400B  Font table section
     ([0x009C FontInfo], 0x0025 FontName)*
    0x4010  Default text style section
     0x0061*  TextStyle
    0x4009  Chart title section
     (0x0026 TitlePartIndex, 0x0060 Text, [0x4010 TextStyle section])*  six parts
    0x0024
    0x0023  Default BoxTemplate for new boxes
    0x0052
    0x0053  Default Border            ┐
    0x0092  Default Shadow            │
    0x0054  Default BoxLayout         │ defaults (§3.4)
    0x0056  Default Line              │
    0x0093  Default LineShadow        │
    0x0095  Chart frame Border        │
    0x0096  Chart frame Shadow        ┘
    0x4006  Groups section
     (0x0041 Group, 0x0042 GroupLayout, [0x0093 LineShadow], 0x4007 Boxes section, 0x400C Lines section)*
    0x4012  Connectors section
     (0x0055 Connector, [0x0056 Line], [0x0093 LineShadow], [0x0088 Selected], [0x0094 Arrow]*)*
    0x400E  Drawing shapes section
     (0x0083 Shape, shape-type-specific records, [0x0097 Anchor], [0x0088 Selected])*
    [preserved records]*                 0x1000–0x13E7 (§3.5)
  end 0x6003, 0x6002, 0x6001
[slack bytes]
 0x0FFF  Footer
```

✅ The program never writes sections `0x4004`, `0x400D`, `0x400F` or `0x4011`.

✅ When the chart is saved for the clipboard or for a partial copy, the same routine writes the canvas section without the connectors and drawing shapes sections, and with only the selected group.

✅ **Required parts.** The reader refuses a file unless it has:

| Part | Error when missing |
|---|---|
| `0x0003` and `0x0002` before `0x4002` | "not a properly formatted OPX file" |
| `0x0FFD` GlobalInfo before the first `0x4003` | "The GLOBALINFO section of this file does not occur before the first view." |
| `0x0021` ViewUnits before `0x0020`, `0x0022` and `0x0091` in the canvas | "No valid VIEWUNITS found." |
| a `0x400A` section with at least one `0x0023` | "No label setup list was found." |
| `0x0020` PageInfo | "No view info was found." |
| at least one `0x0025` in a `0x400B` section | "The font list read was invalid." |
| in `0x4008` field definitions: `0x0011`, then `0x0012`, then `0x0060` for each field | "The global field name section of the file could not be read." |
| in `0x4006`: a `0x0041` before any group or box record, at least one `0x0051` in every group, and a box record only after its `0x0051` | "The peergroup structure of this file is invalid." |
| in `0x400E`: a `0x0083` before any other shape record | "File has a bad free object format." |

Only the first canvas section is read as the chart.

### 3.1 Org chart model

✅ The chart is a tree of **groups**. A group is a set of sibling boxes that share one parent box.

- Each group (`0x0041`) records its id, its parent group, its parent box, its box count and how many of those boxes are assistants.
- Groups are written depth-first: a group, then for each of its boxes in order, the groups below that box. A parent group always comes before its children, and the reader relies on this: it looks the parent group up by id as soon as the next group starts.
- Boxes are numbered `1, 2, 3 …` across the whole chart, in file order (`0x0050`). The reader finds a group's parent box by searching the parent group for the box with that number.
- `0x0020` PageInfo records the total box count (ignored on read).
- ✅ A group of **assistants** ends with a **silent position**: an invisible box of type 4 that the program adds after the last assistant. It is written like any other box and counted in the group's box and assistant counts.
- ✅ A **co-manager** group also ends with a silent position in memory, but that one isn't written: the writer leaves it out and subtracts it from the box count, and the reader adds it back when `0x0041` offset 16 is set. Groups below co-managers hang from this silent position, and are written with the group's first box as their parent box.

Example: `Plus/Level2.opx` contains

```
Group 0  parent box 0   boxes 1 "Name", 2 "Co Worker", 3 "Other Co-Worker"
Group 1  parent box 1   box  4 "Subordinate"
Group 2  parent box 4   boxes 5 "Assistant" (box type 2), 6 (box type 4, no text)    style 25
```

### 3.2 Boxes section (`0x4007`) and lines section (`0x400C`)

Each box in a boxes section is:

```
0x0050 BoxIndex
0x0051 Box
[0x0087]                box field = 0x68 (program only)
[0x0088 Selected]       box is selected
[0x009A]                box is excluded from calculations (program only)
0x0053 Border
0x0092 Shadow
[0x0054 BoxLayout]      only when it differs from the default (§3.4)
[0x4008 Box text section]    absent for box type 4
  (0x0011 FieldIndex, [pictures], [field extras], 0x0060 Text, [0x4010 TextStyle section])*
```

✅ After the boxes, the group's lines section holds the segments of the group's connecting lines (the lines from the manager to its boxes). The program writes one entry per segment, but only when that segment's line differs from the group's default, which is why the section is empty in every sample:

```
([0x0056 Line], [0x0094 Arrow], [0x0094 Arrow], [0x0088 Selected])*
```

✅ The line's `ItemIndex` (offset 6) is the zero-based number of the segment it styles. The number of segments is in the group's `0x0042` offset 10. The program's lines-section writer loops over the group's segment table (20-byte entries) and passes the loop counter as this index.

✅ When reading, a `0x0056` for segment 0 sets the thickness and colour of **all** the group's lines; one for any other segment only sets that segment's line style. A `0x0056` whose index is not below the group's segment count is ignored. A `0x0088` in this section selects the segment whose number is its value.

### 3.3 Drawing shapes section (`0x400E`)

Each shape starts with `0x0083` Shape. What follows depends on the shape type (§5.7):

| Type | Following records |
|---|---|
| 1 Rectangle | `0x0053` Border, `0x0092` Shadow, `0x0084` Fill |
| 2 Text box | `0x0053` Border, `0x0092` Shadow, `0x0084` Fill, `0x0060` Text, `0x4010` TextStyle section |
| 3, 4, 5 Lines | `[0x0056 Line]`, `[0x0093 LineShadow]` (each only when it differs from the default), then an `0x0094` Arrow for each arrowhead |
| 6, 8 Pictures (program only) | `0x0053` Border, `0x0092` Shadow, then picture records |
| 7 | never: these are connectors, written in `0x4012` |

✅ After these come an `0x0097` Anchor if the shape is anchored to a box, and an `0x0088` if it is selected.

✅ The program keeps connectors and shapes in one drawing list. The shapes section is written from the end of that list to the start, skipping connectors; the connectors section is written from the start to the end, skipping everything else.

### 3.4 Defaults and differences

✅ The canvas section writes default values, and the program keeps a copy of them. ✅ They aren't settings of their own: when a save starts, the program takes them from the **first box of the starting group** (`0x0024`): that box's border, shadow and layout become the canvas `0x0053`, `0x0092` and `0x0054`, its template the canvas `0x0023`, and its group's line and line shadow the canvas `0x0056` and `0x0093` (when the group has connecting lines; otherwise a 1 pt solid line in the default colour and no shadow). Some per-object records are written **only when they differ** from that copy:

| Record | Compared against |
|---|---|
| `0x0054` BoxLayout | canvas `0x0054` |
| `0x0056` Line (group lines, drawn lines) | canvas `0x0056` |
| `0x0093` LineShadow | canvas `0x0093` |
| Text-style properties | canvas `0x4010`, and the previous run (§4.6) |

Readers should start from the canvas defaults and apply per-object records over them. For example, `Lines/Line.opx` has no `0x0056` for its line, so the line uses the default thickness of 2.

### 3.5 Preserved records

✅ After the drawing shapes, the program writes back, unchanged, the contents of its objects of class `0x6F`. An object numbered *n* (0–999) is written as record `0x1000 + n`, so these records have ids `0x1000`–`0x13E7`. The `0x1000` record in the 3D Gold style file is one of them.

✅ **This program never creates such objects.** Its canvas reader skips `0x1000` like any other id it doesn't know, so a `0x1000` record is lost when the 1995 program opens and saves the file. 🟡 The writing code is shared with the full Org Plus, which presumably fills these objects itself; what `0x1000` holds isn't known.

---

## 4. Record reference

### 4.1 Index

| Id | Name | Length | Parent | Notes |
|---|---|---|---|---|
| `0x0001` | Password | 144 | top | |
| `0x0002` | VersionB | 10 | top | |
| `0x0003` | VersionA | 8 | top | |
| `0x0010` | FieldCount | 2 | `4005` | |
| `0x0011` | FieldIndex | 2 | `4008` | |
| `0x0012` | FieldKind | 2 | `4008` (global) | |
| `0x0013` | Formula | var | `4008` | program only; ignored on read |
| `0x0020` | PageInfo | 34 | `4003` | C#: `BodyInfo` |
| `0x0021` | ViewUnits | 8 | `4003` | |
| `0x0022` | PageSetup | ≥45 | `4003` | |
| `0x0023` | BoxTemplate | var | `400A`, `4003` | the copy in `4003` is ignored on read |
| `0x0024` | RootGroup | 2 | `4003` | |
| `0x0025` | FontName | var | `400B` | |
| `0x0026` | TitlePartIndex | 2 | `4009` | |
| `0x0041` | Group | 17 | `4006` | |
| `0x0042` | GroupLayout | 18 | `4006` | |
| `0x0050` | BoxIndex | 2 | `4007` | |
| `0x0051` | Box | 36 | `4007` | |
| `0x0052` | ❓ | 2 | `4003` | ignored on read |
| `0x0053` | Border | 16 | `4003`, `4007`, `400E` | |
| `0x0054` | BoxLayout | 20 | `4003`, `4007` | |
| `0x0055` | Connector | 26 | `4012` | |
| `0x0056` | Line | 14 | `4003`, `400C`, `4012`, `400E` | |
| `0x0060` | Text | var | many | |
| `0x0061` | TextStyle | 8 | `4010` | |
| `0x0070` | NumericValue | 8 | `4008` | program only; ignored on read |
| `0x0071` | NumberFormat | 18 | `4008` | program only; ignored on read |
| `0x0080` | PictureSize | 40 | `4008`, `400E` | program only |
| `0x0081` | BitmapHeader | var | `4008`, `400E` | program only |
| `0x0082` | BitmapData | var | `4008`, `400E` | program only |
| `0x0085` | PictureData | var | `4008`, `400E` | program only |
| `0x0083` | Shape | 22 | `400E` | |
| `0x0084` | Fill | 10 | `400E` | C#: `ShapeColor` |
| `0x0087` | PictureBox | 2 | `4007` | program only |
| `0x0088` | Selected | 2 | `4007`, `400C`, `4012`, `400E` | |
| `0x0089` | DevMode | var | `4003` | |
| `0x0090` | ColorPalette | var | `4003` | program only |
| `0x0091` | EmbeddedSize | 6 | `4003` | program only |
| `0x0092` | Shadow | 16 | `4003`, `4007`, `400E` | |
| `0x0093` | LineShadow | 16 | `4003`, `4006`, `4012`, `400E` | |
| `0x0094` | Arrow | 14 | `400C`, `4012`, `400E` | program only |
| `0x0095` | ChartFrameBorder | 16 | `4003` | |
| `0x0096` | ChartFrameShadow | 16 | `4003` | |
| `0x0097` | Anchor | 22 | `400E` | program only |
| `0x0098` | Recalculation | 4 | `4003` | |
| `0x0099` | FieldFlags | 4 | `4008` | program only; ignored on read |
| `0x009A` | ExcludeFromCalculations | 4 | `4007` | program only |
| `0x009B` | FitToPages | 10 | `4003` | |
| `0x009C` | FontInfo | 4 | `400B` | |
| `0x0D0A` | Banner | var | top | |
| `0x0FFC` | PrintTimeCode | var | `4005`, box text | read only: an Org Plus print time code (§4.3) |
| `0x0FFD` | GlobalInfo | 34 | `4005` | |
| `0x0FFF` | Footer | 10 | end of file | §2.3 |
| `0x1000`–`0x13E7` | Preserved record | var | `4003` | §3.5; 1864 bytes in the `.ops` sample |

| Section | Name | Section | Name |
|---|---|---|---|
| `0x4001` | Root | `0x4009` | Chart title |
| `0x4002` | Document (C#: "Container") | `0x400A` | Box templates |
| `0x4003` | Canvas | `0x400B` | Font table |
| `0x4005` | Global info | `0x400C` | Lines |
| `0x4006` | Groups | `0x400E` | Drawing shapes |
| `0x4007` | Boxes | `0x4010` | Text style |
| `0x4008` | Text fields | `0x4012` | Connectors |

### 4.2 Header records

#### `0x0D0A` Banner

✅ This is a copyright notice that doubles as a DOS `TYPE` trick. The id bytes are `0A 0D` (LF, CR). The length bytes are followed by two backspaces (`08 08`) that erase the length bytes on screen. Then comes the text, ending in `\r\n` and `0x1A` (Ctrl-Z, end of file in DOS) and a NUL. So `TYPE STYLE.OPS` prints "UOCF", a newline and a clean copyright message, then stops.

| Offset | Type | Value |
|---|---|---|
| 0 | u8[2] | `08 08` |
| 2 | string | "(c) Copyright 1992 Banner Blue Software Incorporated. … Banner Blue will aggressively pursue prosecution.\r\n\x1A" |

✅ The 1995 program always writes it. It builds the text from string resources `0x60F`–`0x615`. `TEMPLATE.MSO` and the `.ops` sample have it; the sample `.opx` files, written by another build, don't. The program ignores it when reading.

#### `0x0001` Password

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 144 | ✅ |
| 2 | u16 | password flag | ✅ Non-zero means the chart is password-protected. The 1995 program refuses such a file with "A password is required to open this file. MS Organization Chart does not support password entry." It always writes 0. |
| 4 | u8[140] | leftover data | ✅ Not written: whatever was left in the program's scratch buffer (§6). That is the banner text when a banner was written, otherwise zeros. ❓ In password-protected files from other versions this probably holds the password data. |

#### `0x0003` VersionA / `0x0002` VersionB

✅ The program writes these as constants. The date fields are day 26, month 12, year 92: 26 December 1992.

✅ When opening a file, the program reads the header records up to section `0x4002`:
- `0x0003` is required, otherwise the file is "not a properly formatted OPX file".
- `0x0003`'s version must be **1001 or lower**, otherwise "MS Organization Chart cannot open files with this version of OPX". So a newer Org Plus that raises this number writes files the 1995 program refuses.
- `0x0002` is required too. Its version and date are not checked; only its writer id (offset 2) is used.
- Records other than `0x0001`, `0x0002` and `0x0003` before `0x4002` are skipped, including the banner and the `0x4001` marker.

| Record | Offset | Type | Field | Value |
|---|---|---|---|---|
| `0x0003` | 0 | u16 | cb | 8 |
| | 2 | u16 | version | 1001 |
| | 4 | u8 | day | 26 |
| | 5 | u8 | month | 12 |
| | 6 | u16 | year | 92 |
| `0x0002` | 0 | u16 | cb | 10 |
| | 2 | u16 | ✅ writer id | 4 = written by Microsoft Organization Chart (every `.opx` sample, and what the 1995 program always writes). The `.ops` sample has 2. Any value other than 4 makes the reader treat the chart as one from another Org Plus edition and convert it (§6.1). |
| | 4 | u16 | version | 1010 |
| | 6 | u8 | day | 26 |
| | 7 | u8 | month | 12 |
| | 8 | u16 | year | 92 |

### 4.3 Global info

#### `0x0010` FieldCount

✅ `u16`: the number of text fields defined in the following `0x4008` section. This is 4 (Name, Title, Comment 1, Comment 2) in `.opx` samples and 2 (Name, Title) in the `.ops` sample.

#### `0x4008` Field definitions (global)

Each field is a `0x0011` FieldIndex (`u16`, zero-based), a `0x0012` FieldKind (`u16`, the label type below) and a `0x0060` Text holding the field's label ("Name", "Title", "Comment 1", "Comment 2"). Optional picture records and field extras may follow.

✅ FieldKind is the field's **label type**. The program's own error message lists the four types: "You cannot match up labels with different types (Text, Names, Amounts and Pictures)."
- **1 = Name**: the field has a name format (text-style property 8, §5.9).
- **2 = Text**: an ordinary text field.
- **3 = Amount**: field objects of internal type 2, the ones that carry number formats and calculation flags.
- **4 = Picture**: field objects of internal type 1. The reader creates a picture field for this type, and only fields of this type may carry picture records.

✅ **Reading field definitions.** The reader handles `0x0011`, `0x0012`, `0x0060` and `0x0080` and skips everything else. Fields with an index of 30 or more are dropped, and labels are cut to 255 characters.

✅ **This edition has no amount fields.** When the reader sees label type 3 it changes it to 2 (Text), and remembers to tell the user "Change your amount fields to text fields" (§6.1). The field keeps its text in every box but loses its number format, formula and stored values, because the records that hold them (below) are never read.

✅ After the definitions are read, each Name field is given a spare field slot (a field index with no definition) as a companion. A Name field that can't get one is turned back into a Text field. If the file has no Name field at all, its first Text field is made a Name field in the same way. 🟡 The companion slot holds the name as typed, before the name format rewrites it.

✅ **Field extras** (program only; ignored on read). Fields aren't just text: Org Plus supports **numeric and currency fields** and **calculated fields** that roll values up the chart. These records describe them. The 1995 program carries Org Plus's code for writing them, but nothing in its open path looks at `0x0013`, `0x0070`, `0x0071` or `0x0099`, and since it turns every amount field into a text field it never has one to write. They matter only to a reader of files from the full Org Plus. ✅ One routine writes them for both field definitions and box text, but with two switches, so which records appear depends on where the field is:

| Id | Length | Content |
|---|---|---|
| `0x0071` | 18 | the field's **number format**, always written in both places |
| `0x0013` | var | field definitions only: the field's **formula**, when the field has flag `0x20` |
| `0x0099` | 4 | box text only: `{cb = 4, the field's flag word}`. Bit 0 (`0x01`) = the text isn't a valid number. Bit 1 (`0x02`) = ✅ the field's amount **counts in calculations** (see below). Bit 5 (`0x20`) = the field has a formula. |
| `0x0070` | 8 | box text only: ✅ the field's **numeric value** as a little-endian IEEE 754 double, written only when the text parses as a number (flag `0x01` clear) |

✅ **Counting a field in calculations.** A new amount field in a box copies the field definition's number format and starts with flags `0x01` and `0x02` set. The formula accumulator only adds a box's field when its flag `0x02` is set (program property `0x6C`), the box isn't excluded (`0x009A`) and the box isn't hidden. Flag `0x02` is cleared per box and per field by the `IDD_EXCLAMOUNTS` ("exclude amounts") dialog. The help file's own name for these numeric fields is "Amount Fields".

✅ **`0x0071` number format.** The separators and symbol come from Windows' `[Intl]` settings when the field is saved:

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | cb = 18 |
| 2 | u16 | the text's flag `0x1000`, the same flag as text-style property 5 |
| 4 | char[4] | currency symbol (`sCurrency`), only for currency formats; otherwise empty |
| 8 | char | decimal separator (`sDecimal`) |
| 9 | char | thousands separator (`sThousand`) |
| 10 | u16 | 1 when the currency symbol goes before the amount (`iCurrency` 0 or 2) |
| 12 | u16 | decimal digits: the format's own value when it has flag `0x10`, otherwise `iCurrDigits` (currency) or `iDigits` |
| 14 | u16 | a value derived from the text's alignment |
| 16 | u16 | the format's flags: bit 0 = currency, bit 4 = explicit digit count |

The program's number formatting also reads `OrgPlus.ini` `[AMOUNTEDIT]` settings, such as `DigsPrePosExp`, `UseNegExp` and `OverrideDecDigs`.

✅ **`0x0013` formula.**

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | ✅ 5: the length of the fixed part, which is also the offset of the formula string |
| 2 | u16 | 0 for a formula string; 2, 3 or 4 for three predefined formulas |
| 4 | u8 | for a predefined formula, a field number; otherwise `0xFF` |
| 5 | string | the formula, when offset 2 is 0 |

The formula is `@` *function* `(` *scope* `,` `0x01` *field* `)`. The field reference is the byte `0x01` followed by the field's number as a raw byte. For example, `@sum(sub,` `01 02` `)` sums field 2 over a box's subordinates.

| # | Function | # | Scope |
|---|---|---|---|
| 0 | `avg` average | 0 | `ame` all managers above the box |
| 1 | `cnt` count of boxes whose value isn't zero | 1 | `ami` all managers above the box, and the box itself |
| 2 | `cmp` count of all boxes in the scope | 2 | `brn` the box's whole branch, including the box |
| 3 | `fot` **fraction of total**: this box's value ÷ the sum over the scope | 3 | `bre` the box's branch, excluding the box |
| 4 | `max` maximum | 4 | `sib` the boxes in the box's group, itself included |
| 5 | `min` minimum | 5 | `mgr` the box's immediate manager |
| 6 | `prd` product | 6 | `sub` the box's direct subordinates |
| 7 | `std` standard deviation | 7 | `sui` the direct subordinates and the box itself |
| 8 | `sum` sum | 8 | `all` all boxes |
| 9 | `var` variance | 9 | `lvl` the boxes on the box's level |
| | | 10 | `top` the top of the chart (the root group's boxes) |

✅ The function meanings come from the program's evaluator. It accumulates over each box in the scope, then finishes the result:
- `cnt` only counts boxes whose value isn't zero; `cmp` counts every box that passes the program's "is a box" test.
- `fot` divides the box's own value by the scope's total. A zero total gives 0, or 1e300 when the box's own value isn't zero.
- `std` and `var` keep a sum of squares.

✅ The scopes come from the recalculation routine (`0x50763A`), which runs one tree walker per scope. The walkers move through the chart with the program's navigation properties: `0x71` is a box's number of subordinates and `0x72` the *n*th one. Each walker carries an accumulator down the tree and works out a box's result before or after adding the box's own value:
- `mgr` resets the accumulator, adds the box, and passes it to each subordinate, so every box sees only its immediate manager. `ame` never resets, so a box sees every manager above it. `ami` adds the box's own value before working out its result.
- `sub` adds each direct subordinate, then works out the result. `sui` also adds the box's own value.
- `brn` and `bre` collect totals from the whole subtree: `brn` adds the box's own value, `bre` doesn't.
- `sib` resets once per group and adds every box in it. `lvl` collects per level. `all` collects over every box. `top` adds only the root group's boxes, then gives that result to every box.

So a trailing `i` means the box itself is included and `e` that it is excluded. 🟡 The expansions of the abbreviations (all managers, branch, siblings, subordinates and so on) are inferred from this behaviour. The Microsoft build carries Org Plus's calculation code but **no user interface text for it**: the string resources its formula code would load don't exist.

✅ The three predefined codes are used for three (function, scope) pairs when the formula's field is the one numbered six below the field that owns the formula: `@var(brn,…)` → 2, `@cnt(brn,…)` → 3, `@std(ami,…)` → 4. Offset 4 then holds the field number.

#### `0x0FFD` GlobalInfo (34 bytes)

✅ Built field by field from the document:

| Offset | Type | Field | Samples |
|---|---|---|---|
| 0 | u16 | ❓ record version? | 4 (1995 program, `.ops`, `TEMPLATE.MSO`); 8 (`.opx` samples) |
| 2 | u16 | ✅ **show branch markers**: document flag `0x02000000` is **clear** | When shown, the box painter draws a 9 × 9 marker in the bottom-right corner of every box that has boxes below it, and a click on it is hit-tested separately from the box. Markers are also hidden when the magnification is set below 50%, and when printing. Microsoft's edition forces the flag on (markers hidden): 0 in every sample. |
| 4 | u16 | ✅ show page breaks: document flag `0x04000000` is **clear** | When the flag is set, the paint routine skips the page-break and guide lines. Picture export sets it so they don't appear in the exported image. |
| 6 | u16 | boolean: document flag `0x00100000` | ❓ The program exposes this flag to other applications as property `0x74` of its external property interface; it isn't used anywhere else. |
| 8 | u16 | ✅ include the chart title in exported pictures | When the chart is copied as a picture, the title parts (other than the placeholder "Chart Title") are included in its bounds. 1 |
| 10 | u16 | ✅ include drawing objects in exported pictures | Drawing shapes (not connectors) are included in the exported picture's bounds. 1 |
| 12 | i16 | ✅ magnification | In tenths of a percent. 0 = Actual Size (100%); +N = a fixed zoom of N/10 % (500 = 50%, 2000 = 200%); −N = Size to Window, currently at N/10 %. The −564 in the samples is Size to Window at 56.4%. |
| 14 | u16 | ✅ default group style | §5.6 |
| 16 | u16 | ✅ a parameter stored with the default group style | 0 |
| 18 | u16 | ✅ default assistant style | Same codes as offset 14. Choosing an assistant style (internal 23–33) on the Styles menu stores it here instead of at offset 14. 25 (`.opx`), 29 (`.ops`). |
| 20 | i16 | ✅ current box: group id | The group id (`0x0041` offset 2) of the box the cursor is on; −1 when there is none. |
| 22 | i16 | ✅ current box: position in group | The zero-based position of that box within its group. (0, 0) is the first box of the root group. |
| 24 | u16 | 🟡 selection kind | 1 exactly when a box is selected (18 of 18 samples) |
| 26 | u8[4] | leftover data | ✅ never written (§6); "r Bl" in the `.ops` sample |
| 30 | u16 | ✅ horizontal scroll position | from `GetScrollPos` on the chart window; 0, 133 (`.ops`), 219 (`TEMPLATE.MSO`) |
| 32 | u16 | ✅ vertical scroll position | 0 |

✅ When reading GLOBALINFO, the program:
- **ignores offset 0**, so the 4-versus-8 difference between builds has no effect;
- reads offsets 8 and 10 and then **sets both to 1** regardless, so the two picture-export options are always on after a file is opened;
- reads offset 20 into both of the fields that offsets 20 and 22 come from, and ignores offset 22;
- ignores offsets 24–29, so the selection isn't restored;
- reads offsets 12, 30 and 32 only when it isn't importing the chart into an existing window;
- converts offsets 14 and 18 from file style codes to internal ones (§5.6).

GLOBALINFO must come before the first view, otherwise "The GLOBALINFO section of this file does not occur before the first view."

#### `0x0FFC` PrintTimeCode (read only)

✅ The 1995 program never writes this record but accepts it in two places. The program's own name for it is a **print time code**: "Print time codes were encountered in your file, and their fields have been replaced by counts."

**In the global info section**, only its presence matters; the contents are ignored. It marks an **older file format**, and changes how two things are read:
- the connector reader treats zero elbow distances as missing and substitutes 10 (§4.7);
- a group made only of assistants is given one more box than its `0x0041` says, because old files don't store the silent position that ends an assistant group (§3.1).

**In a box's text section** (`0x4008` inside `0x4007`), it follows the `0x0011` of a Name or Text field and holds a code string. The reader only acts on a code that starts with `#` (`0x23`): it sets a program-wide flag, then sets the field text's flag `0x400` if the second character is `S` or `s`, and flag `0x800` otherwise. Any other code is ignored.

✅ `#` is an **Org Plus Advanced head-count code**, and the exported `OPACountFn` ("OPA count") converts it when the chart is opened. When any `#` code was read from a chart that another edition wrote (`0x0002` offset 2 isn't 4), the open routine (`0x498AE4`):
1. Creates two amount fields, numbered 28 and 29, labelled with the program's strings "CntBrnMgr" and "CntBrnNoMgr".
2. Gives them the formulas `@cmp(brn, …)` (count every box in the branch, including the box at its head) and `@cmp(bre, …)` (the same without that box).
3. Runs `OPACountFn` on every box. For each field carrying a code, it clears the two text flags and clones the box's label setup. In the clone, the field's slot shows field 28, or field 29 for an `#S` code. The box is then switched to the clone, and boxes with the same template and choice share one clone.

So `#` means "show this box's branch head count here", and `#S` means "without the manager". The same routine also clears flag `0x02` (counts in calculations) on the amount fields of any box with box flag `0x800` (`0x0051` offset 14), then clears that box flag. It also runs when any box had `0x0051` offset 14 set.

### 4.4 Canvas records

#### `0x0020` PageInfo

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 34 | |
| 2 | u16 | ✅ box count | 1, 2, 5, 6 match the number of `0x0050` records. Ignored on read. |
| 4 | char[4] | ✅ default currency symbol | The writer stores the constant `"$"` and a NUL; the last two bytes are never written (leftover stack data: 0, or junk in `.ops` and `TEMPLATE.MSO`). |
| 8 | char | ✅ default decimal separator | constant `.` |
| 9 | char | ✅ default thousands separator | constant `,` |
| 10 | u16 | ✅ default "currency symbol before the amount" | constant 1 |
| 12 | u16 | ✅ default decimal digits | constant 2 |
| 14 | u16 | ✅ character set | 1 = ANSI (all samples), 0 = OEM code page. Other values: "MS Organization Chart does not support the character set used by this file." The 1995 program always writes 1. |
| 16 | RBG4 | ✅ background colour | flag byte 1 = default |
| 20 | u32 | ✅ a chart-wide spacing measurement (view units) | 22 in every sample. It is the chart's copy of a sixth group spacing value, one that isn't in `0x0054` (§4.5): when the program opens a chart from another edition it resets this field and each group's spacing to its defaults together, and reports "Change the spacing between boxes". |
| 24 | u32 | ✅ guide inset 1 | A draggable guide's position, in view units measured in from the page margins. The paint routine draws guide lines at margin + inset on each side. 0 (`.ops`: 144 = 1 inch). |
| 28 | u32 | ✅ guide inset 2 | A second guide, set by dragging. 0 (`.ops`: 144). |
| 32 | u16 | ✅ background colour `HH` | `0x00FF` with the default white, otherwise 0. Ignored on read. |

✅ Offsets 4–13 have the same layout as the middle of a `0x0071` number format (§4.3): symbol, separators, symbol position, digits. They are the chart's default number format, which this edition fixes at `$`, `.`, `,`, symbol first, two digits. The reader ignores them, and only uses offsets 14–31.

#### `0x0021` ViewUnits

✅ Called VIEWUNITS by the program. It sets the unit for every coordinate and distance in the file (§1.2).

| Offset | Type | Field | Value |
|---|---|---|---|
| 0 | u16 | cb | 8 |
| 2 | u16 | ❓ | 0 |
| 4 | u16 | base units per inch | 144 |
| 6 | u16 | percentage scale | 100 |

Units per inch = offset 4 × offset 6 / 100. The program always writes `{8, 0, 144, 100}`. When reading, it rejects a file where the result is 0 ("No valid VIEWUNITS found").

#### `0x0098` Recalculation, `0x0024`, `0x0052`, `0x009B` FitToPages

| Id | Content | |
|---|---|---|
| `0x0098` | `{4, v}`: v = 1 unless document flag `0x08000000` is set. ✅ When that flag is set, the program **recalculates every formula** (`0x50763A`, which runs the evaluator for each formula scope) after several kinds of edit (including editing box text). So v = 0 means automatic recalculation is on. ✅ The only thing that sets the flag is reading this record: the global-info reader sets it when v is 0. This edition then turns it off again: the open routine clears it before its own "recalculate on open" check, so that check can never fire, and nothing in the UI turns it back on. The program therefore always saves v = 1, as every sample has. A file from the full Org Plus with v = 0 would be read correctly but would lose the setting when saved. | ✅ |
| `0x0024` | ✅ `u16`: the **id of the root group** (`0x0041` offset 2). The writer starts the groups section from the group with this id, and the reader stores it back. When part of a chart is saved (a copy), it is the id of the group the copy starts from. | ✅ 0 in every sample |
| `0x0052` | ✅ `u16`: the layout style (§5.6) of the **starting group**, the group named by `0x0024`. Ignored on read. | ✅ 0 in every sample |
| `0x009B` | ✅ **Fit to pages** when printing: `{cb = 10, enabled, scale, pages across, pages down}`. The print layout makes the chart cover pages-across × pages-down pages, allowing for overlaps, and writes the resulting scale back to offset 4 in thousandths (0 = none). `0a 00 00 00 00 00 01 00 01 00` (off, one page by one page) in every sample. | |
| `0x0090` | ✅ program only: the chart's **colour palette**, the colours offered by the Color dialog. `{cb = 4 + 4n, n}` then *n* RBG4 colours. An entry that is still one of the program's 16 standard colours is written from its built-in table with the flag byte set to 1. | |
| `0x0091` | ✅ program only: `{cb = 6, x, y}`, two measurements. Only written when the chart is saved for **embedding in another document** (OLE): the program then saves to a temporary file. 🟡 The size of the embedded object. | |

#### `0x0022` PageSetup

✅ Page and layout setup. Fields marked "measurement" are in view units (§1.2) and converted when read.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb of the fixed part = 44 | ✅ |
| 2 | u16 | layout direction | ✅ must be 0, top to bottom. Other values: "MS Organization Chart can only import charts that are formatted from top to bottom." |
| 4 | u16 | orientation | ✅ 0 portrait, 1 landscape (from the printer's `dmOrientation`) |
| 6 | u16 | ❓ | 0 |
| 8 | u16 | ✅ **centre the chart vertically** on its pages (document flag `0x00040000`) | When set, the pagination routine places the chart midway between the top and bottom margins; when clear, the chart keeps its own y position (offset 40), moved only as far as needed to stay inside the margins. 0 in the samples. Cleared in charts from another edition. |
| 10 | u16 | ✅ **centre the chart horizontally** on its pages (document flag `0x00020000`) | As offset 8, for the x position (offset 36) and the left and right margins. 1 in the samples. Set in charts from another edition. |
| 12 | u32[4] | ✅ page margins: left, right, top, bottom | The page-break drawing tests offset 12 against the clip rectangle's left and right edges and offset 20 against its top, and draws the right and bottom margins at page size minus offsets 16 and 24. 0, 72, or 37/37/27/27 (`TEMPLATE.MSO`). |
| 28 | u32 | ✅ **printer edge allowance** | 14 when the printer reports an unprintable strip on both axes, otherwise 0: a new chart and the `0x0089` reader both set it this way, and the page-break drawing adds it to the page margins. 0, 7, 14 in the samples. |
| 32 | u16 | ✅ paper size | the printer's `dmPaperSize` (1 = Letter) |
| 34 | u16 | ✅ use the default printer | 1: the chart prints to the default Windows printer, and the reader drops a saved printer name that differs from it. 0: the chart keeps its own printer. 1 in the samples. |
| 36 | u32 | ✅ chart anchor x | The horizontal centre line of the chart's top box. The program moves objects to (x, y) and subtracts a half-width from x. New charts start at 90; 611–639 in the samples (a side shadow moves it by 2). |
| 40 | u32 | ✅ chart anchor y | The top of the chart. New charts start at 18; 41 and 139 in the samples. |
| 44 | string | ✅ printer as `device,driver,port` | e.g. `HP LaserJet 4/4M Plus PS 600,adobeps,LPT2:`. Empty (a single NUL) in most samples. When reading, the program compares it with the current Windows printer. |

#### `0x0089` DevMode

✅ A Windows `DEVMODE` structure for the printer named in `0x0022`: `dmDeviceName[32]`, `dmSpecVersion` (`0x030A` or `0x0400`), `dmDriverVersion`, `dmSize`, `dmDriverExtra`, `dmFields`, `dmOrientation`, `dmPaperSize`, `dmPaperLength`, `dmPaperWidth`, `dmScale`, `dmCopies`, `dmDefaultSource`, `dmPrintQuality` and so on, followed by driver-private bytes.

✅ The 1995 program writes it unconditionally, as the whole of its printer-settings block. The build that wrote the samples only includes it when a printer has been configured (1 sample of 104). When reading, the program replaces its printer settings with the record, and re-opens the printer if the device name, `dmDriverVersion` or `dmSize` differ from the current ones. It then sets `0x0022` offset 28 to 14 or 0 from what the printer reports.

#### `0x0023` BoxTemplate

✅ Called a "label setup" by the program. It is a small **byte-code program** that lays out a box's text fields line by line. Boxes refer to it by id (`0x0051` offset 32).

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | ✅ template id | A bitmask of the fields shown: `0x01` = Name only, `0x0F` = all four. Id 0 lists every field (the master template). |
| 2 | u16[4] | ✅ inner margins: top, left, bottom, right | The reader builds a Windows `RECT` (left, top, right, bottom) from offsets 4, 2, 8, 6. 8 in charts, 4 in the style file. |
| 10 | u8[] | ✅ layout program | opcodes, ending with `00` |

The opcodes:

| Opcode | Operand | Meaning |
|---|---|---|
| `00` | – | end of the program |
| `01` | u8 | place a field; the operand is the field index, or `0xFF` for none |
| `02` | u8 | ✅ a **vertical line** inside the box, to the right of the field just placed. `02 01` keeps it inside the box's inner margins; `02 00` runs it the full height to the border. |
| `03` | u8 | ✅ a **horizontal line** inside the box, under the row of the field just placed. `03 01` keeps it inside the inner margins; `03 00` runs it the full width to the border, following a rounded or cut-corner shape. |
| `04` | – | marks the line selected by one of the label setup's line indexes |
| `05` | – | marks the line selected by its other line index |
| `06` | – | end of a line |
| `09` | – | ✅ the field just placed is **hidden**: it stays in the template but isn't displayed |

For example, `04 01 00 06 01 01 06 01 02 06 01 03 06 00` is: marker, field 0, end of line, field 1, end of line, field 2, end of line, field 3, end of line, end. Every sample's templates decode completely with these opcodes.

✅ The program's parser confirms these opcodes. It also accepts `07` and `08` (with no operand) and ignores them, and allows at most 30 lines. Each field placed becomes a 20-byte entry: kind, `03` setting, `02` setting, field index, flags. `02` stores its setting on the field entry (1 for operand 1, 2 for operand 0) and changes the field entry's kind to 4. `03` stores its setting the same way but changes the kind of the current **line** to 4 instead. `09` sets the entry's flag `0x01`.

✅ **The two markers split the lines into three sections**: lines before marker `04`, lines between the markers, and lines from marker `05` onwards. The program's text export uses the first and last sections to find which fields hold each box's name and title. If those sections are empty, as in every sample (marker `04` is on line 0 and there's no `05`), it falls back to matching the field labels "Name" and "Title". The export also counts the fields whose `03` setting is non-zero.

✅ **The `02` and `03` settings are the lines Org Plus can draw inside a box** between fields. The box painter (`DrawPosition`, `0x431EDC`) draws them after the field's text: for `03`, a horizontal line at the bottom of the field's row (2 units lower for the full-width form); for `02`, a vertical line 2 units to the right of the field. When the program opens a chart from another edition, it checks every template for a field with either setting and, if it finds one, reports "Remove lines inside boxes" (§6.1). ✅ **Opcode `09` hides a field.** The routine that builds the template the program actually lays out (`0x42AA07`) leaves out any row whose fields are all flagged, the program's property interface reports a flagged field as not shown, and the routine that lists a template's fields skips flagged ones. This is how Org Plus keeps a field's data in a box without displaying it; when the program has to shrink an oversized chart it offers to "Hide all but the first text field (the information will be retained but not displayed)".

✅ The writer emits a field's opcodes in the order `01`, `09`, `02`, `03`, and writes `0xFF` as the field index when the field no longer exists.

The `0x400A` section holds every template in use. The canvas-level `0x0023` is the template given to new boxes and is a copy of one in `0x400A`. ✅ The reader ignores the canvas-level copy. Every `0x0051` template id appears in `0x400A` (all samples).

#### `0x400B` Font table

Each font is an optional `0x009C` FontInfo followed by a `0x0025` FontName. ✅ The program only writes `0x009C` when a program-wide version word is at least 4, probably Windows 95 or later, which is why the `.ops` sample has none. Text styles refer to fonts by index (§5.5, property `0x02`).

✅ **Reading.** A `0x009C` applies to the `0x0025` that follows it, and only when both carry the same font index (and the version word is at least 4). The reader looks the face name up among the installed fonts, with the character set if it has one, then by name alone, and otherwise substitutes a font. Font indexes may have gaps. A text style that names a missing index gets the first font that exists.

| Record | Offset | Type | Field |
|---|---|---|---|
| `0x009C` | 0 | u16 | ✅ font index |
| | 2 | u16 | ✅ the font's Windows character set (`LOGFONT.lfCharSet`): 0 ANSI; `0xA1` (Greek) for Century Gothic in one sample |
| `0x0025` | 0 | u16 | ✅ font index |
| | 2 | string | ✅ face name, e.g. "Arial", "Arial Narrow", "Arial Black", "Times New Roman" |

⚠️ In `PurpleBackground.opx` the face names are binary junk (`a0 8a 85 59 6a 01`), so readers must not assume they are printable.

#### `0x4009` Chart title

✅ There are always six parts (indexes 0–5) laid out as two rows of three. The default alignments are left, centre, right, left, centre, right (text-style property `0x07`). Part 1, top centre, holds the main title ("Organization Chart Title"). Unused parts have empty text.

✅ The program calls the parts "titles and footers". Microsoft's edition only supports part 1: when it opens a chart from another edition it empties any other part that has text and reports "Exclude some of your titles and footers" (§6.1). 🟡 So parts 0–2 are the left, centre and right titles and parts 3–5 the footers. A part index of 30 or more is ignored on read.

Each part is a `0x0026` TitlePartIndex (`u16`), a `0x0060` Text and an optional `0x4010` style section.

### 4.5 Groups and boxes

#### `0x0041` Group

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 17 | ✅ |
| 2 | u16 | group id | ✅ The group's id in the program's object table (class `0x65`). Connectors refer to groups by this id. Ids are assigned when groups are created, so in a new chart they match file order. |
| 4 | u16 | assistant-part group id | ✅ The program writes the group id here too. When reading, it uses this id for the group's assistant part. |
| 6 | i16 | parent group id | ✅ −1 for the root group, and for the first group of a partial copy |
| 8 | u16 | parent box number | ✅ the `0x0050` number of the parent box; 0 for the root group |
| 10 | u16 | box count | ✅ the number of `0x0050` records in the following `0x4007` |
| 12 | u16 | assistant count | ✅ how many of the group's boxes are assistants. The 1995 program only writes 0 or the whole box count: a group is either all assistants or has none. |
| 14 | u8 | group flag `0x20` | ✅ When set, a silent position (box type 4) is allowed anywhere in the group, and its boxes get a marker the layout code uses. 0 in the samples. |
| 15 | u8 | group flag `0x40` | ✅ written; ignored on read |
| 16 | u8 | co-manager | ✅ 1 when the group uses the co-manager layout (style 34) |

✅ **How the reader builds groups.** A file group can hold both ordinary boxes and assistants, and the reader turns it into up to two groups in memory:

| File group | In memory |
|---|---|
| no assistants (offset 12 = 0) | one group of offset 10 boxes, plus a silent position at the end if offset 16 is set |
| all assistants (offset 12 = offset 10) | one assistant group with the id at offset 4, of offset 12 boxes (plus one silent position if the file has `0x0FFC`, §4.3) |
| mixed | an ordinary group of (offset 10 − offset 12) boxes with the id at offset 2, and an assistant group of offset 12 boxes plus an added silent position, with the id at offset 4. Boxes of type 2 go to the assistant group, all others to the ordinary one. |

A file group with no boxes creates no group, so the records after it are an error. An assistant group starts with the internal style 29 until its `0x0042` is read. The reader attaches each group to its parent when the next `0x0041` arrives (or the section ends): it looks up the parent group by id and finds the box numbered offset 8 in it. If the parent is a co-manager group, the group is attached to that group's silent position instead. A missing parent box is an error ("The peergroup structure of this file is invalid.").

#### `0x0042` GroupLayout

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 18 | ✅ |
| 2 | u32 | ✅ view units | Normally a value the program stores with the group but its layout routines never use (0). When offset 16 is `0xFFFF`, it is instead the **position of the group's stem**: the x position, measured from the left of the group, of the vertical line that comes down from the parent box. |
| 6 | u32 | ✅ **extra drop** (view units) | 0. Added to the vertical distance between the parent box and the group, on top of the two gaps in `0x0054` (§4.5). |
| 10 | u16 | ✅ number of connecting-line segments | The number of segments the group's connecting lines are drawn with, which is the number of entries the following `0x400C` can have (§3.2). The program recalculates it when it lays out the chart, so the reader ignores it. `0x08`, `0x0D`, `0x11` in the samples. |
| 12 | u16 | ✅ layout style of the group | §5.6; 0, 3, 25 in the samples |
| 14 | u16 | ✅ layout style of the group's assistant part | The reader uses offset 14 for an assistant group and offset 12 for an ordinary one (§4.5 `0x0041`). The 1995 program writes the same value in both. |
| 16 | u16 | ✅ **stem position**: where the line from the parent box meets the group | 0 = centred (the default); 1 = at the first box; 2 = at the last box; `0xFFFF` = at the position in offset 2, kept between the first and last boxes; 3 is treated as 0 by the placement code. The layout routine for the row styles (`0x421E72`) applies it; a group of one box is always centred. The exported `_PGSetBranchID` resets offsets 2 and 6 to 0 whenever the style changes, and sets this one for only two styles, those written as 5 and 18 (§5.6): 1 when the group's previous style was one of three particular styles, otherwise 0. 0 in the samples. |

#### `0x0050` BoxIndex

✅ `u16`: the one-based number of the box within the whole chart.

#### `0x0051` Box

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 36 | ✅ |
| 2 | u16 | box type | ✅ §5.8 |
| 4 | u16 | 0 | ✅ ignored on read |
| 6 | u16 | ✅ **hidden box** (box flag `0x04`) | The flag set by the exported `_PGShowPos` (Hide Box / Show Box) on each selected box. A box of type 3 is also read as hidden. A chart from another edition with hidden boxes gets "Exclude hidden boxes" (§6.1). 0 in the samples. |
| 8 | u16 | ✅ **hidden branch** (box flag `0x10`) | Everything below the box is hidden (`_PGShowBr`, Hide Branch / Show Branch). When the program opens a chart from another edition it deletes the groups below such a box, clears the flag and reports "Exclude hidden branches". |
| 10 | u16 | box flag `0x01` | ✅ 1 on every visible box in the samples, 0 on the silent position. Both this flag and the next must be set on every ordinary box in Microsoft's edition: when it opens a chart from another edition it sets any that are clear and reports "Change the size of some boxes". 🟡 So they are the two halves (width and height) of "size the box to fit its text". |
| 12 | u16 | box flag `0x02` | ✅ See offset 10; 1 on every visible box, 0 on the silent position. |
| 14 | u16 | box flag `0x800` | ✅ The program writes 0, but its reader sets box flag `0x800` from it. When the chart is opened, `OPACountFn` turns that flag into "this box's amounts aren't counted" (it clears field flag `0x02`, §4.3), then clears the box flag. 🟡 A setting written by the full Org Plus. |
| 16 | u16 | 0 | ✅ ignored on read |
| 18 | u16 | box flag `0x200` | ✅ The program writes 0, but its reader sets box flag `0x200` (excluded from calculations, the same flag as `0x009A`) from it. |
| 20 | u32 | ✅ **extra horizontal space before the box** (view units) | 0. The layout routine adds it to the running position along the row before placing the box. The exported `_PGSetBranchID` resets it to 0 for every box of a group whose style changes, together with the group's `0x0042` offsets 2 and 6. |
| 24 | u32 | ✅ a value kept with the box (view units) | 0. Reset together with offset 20. Neither layout routine uses it. |
| 28 | RBG4 | fill colour | ✅ flag byte 1 when `HH = 0xFF`, and then the program's default fill colour is written |
| 32 | u16 | box template id | ✅ refers to `0x0023`; `0xFFFF` for type-4 boxes |
| 34 | u16 | fill colour `HH` | ✅ ignored on read |

✅ **Reading a box.** Each `0x0051` fills the next box slot of its group: a type-2 box goes to the assistant group when the file group is mixed, any other to the ordinary group. The records that follow (`0x0087`, `0x0088`, `0x009A`, `0x0053`, `0x0092`, `0x0054`, `0x4008`) apply to the box most recently read. The box takes its number from the `0x0050` before it. Before any of its records are read, a box starts with the canvas defaults (`0x0053`, `0x0092`, `0x0054`), the default fill and template id −1.

#### `0x0054` BoxLayout

✅ Only written for a box when it differs from the canvas default (§3.4). It holds the box's size and its group's spacing, all in view units. The exported `_PGSetSpace` copies a spacing structure into exactly the group fields used here.

| Offset | Type | Field | Samples |
|---|---|---|---|
| 0 | u16 | cb = 20 | |
| 2 | u16 | 0 | ignored on read |
| 4 | u16 | 0 | ignored on read |
| 6 | u16 | ✅ **box width** | 180 = 1¼ inch (`.ops`: 90) |
| 8 | u16 | ✅ **box height** | 36 = ¼ inch, or 180 (`.ops`: 18) |
| 10 | u16 | ✅ **gap from the parent box down to the connecting line** | 22, or 0 on boxes |
| 12 | u16 | ✅ **gap from the connecting line down to the group's boxes** | 22 |
| 14 | u16 | ✅ **horizontal gap between boxes** | 22 |
| 16 | u16 | ✅ **vertical gap between stacked boxes** | 14 |
| 18 | u16 | ✅ never written: leftover stack data | 22. Ignored on read, but the writer does include it when it compares a box's record with the default. |

✅ **Box size.** The program builds a box's starting rectangle as (0, 0, width, height) from offsets 6 and 8. ✅ The box-sizing routine (`0x440DAD`) then makes the box at least as large as its text. The stored width, less the box's insets, is the smallest the text area may be, except in one sizing mode where a box with either of `0x0051` offsets 10 and 12 set may shrink to 18 units. 🟡 So those two flags mean "fit the box to its text", one for each direction. A chart from another edition whose boxes don't have the program's default pair gets them reset, with the message "Change the size of some boxes".

✅ **Spacing.** The names come from the two layout routines (the column routine `0x41D011` and the row routine `0x421E72`, chosen by layout style, §5.6):
- The distance from the bottom of the parent box to the top of the group is offset 10 + offset 12 + the group's `0x0042` offset 6. The horizontal connecting line is drawn offset 12 above the group's boxes.
- Offset 14 is added between neighbouring boxes, and between neighbouring branches, along a row.
- Offset 16 is added between boxes that a style stacks in a column. For internal layout styles 4, 9, 13, 14, 18, 19, 28 and 33 (§5.6) the program keeps a separate value for this: offset 16 is written from it instead, and read into both.
- A sixth spacing value, the one in `0x0020` offset 20, is stored with each group but isn't used by either layout routine.

The program resets all six when it opens a chart from another edition, with the message "Change the spacing between boxes". No spacing dialog survives in the program's resources and `_PGSetSpace` has no callers inside the program.

✅ When reading, the box part (offsets 6 and 8) goes to the box and the group part (offsets 10–16) to the box's group, so the last box's record wins for the group. All values are converted from view units.

### 4.6 Styling records

#### `0x0053` Border

`0x0095` has the same layout.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 16 | ✅ |
| 2 | u16 | border style | ✅ §5.1 |
| 4 | u16 | ✅ **box shape** | 0 rectangle, 1 rounded rectangle, 2 octagon (§5.10). The value set by the exported `_PGSetShape`. Microsoft's edition has no command for it; 0 in every sample. |
| 6 | u16 | shadow preset | ✅ §5.3. A summary of the `0x0092` that follows, kept for older readers. |
| 8 | u16 | line style | ✅ §5.2. On read, 1 and 2 are solid, 3 dotted and 4 dashed; 0 and anything else leave the border solid, because "no border" is carried by border style 0. |
| 10 | RBG4 | border colour | ✅ flag byte always 0 |
| 14 | u16 | border colour `HH` | ✅ 0, or 9 on drawing shapes. Ignored on read. |

✅ The exported name for the line style of a border is the "border pattern" (`_PGSetBorderPat`).

#### `0x0092` Shadow

`0x0093` LineShadow and `0x0096` have the same layout.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 16 | ✅ |
| 2 | u16 | shadow type | ✅ §5.3. In `0x0092` and `0x0096` the reader accepts 0–9 and turns anything else into 9. `0x0093` uses the line-shadow codes instead (§5.3). |
| 4 | i16 | vertical offset | ✅ + down, − up (Top = −4); view units |
| 6 | i16 | horizontal offset | ✅ + right, − left (LeftBottom = −4); view units |
| 8 | RBG4 | shadow colour | ✅ flag byte always 0 |
| 12 | RBG4 | secondary colour | ✅ white in the samples; 🟡 a bevel highlight |

✅ `0x0093` is a **line shadow**. The program writes it alongside a `0x0056` line for the canvas default, groups, connectors and drawn lines. ✅ `0x0095` and `0x0096` are the border and shadow of the **chart frame**: a border the program can draw around the chart at the page margins. Its rectangle runs from the left and top margins to the page width and height less the right and bottom margins (`0x0022` offsets 12–27), enlarged by the border's own widths, and it is drawn with the same painters as a box border and shadow. It isn't drawn when the border style is 0, as in every sample, or when it would fall outside the printer's printable area. Microsoft's edition has no command for it.

#### `0x0056` Line

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 14 | ✅ |
| 2 | u16 | line style | ✅ §5.2. On read, 0 = no line, 3 = dotted, 4 = dashed; every other value is solid. |
| 4 | u16 | thickness (pen width) | ✅ §5.4; view units |
| 6 | u16 | item index | ✅ In a lines section, the zero-based number of the connecting-line segment (§3.2); 0 elsewhere. |
| 8 | RBG4 | colour | ✅ flag byte always 0 |
| 12 | u16 | colour `HH` | ✅ 0 or 9. Ignored on read. |

#### `0x0094` Arrow (program only)

✅ An arrowhead on a line, matching the program's exported `_PGSetArrow`. A line can have one at each end.

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | cb = 14 |
| 2 | u16[3] | 0 |
| 8 | u16 | end: 0 for one end (line flag `0x08`), 1 for the other (line flag `0x10`) |
| 10 | u16 | 0 |
| 12 | u16 | item index, as in `0x0056`. Used only in a lines section, where the reader also requires cb ≥ 14. |

✅ The reader uses only offsets 8 and 12.

#### `0x4010` / `0x0061` TextStyle

✅ Text is rich text: formatting can change part-way through. A text-style section is a list of 8-byte property records:

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | property id (§5.5) |
| 2 | u32 | value |
| 6 | u16 | **character position**: for per-run properties, the zero-based character where the run starts; 0 for paragraph properties |

The text is stored as a list of runs. For each run, the program writes per-run properties only when they change from the previous run. `Plus/Level2.opx` has a box whose font changes to font 1 at character 8.

✅ **Reading runs.** The reader keeps a current run, which starts at character 0 with the canvas defaults. For each per-run property:
- if its position equals the current run's start, the property changes the current run;
- if its position is greater, the current run is ended there and a new run starts, copying the old one's formatting, with the property applied;
- if its position is less, or above 29 000, the property is ignored.

So per-run properties must be in ascending order of position, as the writer produces them. A text can't hold more runs than fit, with the text, in 29 000 bytes at 80 bytes a run.

The canvas section's `0x4010` lists all the defaults. Sections on boxes, title parts and shapes only list paragraph properties that differ from those defaults, plus each run's changes.

#### `0x0060` Text

✅ A string. Empty text is a single NUL. The samples never contain line breaks.

✅ A line break is a carriage return (`0x0D`). When the program opens a chart from another edition, it cuts a wrapped field (text-style property 6) at its first carriage return and reports "Remove all text following a <return> from positions" (§6.1).

✅ When reading, the text's length is the record length minus one, whatever the bytes are. If the file's character set is OEM (`0x0020` offset 14 = 0) the text is converted with `OemToCharA`.

### 4.7 Connectors

#### `0x0055` Connector

✅ A user-drawn connector between two boxes (an "auxiliary line" in the program's interface). In memory, connectors are drawing objects of internal type 7; the program writes them here instead of in `0x400E`. A box can have at most 4.

✅ The records after it apply to this connector, in this order when written:

| Record | Written when |
|---|---|
| `0x0056` Line | the connector's line differs from the canvas default (it does in all three samples) |
| `0x0093` LineShadow | its shadow differs from the canvas default |
| `0x0088` Selected | it is selected |
| `0x0094` Arrow | once for each arrowhead |

The reader accepts them in any order, and starts each connector from the canvas default line and line shadow.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 26 | ✅ |
| 2 | u16 | from: box number | ✅ the box's `0x0050` number. The reader looks for it in the group at offset 4 and rejects the file if it isn't there ("The peergroup structure of this file is invalid."). |
| 4 | u16 | from: group id | ✅ `0x0041` offset 2 |
| 6 | u16 | from: anchor side | ✅ Which edge of the start box the line leaves from: 0 any, 1 left, 2 top, 3 right, 4 bottom (the order of a Windows `RECT`). Before drawing, the program swaps the two ends so the higher-priority anchor comes first: top > bottom > left > right > any. |
| 8 | u16 | to: box number | ✅ as offset 2, in the group at offset 10 |
| 10 | u16 | to: group id | ✅ `0x0041` offset 2 |
| 12 | u16 | to: anchor side | ✅ As offset 6, for the end box. In the samples Co6 runs from box 1's right side to box 5's right side. |
| 14 | u32[3] | ✅ **segment offsets**, view units | 18, 18, 18 (1/8 inch). The connector is an angled (elbow) line of up to three movable middle segments, drawn by the angled-line tool (`DP_AnglHandler`). Each value places one of those segments: it is the distance of the segment from the edge it is measured from (added to, or subtracted from, a left or top coordinate depending on the anchor side). The first is how far the line runs out from the start box before its first bend. Dragging a segment's handle changes its value. When reading a file with `0x0FFC`, the program replaces 0 values with 10. |

### 4.8 Drawing shapes

#### `0x0083` Shape

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 22 | ✅ |
| 2 | u16 | shape type | ✅ §5.7 |
| 4 | u32 | **top** | ✅ |
| 8 | u32 | **left** | ✅ |
| 12 | u32 | **right** | ✅ |
| 16 | u32 | **bottom** | ✅ |
| 20 | u16 | flag | ✅ For lines, shape flag `0x04`: which diagonal of the bounding box is drawn. 0 for most other shapes. |

The fields are edges, not a position and size. For example, a horizontal line has `top = 486, bottom = 488`, and a vertical line has `left = 1011, right = 1013`. ✅ The file holds the shape's own geometry. In memory the program's rectangle is larger, and the writer shrinks it: for lines (types 3, 4) it removes the allowance for arrowheads and the line shadow, and for text boxes and pictures (types 2, 6) the border widths and shadow edges. Rectangles (type 1) are written as they are.

#### `0x0084` Fill

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 10 | |
| 2 | u16 | ✅ constant 0 | ignored on read |
| 4 | RBG4 | fill colour | ✅ default `ff ff ff 01`; `RedColor` = `ff 00 00 00` |
| 8 | u16 | ✅ fill colour `HH` | `0x00FF` / 0; ignored on read |

#### `0x0097` Anchor (program only)

✅ Anchors a drawing shape to a box, so the shape moves with the box when the chart is laid out again. It follows the shape's other records and is written for any shape type whose box link is set. The help file lists "anchored objects" among the Org Plus features that Microsoft Organization Chart drops when it opens an Org Plus chart.

✅ The anchor command works on the selected shapes, skipping connectors (internal type 7), and anchors each one to the **current box**. A matching command removes the anchor.

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | cb = 22 |
| 2 | u16 | ✅ the box's group id (class `0x65`) |
| 4 | u16 | ✅ the box's number, as in `0x0050` BoxIndex |
| 6 | i32 | ✅ x offset of the shape's centre from the box's centre (view units) |
| 10 | i32 | ✅ y offset of the shape's centre from the box's centre |
| 14 | i32 | ✅ the box's width when it was anchored: its rectangle minus its left and right inner margins |
| 18 | i32 | ✅ the box's height when it was anchored, measured the same way |

#### Pictures (program only)

✅ Pictures can appear as picture shapes, on box text fields and on field definitions; the program exports `_PGShowPic`. A picture is a program object of class `0x6A`/`0x6C` (stored as a bitmap with a palette) or another class (stored as plain data chunks). Its data is split into chunks of up to 64 KB, followed by a size record.

**`0x0081` BitmapHeader**

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | picture id |
| 2 | u16 | length *L* of the data at offset `0x41C` |
| 4 | u16 | ✅ number of `0x0082` records that follow. The reader reads exactly this many. |
| 6 | u16 | ✅ 0 |
| 8 | u16 | ✅ height in pixels (`biHeight`) |
| 10 | u16 | ✅ width in pixels (`biWidth`) |
| 12 | u16 | ✅ rows per chunk: 20000 / row size, so each chunk holds about 20 000 bytes of whole rows |
| 14 | u16 | ✅ colour count: 2, 16 or 256 for 1-, 4- and 8-bit images, 0 for 24-bit |
| 16 | u32 | ✅ total size of the pixel data (`bfSize − bfOffBits`) |
| 20 | `LOGPALETTE` | ✅ a Windows logical palette: `palVersion` = `0x0300` (u16), the number of entries (u16), then the entries from offset 24. Each entry is red, green, blue, 0, converted from the bitmap's own blue-green-red colour table. The area is always 1032 bytes long and ends at `0x41C`; entries past the colour count are leftover data. |
| `0x41C` | u8[*L*] | ✅ the start of the bitmap file after its 14-byte file header: *L* = `bfOffBits` bytes, which is the 40-byte `BITMAPINFOHEADER`, the colour table and the first 14 bytes of pixel data |

**`0x0082` BitmapData**: a chunk of whole pixel rows, in file order.

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | ✅ rows still to come after this chunk |
| 2 | u16 | ✅ rows in this chunk |
| 4 | u16 | ✅ width in pixels |
| 6 | u32 | ✅ bytes of pixel data in this chunk |
| 10 | u16 | length *L* of the data |
| 12 | u8[*L*] | pixel rows |

The pixel data of all the chunks, in order, is the bitmap file's pixel data from `bfOffBits` to the end.

**`0x0085` PictureData**: a chunk of a picture that isn't stored as a bitmap.

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | picture id + 1 (ignored on read) |
| 2 | u16 | ✅ length of the data at offset 16 |
| 4 | u16 | ✅ in the first chunk, the number of chunks (including this one); otherwise 0 |
| 6 | u16 | ✅ picture kind: 1 for picture class `0x6D`, 0 for class `0x6E`. The reader rejects other values. |
| 8 | i16[4] | ✅ a rectangle: left, top, right, bottom. For kind 1 the picture's size is its width and height. For kind 0 the size is instead taken from the `u32`s at offsets 4 and 8 of the first chunk's data. |
| 16 | u8[] | data |

**`0x0080` PictureSize** (40 bytes), written after the data:

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | cb = 40 |
| 2 | u32[2] | ✅ displayed (width, height), view units |
| 10 | u32[2] | (width, height) again; ignored on read |
| 18 | u32[2] | (0, 0): 🟡 a crop offset; ignored on read |
| 26 | u32[2] | (width, height) again; ignored on read |
| 34 | u16 | ✅ **scale to fit area**: 1 when the picture is scaled to the size at offset 2, 0 when it is shown at full size. For a picture shape it is always 1. |
| 36 | u16 | ✅ **allow distortion**: when 0, scaling keeps the picture's proportions. For a picture shape it is the shape's flag `0x04`. |
| 38 | u16 | ✅ alignment: 0 left, 1 centre, 2 right (as text-style property 7). For a picture shape, 0. |

✅ The settings are those of the "Picture Sizing" dialog: Full size, Scale to fit area (with a height and width) and Allow distortion. 🟡 The repeated sizes are an original size and a current size. The reader only uses the first pair. In box text, `0x0080` is only accepted on a Picture field with no text; elsewhere it is "File has a bad data record format."

✅ A picture shape's `0x0083` flag (offset 20) is 1 for a type-8 picture of class `0x6D` and 0 otherwise.

✅ The order is fixed. When reading, `0x0081` must be followed by `0x0082`, and `0x0085` must follow its predecessor, otherwise "File has a bad bitmap data format."

✅ **Where pictures come from.** The program imports Windows bitmaps (`.BMP`, checked for the `BM` signature) and Aldus placeable metafiles (`.WMF`, checked for `0x9AC6CDD7`):
- **Bitmaps** become `0x0081`/`0x0082` pictures. The loader reads the `BITMAPINFOHEADER`, rejects images wider or taller than 32767 pixels, and accepts 1-, 4-, 8- and 24-bit images. The data is the bitmap's pixel rows; each row is `ceil(width × bits / 32) × 4` bytes, as in a Windows device-independent bitmap.
- **Metafiles** are stored as `0x0085` chunks.

#### `0x0088` Selected

✅ Marks an item as selected:

- In `0x400C`, its value is the zero-based number of the selected line segment, and the reader uses it.
- In `0x400E` and `0x4012`, its value is the item's zero-based position in the program's drawing list, which connectors and shapes share. For example, `Rectangle.opx` has `02 00` after its third shape. The reader ignores the value and selects the shape or connector most recently read.
- In `0x4007`, it follows the box's `0x0051`. Its value is not set: it is leftover data, which is always `0x24`, the cb of the `0x0051` written just before it (§6). The reader ignores the value and selects the box most recently read.

#### `0x0087`, `0x009A` (program only)

✅ Optional box records:
- **`0x0087`** marks a box that **shows a picture** instead of its text template. A box's template is a program object identified by (class, id). Class `0x67` is a label setup (`0x0023`), and the exported `_PGShowPic` switches the box to class `0x68`, a picture. The reader sets class `0x68` when it sees this record. The 2-byte payload is leftover data.
- **`0x009A`** (`{4, 1}`) marks a box that is **excluded from calculations**. It is written when box flag `0x200` is set, and the reader sets the flag from the `u16` at offset 2.
  - ✅ The flag backs the program's box property `0x6D`, which reads as 1 ("included") when the flag is clear. The setter sets the flag when the property is 0.
  - ✅ The only place that sets the property from the UI is the OK handler of the `IDD_EXCLAMOUNTS` ("exclude amounts") dialog. That dialog also sets property `0x6C` for each field of the box: to 1 for every field in its first list and to 0 for every field in its second. Property `0x6C` is flag `0x02` of the box's own text field, which is saved in that field's `0x0099` (§4.3).
  - ✅ The formula accumulator (`0x502CA8`, §4.3) skips any box whose property `0x6D` is 0, or whose box flag `0x04` (a hidden position) is set, so excluded and hidden boxes don't count towards `@sum`, `@avg` and the other functions.

---

### 4.9 What the reader does with each record

✅ Ignored wherever they appear: `0x0013`, `0x0070`, `0x0071`, `0x0099`, `0x0052`, `0x0D0A`, the canvas-level `0x0023`, and `0x1000`–`0x13E7`. Ignored fields are marked in each table. Each section reader handles only these ids:

| Section | Ids handled |
|---|---|
| header | `0x0001`, `0x0002`, `0x0003` |
| `0x4005` | `0x4008`, `0x0FFC`, `0x0FFD` |
| `0x4008` (definitions) | `0x0011`, `0x0012`, `0x0060`, `0x0080` |
| `0x4003` | `0x0020`–`0x0022`, `0x0024`, `0x0053`, `0x0054`, `0x0056`, `0x0089`–`0x0093`, `0x0095`, `0x0096`, `0x0098`, `0x009B`, and sections `0x4006`, `0x4009`, `0x400A`, `0x400B`, `0x400E`, `0x4010`, `0x4012` |
| `0x4006` | `0x0041`, `0x0042`, `0x0050`, `0x0051`, `0x0053`, `0x0054`, `0x0056`, `0x0087`, `0x0088`, `0x0092`, `0x0093`, `0x009A`, `0x4008`, `0x400C` |
| `0x4008` (box text) | `0x0011`, `0x0060`, `0x0080`, `0x0081`, `0x0085`, `0x0FFC`, `0x4010` |
| `0x400C` | `0x0056`, `0x0088`, `0x0094` |
| `0x4012` | `0x0055`, `0x0056`, `0x0088`, `0x0093`, `0x0094` |
| `0x400E` | `0x0053`, `0x0056`, `0x0060`, `0x0080`–`0x0085`, `0x0088`, `0x0092`–`0x0094`, `0x0097`, `0x4010` |

---

## 5. Enumerations

### 5.1 Border style (`0x0053` offset 2)

✅ The program calculates the value from a pen code, where 1500 means no pen and 1501 a hairline:

| Pen | Value |
|---|---|
| none | 0 |
| hairline | `0x0A` |
| default (width 2) | 1 |
| width *w* > 2 | `10 + w / 2` |

✅ **What the values mean.** A border is up to two lines: an outer line, a gap and an inner line, each with a width in view units (half points). The pen code is `1500 + outer width + 10 × inner width`, and a double border's gap is always 2. The reader turns each stored value back into the three widths:

| Value | Outer | Gap | Inner | |
|---|---|---|---|---|
| `0x00` | – | – | – | no border |
| `0x01` | 2 | – | – | 1 pt single (the default) |
| `0x0A` | 1 | – | – | hairline |
| `0x0C`, `0x0D`, `0x0E` | 4, 6, 8 | – | – | 2, 3, 4 pt single |
| `0x16`, `0x17`, `0x18` | 4, 6, 8 | 2 | 2 | double, thick line outside |
| `0x1F`, `0x29`, `0x33` | 2 | 2 | 4, 6, 8 | double, thick line inside |

✅ Read only: the reader also accepts 2 (the same as 1), 3 (1 pt single with a dashed line style) and 4 or 5 (the same as `0x16`). Any other value leaves the border at the 1 pt default. The program's own limit is 4 points: "The maximum border thickness for a box is 4 points."

✅ The Boxes > Border Style menu's command ids are the pen codes themselves, so the menu maps directly onto the stored values:

| Menu item | Pen code | Value | Menu item | Pen code | Value |
|---|---|---|---|---|---|
| 1 | 1500 | `0x00` (none) | 7 | 1501 | `0x0A` (hairline) |
| 2 | 1502 | `0x01` | 8 | 1504 | `0x0C` |
| 3 | 1506 | `0x0D` | 9 | 1508 | `0x0E` |
| 4 | 1542 | `0x1F` | 10 | 1524 | `0x16` |
| 5 | 1562 | `0x29` | 11 | 1526 | `0x17` |
| 6 | 1582 | `0x33` | 12 | 1528 | `0x18` |

The samples:

| Value | Sample | Value | Sample |
|---|---|---|---|
| `0x00` | NoBorder | `0x16` | DoubleBorder2 |
| `0x01` | SingleBorder1 (default) | `0x17` | DoubleBorder4 |
| `0x0A` | HairlineBorder | `0x18` | DoubleBorder6 |
| `0x0C` | SingleBorder2 | `0x1F` | DoubleBorder1 |
| `0x0D` | SingleBorder3 | `0x29` | DoubleBorder3 |
| `0x0E` | SingleBorder4 | `0x33` | DoubleBorder5 |

### 5.2 Line style (`0x0053` offset 8, `0x0056` offset 2)

| Value | Meaning |
|---|---|
| 0 | No line |
| 1 | Solid |
| 3 | Dotted |
| 4 | Dashed |

✅ The names come from the program's Style menus (Solid, Dotted, Dashed). The program never writes 2. The samples `Lines/Style/Dashed.opx` and `DashedWide.opx` are really dotted and dashed.

✅ **How the patterns are drawn.** The line painter (`0x42C754`) takes a unit equal to the pen width, kept between 2 and 6 device units. A dotted line alternates one unit on and one unit off; a dashed line alternates three units on and one unit off. The pattern restarts on each straight segment.

### 5.3 Shadow

✅ **Border offset 6, shadow edges**, is a sum:

| Part | Value |
|---|---|
| base | 1 when the shadow type is 2–4, 2 otherwise |
| left edge | +10 |
| right edge | +20 |
| top edge | +40 |
| bottom edge | +80 |

It is 0 when there is no shadow. The writer puts 1 in the base for shadow types 2–4 and 2 for every other type.

✅ **Reading.** The reader treats the value as one of a fixed list of presets and sets the box's shadow from it. The `0x0092` that follows then replaces that shadow, so the preset only matters in a file with no `0x0092`. Values not in the list (such as the 3D style's 152) leave the box with no shadow until `0x0092` is read.

| Preset | Type | Offsets (v, h) | Edges |
|---|---|---|---|
| 0 | 0 | 0, 0 | none |
| 1, 91 | 2 | 4, −4 | left, bottom |
| 2, 92 | 1 | 4, −4 | left, bottom |
| 41 | 3 | −4, 4 | top |
| 81 | 3 | 4, 4 | bottom |
| 101 | 2 | 4, 4 | right, bottom |
| 102 | 1 | 4, 4 | right, bottom |
| 111 | 4 | 4, 4 | left, right, bottom |

✅ **Line and text shadows use different type codes.** `0x0093` offset 2 and text-style property `0x0E` are written as 0, 1, 2, 7, 8 for the internal types 0–4. When reading `0x0093`, 1 and 2 are themselves, 7 and 8 become internal types 3 and 4, and 3–6 and anything above 8 become type 1.

✅ **Shadow types** (`0x0092` offset 2), from the two painters (`0x42F614` for types 1–4 and 9, the border painter `0x43006B` for types 5–8). "Shape" means the box's outline, including a rounded or cut-corner shape; (h, v) are the record's offsets; n is the larger of |h| and |v|.

| Type | Drawn as | Edges it occupies |
|---|---|---|
| 0 | nothing | none |
| 1 | **Drop shadow**: one filled copy of the shape in the shadow colour, moved by (h, v) | the side h points to and the side v points to |
| 2 | **Block**: n outlines of the shape in the shadow colour, each moved one step further towards (h, v), so the box looks extruded | as type 1 |
| 3 | **Tapered**: n outlines, each moved one step further vertically and narrowed by the same step on the left and right | top or bottom only, by the sign of v |
| 4 | **Flared**: n outlines, each moved one step further vertically and widened by the same step on the left and right | left, right, and top or bottom |
| 5 | **Raised**: n outlines stepped towards (+h, +v) in the shadow colour and n stepped towards (−h, −v) in the secondary colour, then the border | all four |
| 6 | **Sunken**: as 5 with the two colours swapped | all four |
| 7 | **Raised frame**: as 5, but each outline is enlarged on one side rather than moved, so the steps form a bevelled frame around the box: right and bottom in the shadow colour, left and top in the secondary colour | all four |
| 8 | **Sunken frame**: as 7 with the two colours swapped | all four |
| 9 | **Outline**: one copy of the border moved by (h, v), its outer line in the shadow colour and its inner line (for a double border) in the secondary colour | as type 1 |

The secondary colour (`0x0092` offset 12) is the highlight of types 5–9; types 1–4 don't use it. The outlines of types 2–8 are drawn with the border's own widths and line style. The "edges" column is the space the program reserves around the box, and is what border offset 6 summarises.

The Boxes > Shadow menu offers 8 presets (command ids `0x641`–`0x648`). In the samples:

| Type (`0x0092` offset 2) | Offsets (v, h) | Border offset 6 | Edges | Sample |
|---|---|---|---|---|
| 0 | 0, 0 | 0 | – | None |
| 1 | 4, 4 | 102 = 2 + 20 + 80 | right, bottom | Right + bottom |
| 1 | 4, −4 | 92 = 2 + 10 + 80 | left, bottom | Left + bottom |
| 2 | 4, 4 | 101 = 1 + 20 + 80 | right, bottom | Right + bottom, bevelled |
| 2 | 4, −4 | 91 = 1 + 10 + 80 | left, bottom | Left + bottom, bevelled |
| 3 | 4, 4 | 81 = 1 + 80 | bottom | Bottom only |
| 3 | −4, 4 | 41 = 1 + 40 | top | Top only |
| 4 | 4, 4 | 111 = 1 + 10 + 20 + 80 | left, right, bottom | Left + bottom + right |
| 7 | 2, 2 | 152 = 2 + 10 + 20 + 40 + 80 | all | 3D (3D Gold style) |

### 5.4 Line thickness (`0x0056` offset 4)

✅ The pen width in half points. The Lines > Thickness menu's command ids are `0x6A4` + the stored value:

| Menu | Stored |
|---|---|
| None | 0 |
| Hairline | 1 (sample `NoThickness.opx`) |
| 1 pt | 2 (default) |
| 2 pt | 4 |
| 3 pt | 6 |
| 4 pt | 8 |
| 5 pt | 10 |
| 6 pt | 12 |

### 5.5 Text-style properties (`0x0061`)

✅ From the program's text-style writer:

| Id | Kind | Meaning | Value | Canvas default |
|---|---|---|---|---|
| `0x01` | run | font style | bit 0 bold, bit 1 italic. ✅ Bit 2 is a style this edition lacks (🟡 underline): it is cleared in charts from another edition, with "Change the style of some of your text". | 0 |
| `0x02` | run | font | index into the font table (`0x0025` offset 0). The UI's "Narrow" and "Black" are separate fonts ("Arial Narrow", "Arial Black"). An index with no font is read as the first font that exists. | 0 |
| `0x03` | run | text colour | RGB4 | black |
| `0x04` | – | ✅ **date code** (read only in practice) | When the low byte of the value is `%`, the reader appends today's date to the text, formatted by the next byte: `1` = `%B %d, %Y`, `2` = `%d %B %Y`, `4` = `%d/%m/%y`, `5` = `%m-%d-%y`, anything else `%d-%m-%y` (C `strftime` formats). The writer only emits this property with the value `0x29`, which the reader ignores, for a hidden box in a group of file layout style 6. | – |
| `0x05` | paragraph | ✅ **show the field's label** (text flag `0x1000`, which also appears in `0x0071` offset 2). Charts from another edition get it cleared: "Remove field labels". | 0 or 1 | 0 |
| `0x06` | paragraph | ✅ **wrap text** (text flag `0x02`). Charts from another edition get it cleared: "Unwrap text in fields". | 0 or 1. ✅ Set through `_PGTextAtt` when its request mask has bit `0x40`. | 1 (0 on boxes and shapes) |
| `0x07` | paragraph | alignment | 0 left, 1 centre, 2 right. ✅ The reader takes 1 as centre, 2 as right and every other value as left. | **6**: the canvas default stores the raw Windows `SetTextAlign` flags, where 6 is centre and 2 right. Because 0–2 never equals 6, every box, title part and text box carries its own property 7 (all samples), and the default's value never takes effect. |
| `0x08` | paragraph | a **name format**, 1–28 (§5.9) | ✅ Set through the exported `_PGTextAtt` (text attributes); text flag `0x4000` means "this value is set". A field with a name format is a **Name** field: FieldKind (`0x0012`) is 1 when it is set and 2 when it isn't. A box's text takes its format from the field definition when that has flag `0x4000`. | 0 |
| `0x09` | run | font size | points | 14 |
| `0x0A` | run | text shadow colour | RGB4 | 0 (3D Gold: `40 40 40`) |
| `0x0B` | run | text shadow second colour | RGB4 | white |
| `0x0C` | run | ✅ text shadow vertical offset | signed, view units | 0 (3D Gold: 1) |
| `0x0D` | run | ✅ text shadow horizontal offset | signed, view units | 0 (3D Gold: 1) |
| `0x0E` | run | ✅ text shadow type | line-shadow codes: 0, 1, 2, 7, 8 for internal types 0–4 (§5.3) | 0 (3D Gold: 7 or 8) |
| `0x14`+ | – | special text, such as an inserted date | id = internal kind − 30000 + 20. ✅ The reader accepts ids `0x14`–`0x16` and stores the value with the kind; ids `0x0F`–`0x13` and anything above `0x16` are ignored. | – |

✅ Properties `0x0A`–`0x0E` come from one 20-byte structure and are written together whenever it changes: the same shape as a box shadow (type, two colours, two offsets). The default section is written in the order `06 08 07 05 02 09 0E 0A 0B 0C 0D 01 03`.

### 5.6 Layout style (`0x0FFD` offsets 14 and 18, `0x0042` offsets 12 and 14)

✅ The program converts between its internal layout codes and a fixed set of file codes. The file values it writes are 0–9, 12, 15, 16, 18, 19, 22–26, 28, 29 and 32–34; every other internal code is written as 0, and every other file value is read as style 0.

| File | Internal | File | Internal | File | Internal |
|---|---|---|---|---|---|
| 0 | 0 | 8 | 8 | 23 | 23 |
| 1 | 2 | 9 | 4 | 24 | 24 |
| 2 | 6 | 12 | 12 | 25 | 25 |
| 3 | 5 | 15 | 15 | 26 | 26 |
| 4 | 3 | 16 | 16 | 28 | 33 |
| 5 | 13 | 18 | 18 | 29 | 29 |
| 6 | 14 | 19 | 19 | 32 | 32 |
| 7 | 9 | 22 | 22 | 33 | 28 |
| | | | | 34 | 34 |

✅ Internal codes 23–33 are assistant layouts: the program keeps the default for those separately (`0x0FFD` offset 18). 34 is the co-manager layout.

✅ **Microsoft's edition only offers eight of these**: file codes 0, 2, 3, 4, 5 and 6 (the six group styles on the Styles menu), 25 (assistants) and 34 (co-managers). The others belong to the full Org Plus. A chart from another edition has them replaced when it is opened (§6.1):

| File code in the chart | Becomes |
|---|---|
| 1 | 0 |
| 8 | 4 |
| 15 | 3 |
| 12, 16, 22 | 2 |
| 18 | 5 |
| 7, 9, 19 | 6 |
| 23, 24, 26, 28, 29, 32, 33 | 25 |

✅ **What the styles look like.** The program has two layout routines, named by its exports: `CHBHorizHandler` lays a group out as a **row** (`0x421E72`) and `CHBColumnHandler` as **columns** (`0x41D011`). The column routine places the group's boxes in rows of one, two or four beside vertical connecting lines.

| File code | Internal | Routine | Arrangement |
|---|---|---|---|
| 0 | 0 | row | the boxes side by side under a horizontal connecting line |
| 34 | 34 | row | co-managers: a row, with the groups below hung from a silent position |
| 2 | 6 | column | one box per row, in a single column to the **right** of a vertical line; a short horizontal line joins the middle of each box's left edge to it |
| 5 | 13 | column | as 2, but each box is joined at the middle of one row of its text (🟡 the row marked by template opcode `04`, normally the first) instead of the middle of the box, and the vertical gap is its own value (`0x0054` offset 16) |
| 3 | 5 | column | two boxes per row, one on each side of a central vertical line |
| 4 | 3 | column | four boxes per row around **two** vertical lines: box, line, two boxes, line, box. Each line has a box on either side, as in style 3. A group of one box is laid out as style 3. |
| 6 | 14 | column | one box per row, stacked directly under the parent box with no gap beside a line: the boxes are offset by the parent's border and margins, and the group has a single line segment |
| 25 | 25 | column | assistants: laid out as style 3, two per row on either side of the line down from the manager |

✅ **Left and right.** The column routine keeps, for each box, the space it needs on its left and on its right, and places the vertical line in the gap between column groups. A single-column style puts the gap (`0x0054` offset 14, plus half the line's thickness) on the box's left when a per-style flag is set and on its right otherwise. The flag is set for internal styles 4, 6, 12 and 13, so file styles 2 and 5 hang their boxes to the right of the line; the Org Plus-only single-column styles without the flag (such as internal 16) are their mirror images, with the boxes to the left.

The other assistant styles are laid out as an ordinary style too: internal 23 as 16, 28 as 19, 29 as 6 and 33 as 14. Internal styles 1, 2, 24, 26 and 32 use the row routine; the row routine itself sets a group below to 24 or 32 depending on which side of its parent it falls.

In the samples:

| Value | Sample |
|---|---|
| 0 | Groups1 (default horizontal) |
| 2 | Groups2 |
| 3 | Groups3 |
| 4 | Groups4 |
| 5 | Groups5 |
| 6 | Groups6 |
| 25 | assistant group (`0x0042`), and `0x0FFD` offset 18 in `.opx` files |
| 29 | `0x0FFD` offset 18 in the `.ops` sample |
| 34 (`0x22`) | CoManager |

### 5.7 Shape type (`0x0083` offset 2)

| Value | Meaning | |
|---|---|---|
| 1 | Rectangle | ✅ |
| 2 | Text box | ✅ |
| 3 | Horizontal line | ✅ |
| 4 | Vertical line | ✅ |
| 5 | Free (diagonal) line | ✅ `0x0083` offset 20 says which diagonal of the rectangle it is |
| 6 | ✅ Picture (image), written with border, shadow and picture records. It shares its editing code with type 8. | program only |
| 7 | Aux line (connector). Stored with the drawing objects but written to `0x4012` as `0x0055`, not here. | ✅ |
| 8 | 🟡 Picture | program only |

### 5.8 Box type (`0x0051` offset 2)

✅ The program picks the first that applies:

| Value | Meaning |
|---|---|
| 4 | ✅ **silent position** (box flag `0x80`; no text, template `0xFFFF`). Allowed as the last box of an assistant or co-manager group, or anywhere in a group with flag `0x20`; otherwise "Illegal silent position found in file." |
| 3 | ✅ read only: a hidden box (the same as type 0 with `0x0051` offset 6 set) |
| 2 | assistant |
| 1 | co-manager (the group's style is `0x22`) |
| 0 | normal |

### 5.9 Name formats (text-style property `0x08`)

✅ When a name is typed into a field with a name format, the box editor (`0x444E40`) rewrites it. The steps below come from the editor code; the program has no names for the formats, so the ones given here describe what each produces.

**Splitting the name.** The text before the first comma is split into words at spaces. Words between `\` characters count as one word, so `Mary\Ann` stays together.
- A trailing word made only of `I` and `V` (II, III, IV) is set aside as a suffix.
- The **last name** is the final word, together with any particle in front of it. The single-word particles are D', Da, Dal, De, Del, Dela, Delos, Des, Den, Di, Du, La, Le, Los, O', Mc, Mac, St, St., San, Santa, Saint, Van and Von. Some of these (Del, Mac, Santa, Van) only count when the name has three or more words. The two-word particles are "de la", "de los" and "van"/"von" followed by "de", "den" or "der". Letters are compared ignoring case.
- The **first name** is the first word before the last name, and the **middle** is any words between them.
- The **suffix** is everything from the comma on, including the comma, or the Roman-numeral word.
- A name already typed as "Last, First Middle" (one word before the comma) is read that way round.

**Rebuilding it.** Each format lists the parts in its order and appends them after the typed text; the typed text is then deleted. A space goes between parts, but not before one that starts with a comma, such as a suffix with a comma. A format can also:
- put a comma after one part
- reduce a part to its first letter followed by `.`
- reduce a part to an initial only if it starts with a capital letter, dropping it if it doesn't, so lower-case particles in a middle name disappear

Formats 8–14 and 22–28 are 1–7 and 15–21 with the **last name in capitals**.

| Format | Capitals | Parts | "John Quincy Public, Jr." becomes |
|---|---|---|---|
| 1 | 8 | First Middle Last Suffix | John Quincy Public, Jr. |
| 2 | 9 | First, middle initial, Last, Suffix | John Q. Public, Jr. |
| 3 | 10 | First Last Suffix | John Public, Jr. |
| 4 | 11 | first initial, middle initial, Last, Suffix | J. Q. Public, Jr. |
| 5 | 12 | first initial, Last, Suffix | J. Public, Jr. |
| 6 | 13 | unchanged (13 only capitalises the last name) | John Quincy Public, Jr. |
| 7 | 14 | Last | Public |
| 15 | 22 | Last, First Middle Suffix | Public, John Quincy, Jr. |
| 16 | 23 | Last, First, middle initial, Suffix | Public, John Q., Jr. |
| 17 | 24 | Last, First Suffix | Public, John, Jr. |
| 18 | 25 | Last, first initial, middle initial, Suffix | Public, J. Q., Jr. |
| 19 | 26 | Last, first initial, Suffix | Public, J., Jr. |
| 20 | 27 | Last Suffix | Public, Jr. |
| 21 | 28 | Last (the same code as 7) | Public |

🟡 The example outputs were worked out by following the code, not by running the program.

### 5.10 Box shape (`0x0053` offset 4)

✅ From the fill and outline painters (`0x43170A`, `0x42ECD2`). Let *m* be the shorter of the box's width and height.

| Value | Shape |
|---|---|
| 0 (and any other value) | rectangle |
| 1 | **rounded rectangle**, drawn with the Windows `RoundRect` function with a corner ellipse 2 × 0.25 × *m* wide and high, so each corner has a radius of a quarter of the shorter side |
| 2 | **octagon**: a rectangle with each corner cut off diagonally, 0.2 × *m* along each edge |

The corner size is rounded up to an even number, and a factor above 0.4 is treated as 0.4. Shadows and double borders follow the same shape.

---

## 6. Quirks and hazards

- **Leftover data.** The program builds many records in one shared scratch buffer and doesn't clear it, so any field it doesn't set keeps old bytes:
  - `0x0001` holds the banner text when a banner was written.
  - `0x0FFD` bytes 26–29 hold "r Bl" in the `.ops` sample.
  - A box's `0x0088` holds `0x24`.
  - `PurpleBackground.opx` has garbage font names.

  Don't treat non-zero reserved bytes as errors.
- **Written only when different** (§3.4). Records missing from an object mean "same as the default", not "none".
- **Records written back unchanged** (§3.5). A file can contain records written by other versions of the software.
- **Different builds.** The sample `.opx` files differ from what the 1995 program writes in the banner and `0x0FFD` offset 0. The 1995 program ignores both when reading.
- **Modified files.** A file edited after it was saved keeps its old checksums, and the original program refuses to open it (§2.3).
- **Strict checks.** The original program refuses a file for:
  - a bad checksum
  - a password
  - a version above 1001
  - a missing or zero VIEWUNITS
  - a character set other than 0 or 1
  - a layout direction other than top to bottom
  - picture records out of order
  - a missing required part, or a group, box, connector or shape record out of place (§3)
  - a silent position where one isn't allowed (§5.8)

  A reader can be more lenient, but a writer that wants its files to open in Org Chart must meet all of these.
- **Writing a file Org Chart will open unchanged.** Write 4 at `0x0002` offset 2, or the program converts the chart (§6.1). Use only the layout styles, border styles and features of Microsoft's edition, end every assistant group with a silent position, and don't write one for a co-manager group.
- **Records the program writes but never reads** (§4.9), and fields marked "ignored on read". Their contents can't be relied on to round-trip through the program.
- **Slack bytes** between the root section and the footer (§2.4).
- **Two colour byte orders** (§1.1).

### 6.1 Charts from other Org Plus editions

✅ When `0x0002` offset 2 isn't 4, the open routine (`0x49A311`) converts the chart to what Microsoft's edition supports and lists the changes in a message box. Each line names a feature of the format:

| Message | What is changed |
|---|---|
| Change some of the chart styles | group styles outside this edition's set (§5.6) |
| Change the spacing between boxes | `0x0020` offset 20 and the `0x0054` spacing values |
| Change the size of some boxes | box width and height (`0x0054` offsets 6 and 8); `0x0051` offsets 10 and 12 |
| Exclude hidden boxes / hidden branches | `0x0051` offsets 6 and 8 |
| Remove field labels | text-style property 5 |
| Unwrap text in fields; Remove all text following a <return> | text-style property 6 |
| Exclude name-formatting of your fields | text-style property 8 |
| Exclude pictures in boxes | picture and amount field definitions |
| Change your amount fields to text fields | label type 3 (done for every file) |
| Exclude some of the fields in your chart | only four fields are kept |
| Change the arrangement of fields in boxes; Remove lines inside boxes | box templates: every box gets template 0 |
| Exclude some of your titles and footers | title parts other than 1 |
| Change the style of some of your text | font style bit 2 (🟡 underline) is cleared |

It also clears `0x0022` offset 8, sets offset 10, zeroes the guide insets and sets the default styles to 0 and 25. Group styles outside this edition's eight are replaced as listed in §5.6.

## 7. Open questions

- A few values the program stores but never uses: `0x0042` offset 2 (when offset 16 isn't `0xFFFF`), `0x0051` offset 24, the spacing value in `0x0020` offset 20 (every instruction that touches it only sets, copies, saves or loads it) and `0x0FFD` offset 6 (only exposed through the program's property interface). 🟡 They are probably used by the full Org Plus.
- The appearance of the Org Plus-only layout styles (file codes 1, 7–9, 12, 15–19, 22–24, 26, 28, 29, 32, 33) beyond what §5.6 says. Microsoft's edition replaces them on open.
- The print time codes other than `#` that Org Plus may store in `0x0FFC`. This program ignores them.
- What a preserved record (`0x1000`) holds.
- How a password is stored in `0x0001`. ✅ This can't be recovered from this program: its only password code is the single check that refuses the file.
- The labels of the two lists in the exclude-amounts dialog. The `IDD_EXCLAMOUNTS` dialog template doesn't survive in this build. The help file (decompressed from `ORGCHART.HLP`) describes Microsoft's cut-down edition and doesn't cover these Org Plus features.
- **The older Org Plus formats.** The program also opens "Org Plus 3.1, 4.0, 5.0, 6.0" files through its `IOP*` routines and delimited text through its `IA*` routines. Those are different formats and aren't covered here. ✅ All that was checked is how an old file is recognised: it is delimited text whose first field is `ORG` and whose second is a version number from 300 to 499; older versions get "MS Organization Chart can only open files from Org Plus versions 3.0 and above." The program refuses Org Plus Advanced drawing board files (`.DWG`).

## 8. Methodology

Findings came from:

1. **Parsing every sample** into a tree and tabulating each record id's length, parent section and distinct values.
2. **Diffing samples** that differ by one UI change. For example, `Shadow/*.opx` against `Layout/OneBox.opx` isolated the shadow type and offsets.
3. **Treating the footer checksum as a black box.** Pairs of files that differ by one byte showed the high word changing by `rol16(old ^ new, distance from the footer)`, the signature of a rotate-and-XOR checksum.
4. **Disassembling the program.** `msoc2.exe` is an installer containing a CAB archive. Its `OFFICE1.CAB` holds `ORGCHART.EXE` and `TEMPLATE.MSO`. The program exports 127 function names, including `OEMSaveChart` and `OEMOpenChart`. The save code is unoptimised and every record goes through one writer (`WriteRecord` at `0x46EA6A`), so listing its 104 call sites gives every record the program can write, and following each call shows where each field comes from. The checksum routine (`0x4C0880`) matches the algorithm found in step 3.
5. **Disassembling the open path.** `OEMOpenChart` leads to:
   - the header check (`0x48C2BD`)
   - the checksum check (`0x4974E2`)
   - a record-header reader (`0x494089`) shared by 21 reader functions

   Listing the ids each reader compares against, and the error message each check raises (string resources `0x5DD`–`0x5EF`), gave the validation rules, the extended length, VIEWUNITS, the character set and the original terminology.
6. **Reading the open path record by record.** Each section reader was followed to its dispatch on the record id, which gives the list of ids it handles (§4.9), and each record decoder was read for the offsets it uses, which gives the fields that are ignored on read. The conversion routine for charts from other editions (`0x49A311`) builds its message from string resources `0x2CEC`–`0x2CFC`; pairing each string with the condition that selects it named several fields (hidden boxes and branches, field labels, wrapping, box size, spacing, lines inside boxes, titles and footers). The two layout-style tables (`0x46FEB2` for writing, `0x496F65` for reading) and the border-style and shadow-preset tables in the border reader (`0x496232`) were dumped from the program's jump tables.
7. **Tracing the layout code.** The two layout routines (`0x41D011`, `0x421E72`) were searched for every use of the group and box fields that `0x0054`, `0x0042` and `0x0051` are read into. How each field enters the position arithmetic (added to a vertical drop, to the running position along a row, or to the offset between stacked boxes) named the box size and the spacing values.
8. **Reading the painters.** The box painter (`DrawPosition`), the shadow painter, the border painter and the patterned-line routine were read for how each shadow type, box shape, line style and in-box line is drawn.
9. **Following individual fields.** For each value still unnamed, every instruction that reads the field it is stored in was listed and the surrounding code read: the pagination routine (`0x4D23C0`) for the two centring flags, the box painter for the branch markers, the bitmap importer (`0x461A24`) and the Picture Sizing dialog handler for the picture records, the connector handle routine (`0x455322`) for the segment offsets, and the save set-up routine (`0x46EE16`) for where the canvas defaults come from.
10. **The layout handlers.** The table that maps each internal style to a layout routine is filled at start-up with two exported functions, `CHBHorizHandler` and `CHBColumnHandler`; which one each style gets, the boxes-per-row routine (`0x4383F0`) and the per-style switch at the top of the column routine gave the arrangement of each style. The chart frame came from the routine that builds its rectangle from the page margins (`0x4AAED6`).
11. **Tracing the interface code.** The menu and dialog resources name each command. Following the code that sets each document field (for example the Styles menu handler, and the code that calls `GetScrollPos`) identified the GLOBALINFO fields, the border-style and thickness menus, and the line-style names. Following the reader for `0x0041` identified the group id and assistant count.
