# OPX / OPS File Format Specification

Unofficial specification of the binary file format used by the Microsoft Organization Chart add-in (`.opx` charts and `.ops` style files). The format was licensed from Banner Blue Software's **Org Plus** product (hence "UOCF" and the name of this project).

Nothing here comes from official documentation. It was worked out from two sources:

- **The samples**: 109 files in [`Samples`](Samples) and [`Tests`](Tests), plus `TEMPLATE.MSO`, the template new charts start from.
- **The program**: the save and open code in `ORGCHART.EXE` (internal name `MSOCht32.EXE`, built 17 October 1995), extracted from [`Samples/msoc2.exe`](Samples/msoc2.exe). See §8.

The sample `.opx` files were saved by a **different build** of Org Chart from that executable. That build writes no banner (§4.2) and puts 8 rather than 4 at `0x0FFD` offset 0. Neither difference matters to the 1995 program when it opens a file. Everything else in the samples matches what the 1995 code writes.

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
| silent position | 🟡 box type 4 |
| free object | a drawing shape |
| bitmap data | the picture records |
| aux line | a user-drawn connector (`0x0055`) |
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

The RBG4 flag byte is 1 when `HH = 0xFF` (box fill) and 0 otherwise. Border, shadow and line records always write 0 there. Most RBG4 colours are followed somewhere in the record by a `u16` holding `HH`. That's why a default fill reads `ff ff ff 01 … ff 00`, and a shape border's "9" is its `HH`.

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
    0x0098  Flag
    0x0022  PageSetup
   [0x0089  DevMode]
    0x009B
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
    0x0095  Second default Border     │
    0x0096  Second default Shadow     ┘
    0x4006  Groups section
     (0x0041 Group, 0x0042 GroupLayout, [0x0093 LineShadow], 0x4007 Boxes section, 0x400C Lines section)*
    0x4012  Connectors section
     (0x0055 Connector, 0x0056 Line, [0x0093 LineShadow], [0x0094 Arrow]*, [0x0088 Selected])*
    0x400E  Drawing shapes section
     (0x0083 Shape, shape-type-specific records, [0x0088 Selected])*
    [preserved records]*                 e.g. 0x1000 (§3.5)
  end 0x6003, 0x6002, 0x6001
[slack bytes]
 0x0FFF  Footer
```

✅ The program never writes sections `0x4004`, `0x400D`, `0x400F` or `0x4011`.

### 3.1 Org chart model

✅ The chart is a tree of **groups**. A group is a set of sibling boxes that share one parent box.

- Each group (`0x0041`) records its id, its parent group, its parent box, its box count and how many of those boxes are assistants.
- Groups are written in pre-order: a parent group always comes before its children.
- Boxes are numbered `1, 2, 3 …` across the whole chart, in file order (`0x0050`).
- `0x0020` PageInfo records the total box count.

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
[0x009A]                box flag 0x200 (program only)
0x0053 Border
0x0092 Shadow
[0x0054 BoxLayout]      only when it differs from the default (§3.4)
[0x4008 Box text section]    absent for box type 4
  (0x0011 FieldIndex, [pictures], [field extras], 0x0060 Text, [0x4010 TextStyle section])*
```

✅ After the boxes, the group's lines section holds the line from the parent to each box. Each entry is only written when it differs from the default, which is why the section is empty in every sample:

```
([0x0056 Line], [0x0094 Arrow], [0x0094 Arrow], [0x0088 Selected])*
```

The line's `ItemIndex` (offset 6) says which box in the group it belongs to.

### 3.3 Drawing shapes section (`0x400E`)

Each shape starts with `0x0083` Shape. What follows depends on the shape type (§5.7):

| Type | Following records |
|---|---|
| 1 Rectangle | `0x0053` Border, `0x0092` Shadow, `0x0084` Fill |
| 2 Text box | `0x0053` Border, `0x0092` Shadow, `0x0084` Fill, `0x0060` Text, `0x4010` TextStyle section |
| 3, 4, 5 Lines | `[0x0056 Line]`, `[0x0093 LineShadow]` (each only when it differs from the default), then an `0x0094` Arrow for each arrowhead |
| 6, 8 Pictures (program only) | `0x0053` Border, `0x0092` Shadow, then picture records |
| 7 (program only) | `0x0097` ShapeBoxLink |

✅ If the shape is selected, a `0x0088` record follows it.

### 3.4 Defaults and differences

✅ The canvas section writes default values, and the program keeps a copy of them. Some per-object records are written **only when they differ** from that copy:

| Record | Compared against |
|---|---|
| `0x0054` BoxLayout | canvas `0x0054` |
| `0x0056` Line (group lines, drawn lines) | canvas `0x0056` |
| `0x0093` LineShadow | canvas `0x0093` |
| Text-style properties | canvas `0x4010`, and the previous run (§4.6) |

Readers should start from the canvas defaults and apply per-object records over them. For example, `Lines/Line.opx` has no `0x0056` for its line, so the line uses the default thickness of 2.

### 3.5 Preserved records

✅ After the drawing shapes, the program writes back, unchanged, records it stored as objects of class `0x6F`. 🟡 These are records it read but did not understand: `0x1000` in the 3D Gold style file is one of these, written by some other version of the software.

✅ Not every unknown record is kept. The reader for field definitions, for example, silently skips every id except `0x0011`, `0x0012`, `0x0060` and `0x0080`.

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
| `0x0013` | Field extra | var | `4008` | program only; a string |
| `0x0020` | PageInfo | 34 | `4003` | C#: `BodyInfo` |
| `0x0021` | ViewUnits | 8 | `4003` | |
| `0x0022` | PageSetup | ≥45 | `4003` | |
| `0x0023` | BoxTemplate | var | `400A`, `4003` | |
| `0x0024` | ❓ | 2 | `4003` | |
| `0x0025` | FontName | var | `400B` | |
| `0x0026` | TitlePartIndex | 2 | `4009` | |
| `0x0041` | Group | 17 | `4006` | |
| `0x0042` | GroupLayout | 18 | `4006` | |
| `0x0050` | BoxIndex | 2 | `4007` | |
| `0x0051` | Box | 36 | `4007` | |
| `0x0052` | ❓ | 2 | `4003` | |
| `0x0053` | Border | 16 | `4003`, `4007`, `400E` | |
| `0x0054` | BoxLayout | 20 | `4003`, `4007` | |
| `0x0055` | Connector | 26 | `4012` | |
| `0x0056` | Line | 14 | `4003`, `400C`, `4012`, `400E` | |
| `0x0060` | Text | var | many | |
| `0x0061` | TextStyle | 8 | `4010` | |
| `0x0070` | Field extra | 8 | `4008` | program only |
| `0x0071` | Field extra | var | `4008` | program only |
| `0x0080` | Picture placement | 40 | `4008`, `400E` | program only |
| `0x0081`, `0x0082`, `0x0085` | Picture data | var | `4008`, `400E` | program only |
| `0x0083` | Shape | 22 | `400E` | |
| `0x0084` | Fill | 10 | `400E` | C#: `ShapeColor` |
| `0x0087` | ❓ | 2 | `4007` | program only |
| `0x0088` | Selected | 2 | `4007`, `400C`, `4012`, `400E` | |
| `0x0089` | DevMode | var | `4003` | |
| `0x0090` | ❓ | var | `4003` | program only |
| `0x0091` | ❓ | 6 | `4003` | program only |
| `0x0092` | Shadow | 16 | `4003`, `4007`, `400E` | |
| `0x0093` | LineShadow | 16 | `4003`, `4006`, `4012`, `400E` | |
| `0x0094` | Arrow | 14 | `400C`, `4012`, `400E` | program only |
| `0x0095` | Second default Border | 16 | `4003` | |
| `0x0096` | Second default Shadow | 16 | `4003` | |
| `0x0097` | ShapeBoxLink | 22 | `400E` | program only |
| `0x0098` | Flag | 4 | `4003` | |
| `0x0099` | Field extra | 4 | `4008` | program only |
| `0x009A` | ❓ | 4 | `4007` | program only |
| `0x009B` | ❓ | 10 | `4003` | |
| `0x009C` | FontInfo | 4 | `400B` | |
| `0x0D0A` | Banner | var | top | |
| `0x0FFC` | ❓ | var | `4005`, boxes | read only: the program accepts it but never writes it (§4.3) |
| `0x0FFD` | GlobalInfo | 34 | `4005` | |
| `0x0FFF` | Footer | 10 | end of file | §2.3 |
| `0x1000` | Preserved record | 1864 | `4003` | §3.5 |

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
- `0x0002`'s version is not checked.

| Record | Offset | Type | Field | Value |
|---|---|---|---|---|
| `0x0003` | 0 | u16 | cb | 8 |
| | 2 | u16 | version | 1001 |
| | 4 | u8 | day | 26 |
| | 5 | u8 | month | 12 |
| | 6 | u16 | year | 92 |
| `0x0002` | 0 | u16 | cb | 10 |
| | 2 | u16 | document kind | 4 (`.opx` samples). The 1995 program always writes 4, but the `.ops` sample has 2 (style). When reading, any value other than 4 sets a flag that changes how the chart is loaded. |
| | 4 | u16 | version | 1010 |
| | 6 | u8 | day | 26 |
| | 7 | u8 | month | 12 |
| | 8 | u16 | year | 92 |

### 4.3 Global info

#### `0x0010` FieldCount

✅ `u16`: the number of text fields defined in the following `0x4008` section. This is 4 (Name, Title, Comment 1, Comment 2) in `.opx` samples and 2 (Name, Title) in the `.ops` sample.

#### `0x4008` Field definitions (global)

Each field is a `0x0011` FieldIndex (`u16`, zero-based), a `0x0012` FieldKind (`u16`, ❓ 1 or 2) and a `0x0060` Text holding the field's label ("Name", "Title", "Comment 1", "Comment 2"). Optional picture records and field extras may follow.

❓ FieldKind is not understood. Several `.opx` files have kind 2 for "Name" and others kind 1.

✅ **Field extras** (program only) are written for both field definitions and box text, each depending on a field flag. In order:

| Id | Length | Written when |
|---|---|---|
| `0x0071` | var | field flag `0x1000` |
| `0x0013` | var (a string) | field flag `0x20` |
| `0x0099` | 4 (cb = 4) | |
| `0x0070` | 8 | field flag `0x01` |

#### `0x0FFD` GlobalInfo (34 bytes)

✅ Built field by field from the document:

| Offset | Type | Field | Samples |
|---|---|---|---|
| 0 | u16 | ❓ record version? | 4 (1995 program, `.ops`, `TEMPLATE.MSO`); 8 (`.opx` samples) |
| 2 | u16 | boolean: document flag `0x02000000` is **clear** | |
| 4 | u16 | boolean: document flag `0x04000000` is **clear** | |
| 6 | u16 | boolean: document flag `0x00100000` | |
| 8 | u16 | boolean: document flag at `+0x2A` bit 0 | 1 |
| 10 | u16 | boolean: document flag at `+0x2A` bit 1 | 1 |
| 12 | i16 | 🟡 magnification | The view's zoom; the program writes it negated when a view mode flag is set, probably "Size to Window". 0 or −564 in the samples. |
| 14 | u16 | ✅ default group style | §5.6 |
| 16 | u16 | ✅ a parameter stored with the default group style | 0 |
| 18 | u16 | ✅ default assistant style | Same codes as offset 14. Choosing an assistant style (internal 23–33) on the Styles menu stores it here instead of at offset 14. 25 (`.opx`), 29 (`.ops`). |
| 20 | i16 | 🟡 selection anchor | One half of a point the program records when a box selection starts; −1 when there is none. −1, 0, 1 in the samples. |
| 22 | i16 | 🟡 selection anchor | The other half of that point. −1, 0, 2 in the samples. |
| 24 | u16 | 🟡 selection kind | 1 exactly when a box is selected (18 of 18 samples) |
| 26 | u8[4] | leftover data | ✅ never written (§6); "r Bl" in the `.ops` sample |
| 30 | u16 | ✅ horizontal scroll position | from `GetScrollPos` on the chart window; 0, 133 (`.ops`), 219 (`TEMPLATE.MSO`) |
| 32 | u16 | ✅ vertical scroll position | 0 |

✅ When reading GLOBALINFO, the program:
- **ignores offset 0**, so the 4-versus-8 difference between builds has no effect;
- reads offset 20 into both of the fields that offsets 20 and 22 come from, and ignores offset 22;
- ignores offsets 24–33, so the selection isn't restored;
- reads offsets 12, 30 and 32 only when it isn't importing.

GLOBALINFO must come before the first view, otherwise "The GLOBALINFO section of this file does not occur before the first view."

✅ **`0x0FFC`** is accepted in the global info section, and in the box reader, but never written. In the global info section the program only notes that it was present and ignores its contents. ❓ It may be an older form of GLOBALINFO.

### 4.4 Canvas records

#### `0x0020` PageInfo

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 34 | |
| 2 | u16 | ✅ box count | 1, 2, 5, 6 match the number of `0x0050` records |
| 4 | u16 | ❓ | 36 |
| 6 | u16 | ❓ | 0 (`.ops` and `TEMPLATE.MSO`: leftover data) |
| 8 | u16 | ❓ | `0x2C2E` |
| 10 | u16 | ❓ | 1 |
| 12 | u16 | ❓ | 2 |
| 14 | u16 | ✅ character set | 1 = ANSI (all samples), 0 = OEM code page. Other values: "MS Organization Chart does not support the character set used by this file." |
| 16 | RBG4 | ✅ background colour | |
| 20 | u16 | ❓ | 22 |
| 22 | u16 | ❓ | 0 |
| 24 | u32 | ❓ | 0 (`.ops`: 144) |
| 28 | u32 | ❓ | 0 (`.ops`: 144) |
| 32 | u16 | 🟡 colour `HH` | `0x00FF` with the default white, otherwise 0 |

#### `0x0021` ViewUnits

✅ Called VIEWUNITS by the program. It sets the unit for every coordinate and distance in the file (§1.2).

| Offset | Type | Field | Value |
|---|---|---|---|
| 0 | u16 | cb | 8 |
| 2 | u16 | ❓ | 0 |
| 4 | u16 | base units per inch | 144 |
| 6 | u16 | percentage scale | 100 |

Units per inch = offset 4 × offset 6 / 100. The program always writes `{8, 0, 144, 100}`. When reading, it rejects a file where the result is 0 ("No valid VIEWUNITS found").

#### `0x0098` Flag, `0x0024`, `0x0052`, `0x009B`

| Id | Content | |
|---|---|---|
| `0x0098` | `{4, v}`: v = 1 unless document flag `0x08000000` is set | ✅ |
| `0x0024` | `u16` from a document field | ✅ 0 in every sample |
| `0x0052` | `u16` from a program-wide setting | ✅ 0 in every sample |
| `0x009B` | `0a 00 00 00 00 00 01 00 01 00` | ❓ constant in every sample |
| `0x0090` | program only; data from a stored object | ❓ |
| `0x0091` | program only; 6 bytes from two program-wide values, only when a third is set | ❓ |

#### `0x0022` PageSetup

✅ Page and layout setup. Fields marked "measurement" are in view units (§1.2) and converted when read.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb of the fixed part = 44 | ✅ |
| 2 | u16 | layout direction | ✅ must be 0, top to bottom. Other values: "MS Organization Chart can only import charts that are formatted from top to bottom." |
| 4 | u16 | a two-way setting | ✅ 0 or non-zero; ❓ meaning |
| 6 | u16[2] | ❓ | 0 |
| 10 | i16 | ❓ a setting | 1 |
| 12 | u32[4] | 🟡 page margins (measurements) | 0, 72, or 37/37/27/27 (`TEMPLATE.MSO`) |
| 28 | u32 | measurement | 0, 7, 14 |
| 32 | u16 | ❓ a setting | 1 |
| 34 | i16 | ❓ a setting | 1 |
| 36 | u32 | measurement | 611–639 |
| 40 | u32 | measurement | 41–139 |
| 44 | string | ✅ printer as `device,driver,port` | e.g. `HP LaserJet 4/4M Plus PS 600,adobeps,LPT2:`. Empty (a single NUL) in most samples. When reading, the program compares it with the current Windows printer. |

#### `0x0089` DevMode

✅ A Windows `DEVMODE` structure for the printer named in `0x0022`: `dmDeviceName[32]`, `dmSpecVersion` (`0x030A` or `0x0400`), `dmDriverVersion`, `dmSize`, `dmDriverExtra`, `dmFields`, `dmOrientation`, `dmPaperSize`, `dmPaperLength`, `dmPaperWidth`, `dmScale`, `dmCopies`, `dmDefaultSource`, `dmPrintQuality` and so on, followed by driver-private bytes. It only appears when a printer has been configured.

#### `0x0023` BoxTemplate

✅ A box template lists which text fields a box shows and in what order. Boxes refer to it by id (`0x0051` offset 32).

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | ✅ template id | A bitmask of the fields shown: `0x01` = Name only, `0x0F` = all four. Id 0 lists every field (the master template). |
| 2 | u16[4] | 🟡 inner margins | 8, 8, 8, 8 (`.opx`), 4, 4, 4, 4 (`.ops`) |
| 10 | u8 | ❓ | 4 |
| 11 | entry[] | ✅ field list | 3 bytes each: `01`, field index, `06` (❓) |
| … | u8 | terminator | `00` |

The `0x400A` section holds every template in use. The canvas-level `0x0023` is the template given to new boxes and is a copy of one in `0x400A`. Every `0x0051` template id appears in `0x400A` (all samples).

#### `0x400B` Font table

Each font is an optional `0x009C` FontInfo followed by a `0x0025` FontName. Text styles refer to fonts by index (§5.5, property `0x02`).

| Record | Offset | Type | Field |
|---|---|---|---|
| `0x009C` | 0 | u16 | ✅ font index |
| | 2 | u16 | ❓ 0 normally; `0xA1` for Century Gothic. Possibly charset or pitch-and-family. |
| `0x0025` | 0 | u16 | ✅ font index |
| | 2 | string | ✅ face name, e.g. "Arial", "Arial Narrow", "Arial Black", "Times New Roman" |

⚠️ In `PurpleBackground.opx` the face names are binary junk (`a0 8a 85 59 6a 01`), so readers must not assume they are printable.

#### `0x4009` Chart title

✅ There are always six parts (indexes 0–5) laid out as two rows of three. The default alignments are left, centre, right, left, centre, right (text-style property `0x07`). Part 1, top centre, holds the main title ("Organization Chart Title"). Unused parts have empty text.

Each part is a `0x0026` TitlePartIndex (`u16`), a `0x0060` Text and an optional `0x4010` style section.

### 4.5 Groups and boxes

#### `0x0041` Group

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 17 | ✅ |
| 2 | u16 | group id | ✅ The group's id in the program's object table (class `0x65`). Connectors refer to groups by this id. Ids are assigned when groups are created, so in a new chart they match file order. |
| 4 | u16 | assistant-part group id | ✅ The program writes the group id here too. When reading, it uses this id for the group's assistant part. |
| 6 | i16 | parent group index | ✅ −1 for the root group |
| 8 | u16 | parent box index | ✅ one-based; 0 for the root group |
| 10 | u16 | box count | ✅ the number of `0x0050` records in the following `0x4007` |
| 12 | u16 | assistant count | ✅ how many of the group's boxes are assistants. The reader splits the group into ordinary boxes (offset 10 minus offset 12) and assistants. |
| 14 | u8 | group flag `0x20` | ✅ |
| 15 | u8 | group flag `0x40` | ✅ |
| 16 | u8 | co-manager | ✅ 1 when the group uses the co-manager layout |

#### `0x0042` GroupLayout

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 18 | ✅ |
| 2 | u32 | ❓ a group value (another is used when the field at offset 16 is −1) | 0 |
| 6 | u32 | ❓ a group value | 0 |
| 10 | u16 | ❓ a group value | `0x08`, `0x0D`, `0x11` |
| 12 | u16 | ✅ layout style | §5.6; 0, 3, 25 in the samples |
| 14 | u16 | ✅ layout style again | the program writes the same field twice |
| 16 | u16 | ❓ a group value | 0 |

#### `0x0050` BoxIndex

✅ `u16`: the one-based number of the box within the whole chart.

#### `0x0051` Box

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 36 | ✅ |
| 2 | u16 | box type | ✅ §5.8 |
| 4 | u16 | 0 | ✅ |
| 6 | u16 | box flag `0x04` | ✅ |
| 8 | u16 | box flag `0x10` | ✅ |
| 10 | u16 | box flag `0x01` | ✅ 1 on boxes with text in the samples |
| 12 | u16 | box flag `0x02` | ✅ 1 on boxes with text in the samples |
| 14 | u16[3] | 0 | ✅ |
| 20 | u32 | ❓ a box value | 0 |
| 24 | u32 | ❓ a box value | 0 |
| 28 | RBG4 | fill colour | ✅ flag byte 1 when `HH = 0xFF`, and then the program's default fill colour is written |
| 32 | u16 | box template id | ✅ refers to `0x0023`; `0xFFFF` for type-4 boxes |
| 34 | u16 | fill colour `HH` | ✅ |

#### `0x0054` BoxLayout

✅ Only written for a box when it differs from the canvas default (§3.4). Its fields come from the group's spacing settings (the program exports a `_PGSetSpace` function):

| Offset | Type | Source | Samples |
|---|---|---|---|
| 0 | u16 | cb = 20 | |
| 2 | u16 | 0 | |
| 4 | u16 | 0 | |
| 6 | u16 | a box value | 180 (`.ops`: 90) |
| 8 | u16 | a box value | 36 or 180 (`.ops`: 18) |
| 10 | u16 | a group value | 22 or 0 |
| 12 | u16 | a group value | 22 |
| 14 | u16 | a group value | 22 |
| 16 | u16 | one of two group values, chosen by the group's layout style | 14 |
| 18 | u16 | ❓ | 22 |

### 4.6 Styling records

#### `0x0053` Border

`0x0095` has the same layout.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 16 | ✅ |
| 2 | u16 | border style | ✅ §5.1 |
| 4 | u16 | ❓ a value from the box or shape | 0 in every sample |
| 6 | u16 | shadow edges | ✅ §5.3 |
| 8 | u16 | line style | ✅ §5.2 |
| 10 | RBG4 | border colour | ✅ flag byte always 0 |
| 14 | u16 | border colour `HH` | ✅ 0, or 9 on drawing shapes |

#### `0x0092` Shadow

`0x0093` LineShadow and `0x0096` have the same layout.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 16 | ✅ |
| 2 | u16 | shadow type | ✅ §5.3 |
| 4 | i16 | vertical offset | ✅ + down, − up (Top = −4) |
| 6 | i16 | horizontal offset | ✅ + right, − left (LeftBottom = −4) |
| 8 | RBG4 | shadow colour | ✅ flag byte always 0 |
| 12 | RBG4 | secondary colour | ✅ white in the samples; 🟡 a bevel highlight |

✅ `0x0093` is a **line shadow**. The program writes it alongside a `0x0056` line for the canvas default, groups, connectors and drawn lines. `0x0095` and `0x0096` are the border and shadow of a second default object, which the program builds from separate document fields. 🟡 It may be the default for drawing shapes.

#### `0x0056` Line

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 14 | ✅ |
| 2 | u16 | line style | ✅ §5.2 |
| 4 | u16 | thickness (pen width) | ✅ §5.4 |
| 6 | u16 | item index | ✅ In a lines section, the position of the box in its group; 0 elsewhere. |
| 8 | RBG4 | colour | ✅ flag byte always 0 |
| 12 | u16 | colour `HH` | ✅ 0 or 9 |

#### `0x0094` Arrow (program only)

✅ An arrowhead on a line, matching the program's exported `_PGSetArrow`. A line can have one at each end.

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | cb = 14 |
| 2 | u16[3] | 0 |
| 8 | u16 | end: 0 for one end (line flag `0x08`), 1 for the other (line flag `0x10`) |
| 10 | u16 | 0 |
| 12 | u16 | item index, as in `0x0056` |

#### `0x4010` / `0x0061` TextStyle

✅ Text is rich text: formatting can change part-way through. A text-style section is a list of 8-byte property records:

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | property id (§5.5) |
| 2 | u32 | value |
| 6 | u16 | **character position**: for per-run properties, the zero-based character where the run starts; 0 for paragraph properties |

The text is stored as a list of runs. For each run, the program writes per-run properties only when they change from the previous run. `Plus/Level2.opx` has a box whose font changes to font 1 at character 8.

The canvas section's `0x4010` lists all the defaults. Sections on boxes, title parts and shapes only list paragraph properties that differ from those defaults, plus each run's changes.

#### `0x0060` Text

✅ A string. Empty text is a single NUL. The samples never contain line breaks.

### 4.7 Connectors

#### `0x0055` Connector

✅ A user-drawn connector between two boxes (an "aux line" in the program's menus), followed by its `0x0056` Line.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 26 | ✅ |
| 2 | u16 | from: box index | ✅ |
| 4 | u16 | from: group id | ✅ `0x0041` offset 2 |
| 6 | u16 | from: anchor side | 🟡 3 or 4 |
| 8 | u16 | to: box index | ✅ |
| 10 | u16 | to: group id | ✅ `0x0041` offset 2 |
| 12 | u16 | to: anchor side | 🟡 3 or 4 |
| 14 | u32[3] | ❓ | 18, 18, 18. Possibly the lengths of the elbow segments. |

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

The fields are edges, not a position and size. For example, a horizontal line has `top = 486, bottom = 488`, and a vertical line has `left = 1011, right = 1013`. ✅ The program enlarges the edges to include the pen width and the shadow offsets before writing them.

#### `0x0084` Fill

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 10 | |
| 2 | u16 | ❓ | 0 |
| 4 | RBG4 | fill colour | ✅ default `ff ff ff 01`; `RedColor` = `ff 00 00 00` |
| 8 | u16 | 🟡 fill colour `HH` | `0x00FF` / 0 |

#### `0x0097` ShapeBoxLink (program only)

✅ Links a drawing shape to a box.

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | cb = 22 |
| 2 | u16 | 🟡 the box's group |
| 4 | u16 | box index |
| 6 | u32[4] | four shape values (🟡 offsets from the box) |

#### Pictures (program only)

✅ A picture is written as `0x0081`, `0x0082` and `0x0085` (variable-length data copied from memory blocks), then `0x0080`, a 40-byte record (cb = `0x28`) holding rectangles and flags. Pictures can appear on picture shapes, on box text fields and on field definitions; the program exports `_PGShowPic`.

✅ The order is fixed. When reading, `0x0081` must be followed by `0x0082`, and `0x0085` must follow its predecessor, otherwise "File has a bad bitmap data format." ❓ The data layout is not decoded.

#### `0x0088` Selected

✅ Marks an item as selected:

- In `0x400E`, `0x4012` and `0x400C`, it follows the item and its `u16` value is the item's zero-based index. For example, `Rectangle.opx` has `02 00` after its third shape.
- In `0x4007`, it follows the box's `0x0051`. Its value is not set: it is leftover data, which is always `0x24`, the cb of the `0x0051` written just before it (§6).

#### `0x0087`, `0x009A` (program only)

✅ Optional box records. `0x0087` (2 bytes of leftover data) is written when a box field equals `0x68`. `0x009A` (`{4, 1}`) is written when box flag `0x200` is set.

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

### 5.3 Shadow

✅ **Border offset 6, shadow edges**, is a sum:

| Part | Value |
|---|---|
| base | 1 when the shadow type is 2–4, 2 otherwise |
| left edge | +10 |
| right edge | +20 |
| top edge | +40 |
| bottom edge | +80 |

It is 0 when there is no shadow. The Boxes > Shadow menu offers 8 presets (command ids `0x641`–`0x648`). In the samples:

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
| `0x01` | run | font style | bit 0 bold, bit 1 italic | 0 |
| `0x02` | run | font | index into the font table. The UI's "Narrow" and "Black" are separate fonts ("Arial Narrow", "Arial Black"). | 0 |
| `0x03` | run | text colour | RGB4 | black |
| `0x04` | – | ❓ | always `0x29`; only written in one calling mode | – |
| `0x05` | paragraph | text flag `0x1000` | 0 or 1 | 0 |
| `0x06` | paragraph | text flag `0x02` | 0 or 1 | 1 (0 on boxes and shapes) |
| `0x07` | paragraph | alignment | 0 left, 1 centre, 2 right | **6**: the canvas default stores the raw Windows `SetTextAlign` flags, where 6 is centre and 2 right |
| `0x08` | paragraph | ❓ a text value, only when text flag `0x4000` is set | | 0 |
| `0x09` | run | font size | points | 14 |
| `0x0A` | run | text shadow colour | RGB4 | 0 (3D Gold: `40 40 40`) |
| `0x0B` | run | text shadow second colour | RGB4 | white |
| `0x0C` | run | text shadow offset | | 0 (3D Gold: 1) |
| `0x0D` | run | text shadow offset | | 0 (3D Gold: 1) |
| `0x0E` | run | text shadow type | | 0 (3D Gold: 7 or 8) |
| `0x14`+ | – | special text, such as an inserted date | id = internal kind − 30000 + 20 | – |

✅ Properties `0x0A`–`0x0E` come from one 20-byte structure and are written together whenever it changes: the same shape as a box shadow (type, two colours, two offsets). The default section is written in the order `06 08 07 05 02 09 0E 0A 0B 0C 0D 01 03`.

### 5.6 Layout style (`0x0FFD` offsets 14 and 18, `0x0042` offsets 12 and 14)

✅ The program converts its internal layout codes to a fixed set of file codes. The valid file values are 0–9, 12, 15, 16, 18, 19, 22–26, 28, 29 and 32–34. Internal codes 23–33 are assistant layouts: the program keeps the default for those separately (`0x0FFD` offset 18). In the samples:

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
| 5 | Free (diagonal) line | ✅ |
| 6 | 🟡 Picture (image) | program only |
| 7 | 🟡 Shape linked to a box (`0x0097`) | program only |
| 8 | 🟡 Picture | program only |

### 5.8 Box type (`0x0051` offset 2)

✅ The program picks the first that applies:

| Value | Meaning |
|---|---|
| 4 | box flag `0x80` is set (no text, template `0xFFFF`). 🟡 The program's "silent position": the reader checks these against the group's co-manager and assistant settings and can reject a file with "Illegal silent position found in file." |
| 2 | assistant |
| 1 | co-manager (the group's style is `0x22`) |
| 0 | normal |

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

  A reader can be more lenient, but a writer that wants its files to open in Org Chart must meet all of these.
- **Slack bytes** between the root section and the footer (§2.4).
- **Two colour byte orders** (§1.1).

## 7. Open questions

- What the document, group and box fields behind some values are, for example `0x0FFD` offsets 2–10, most of `0x0020` and `0x0022`, and `0x0042` offsets 2–10. Answering this means tracing the program's UI code that sets those fields.
- The layout of the picture records, the field extras and `0x0FFC`.
- How a password is stored in `0x0001` by versions that support passwords.
- What the second default object (`0x0095`, `0x0096`) applies to. 🟡 The chart window's paint routine reads it while drawing the background, so it is probably a frame drawn around the chart or its title.
- The connector's anchor sides and its trailing `u32` values.
- The FieldKind (`0x0012`) values.
- What the `0x0002` kind flag changes when a file is loaded.

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
6. **Tracing the interface code.** The menu and dialog resources name each command. Following the code that sets each document field (for example the Styles menu handler, and the code that calls `GetScrollPos`) identified the GLOBALINFO fields, the border-style and thickness menus, and the line-style names. Following the reader for `0x0041` identified the group id and assistant count.
