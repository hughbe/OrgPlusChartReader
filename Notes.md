# OPX / OPS File Format Specification

Unofficial specification of the binary file format used by the Microsoft Organization Chart add-in (`.opx` charts and `.ops` style files). The format was licensed from Banner Blue Software's **Org Plus** product (hence "UOCF" and the name of this project).

Everything here was worked out from the 109 files in [`Samples`](Samples) and [`Tests`](Tests). Nothing comes from official documentation.

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
- **cb**: many fixed-size records start with a `u16` that repeats the payload length. These are marked `cb` and must equal the record length in every sample.
- **String**: Windows-1252 (ANSI) text ending in a NUL byte. The record length includes the NUL. Every sample contains only ASCII.
- **Confidence**. Each field is marked:
  - ✅ **confirmed**: varies in controlled samples exactly as described, or holds in every sample
  - 🟡 **probable**: consistent with the samples but not isolated by a controlled sample
  - ❓ **unknown**: only observed values are given

### 1.1 Colours

Colours are 4 bytes. There are two encodings:

| Encoding | Used by | Byte 0 | Byte 1 | Byte 2 | Byte 3 |
|---|---|---|---|---|---|
| **RBG4** | box fill, border, shadow, line, shape fill, background | red | **blue** | **green** | flag |
| **RGB3** | text colour (text-style property `0x03`) | red | green | blue | – |

✅ RBG4: `YellowBorder.opx` and `TextBox/YellowColor.opx` store yellow as `ff 00 ff`, and `PurpleBackground.opx` stores purple as `80 80 00`. The "3D Gold" style stores its gold fill as `c0 00 c0`.

🟡 RGB3: `GreenTitle.opx` stores green text as `00 ff 00`. The 3D Gold style's default text colour is `c0 c0 00`, which is gold in RGB order.

The RBG4 flag byte is `0x01` when the colour is the application default (for example the default white box fill `ff ff ff 01`) and `0x00` when the user has picked a colour. 🟡 Records with a flag byte often have a trailing `u16` that is `0x00FF` alongside flag `0x01` and `0x0000` otherwise (see `0x0020` and `0x0084`).

### 1.2 Coordinates

🟡 Drawing coordinates are unsigned logical units measured from the top-left of the page, with y increasing downwards. The unit is unknown; it is probably screen pixels at 100% zoom.

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

✅ The file starts with the four ASCII bytes `UOCF` (`55 4F 43 46`).

### 2.2 Items

After the signature, the file is a sequence of items. Each item starts with a `u16` id:

| id range | Kind | Layout after the id |
|---|---|---|
| `0x0000`–`0x3FFF` | **record** | `u16 length`, then `length` payload bytes |
| `0x4000`–`0x5FFF` | **section start** | nothing; child items follow |
| `0x6000`–`0x7FFF` | **section end** | nothing; the id is always `start id + 0x2000` |

✅ Sections nest. The end marker always closes the innermost open section; there are no overlapping sections. The maximum record payload is 65535 bytes.

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

Both checksums cover every byte in `[0, F)`, including the signature and any slack bytes:

```c
uint16_t sum = 0, rotxor = 0;
for (size_t i = 0; i < F; i++) {
    sum    += data[i];                                 // modulo 2^16
    rotxor ^= data[i];
    rotxor  = (uint16_t)((rotxor << 1) | (rotxor >> 15)); // rotate left by 1
}
```

✅ Both checksums match in every sample. A file whose checksums don't match has usually been modified after it was saved; the application's behaviour when opening such a file is unknown, so readers should warn rather than refuse.

### 2.4 Slack bytes

✅ The root section is normally followed straight away by the footer. `Simple.opx` and `Box/Name1.opx` have extra bytes in between. They are leftovers from an earlier, longer save of the same file, and include a stale footer (and in `Simple.opx`, part of an older document). The application seems to overwrite the file in place without truncating it, then append a new footer at the end. Readers must ignore slack bytes but include them in the checksum.

---

## 3. Logical structure

The tree below is the complete structure seen in the samples, in file order. `[ ]` marks optional items and `*` marks items that repeat.

```
"UOCF"
[0x0D0A  Banner]                       .ops only
 0x0001  Reserved
 0x0003  Version A
 0x0002  Version B
 0x4001  Root section
  0x4002  Document section
   0x4005  Global info section
    0x0010  FieldCount
    0x4008  Field definitions section
     (0x0011 FieldIndex, 0x0012 FieldKind, 0x0060 Text)*        one per field
    0x0FFD  GlobalInfo
   0x4003  Canvas section
    0x0021  Unknown
    0x0020  PageInfo
    0x0098  Unknown
    0x0022  PrintSetup
   [0x0089  DevMode]
    0x009B  Unknown
    0x400A  Box templates section
     0x0023*  BoxTemplate
    0x400B  Font table section
     ([0x009C FontInfo], 0x0025 FontName)*
    0x4010  Default text style section
     0x0061*  TextStyle
    0x4009  Chart title section
     (0x0026 TitlePartIndex, 0x0060 Text, [0x4010 TextStyle section])*  six parts
    0x0024  Unknown
    0x0023  Default BoxTemplate for new boxes
    0x0052  Unknown
    0x0053  Default Border            ┐
    0x0092  Default Shadow            │ defaults applied to new boxes,
    0x0054  Default BoxLayout         │ lines and shapes
    0x0056  Default Line              │
    0x0093  Default (shadow-like)     │
    0x0095  Default (shadow-like)     │
    0x0096  Default (shadow-like)     ┘
    0x4006  Groups section
     (0x0041 Group, 0x0042 GroupLayout, 0x4007 Boxes section, 0x400C Lines section)*
    0x4012  Connectors section
     (0x0055 Connector, 0x0056 Line, [0x0088 Selected])*
    0x400E  Drawing shapes section
     (0x0083 Shape, shape-type-specific records, [0x0088 Selected])*
   [0x1000  Unknown]                   .ops only
  end 0x6003, 0x6002, 0x6001
[slack bytes]
 0x0FFF  Footer
```

### 3.1 Org chart model

🟡 The chart is a tree of **groups**. A group is a set of sibling boxes that share one parent box, and is what the Org Chart UI calls a "group style" unit.

- Each group (`0x0041`) records its own index, its depth in the tree, its parent group and its parent box.
- Groups are written in pre-order: a parent group always comes before its children.
- Boxes are numbered `1, 2, 3 …` across the whole chart, in file order (`0x0050`).
- `0x0020` PageInfo records the total box count.

Example: `Plus/Level2.opx` contains

```
Group 0  depth 0  parent box  0   boxes 1 "Name", 2 "Co Worker", 3 "Other Co-Worker"
Group 1  depth 1  parent box  1   box  4 "Subordinate"
Group 2  depth 2  parent box  4   boxes 5 "Assistant" (box type 2), 6 (box type 4, no text)
```

### 3.2 Boxes section (`0x4007`)

Each box is:

```
0x0050 BoxIndex
0x0051 Box
[0x0088 Selected]       present when the box is selected
0x0053 Border
0x0092 Shadow
[0x0054 BoxLayout]      present for boxes in assistant-style groups
[0x4008 Box text section]    absent for boxes without text (box type 4)
  (0x0011 FieldIndex, 0x0060 Text, [0x4010 TextStyle section])*
```

### 3.3 Drawing shapes section (`0x400E`)

Each shape starts with `0x0083` Shape. What follows depends on the shape type:

| Type | Following records |
|---|---|
| 1 Rectangle | `0x0053` Border, `0x0092` Shadow, `0x0084` Fill |
| 2 Text box | `0x0053` Border, `0x0092` Shadow, `0x0084` Fill, `0x0060` Text, `0x4010` TextStyle section |
| 3 Horizontal line, 4 Vertical line, 5 Free line | `[0x0056 Line]`. If it is absent, the line uses the defaults. |

✅ If the shape is selected, a `0x0088` record follows it.

### 3.4 Defaults and inheritance

🟡 Records in the canvas section (`0x0053`, `0x0092`, `0x0056`, `0x4010` and so on) hold the current defaults. Per-object records override them. For example, `Lines/Line.opx` and `Lines/Thickness1.opx` have no `0x0056` for their line, so the line uses the default thickness of 2. Per-object `0x4010` sections only list the properties that differ from the defaults.

---

## 4. Record reference

### 4.1 Index

| Id | Name | Length | Parent | Notes |
|---|---|---|---|---|
| `0x0001` | Reserved | 144 | top | cb + zeros |
| `0x0002` | VersionB | 10 | top | |
| `0x0003` | VersionA | 8 | top | |
| `0x0010` | FieldCount | 2 | `4005` | |
| `0x0011` | FieldIndex | 2 | `4008` | |
| `0x0012` | FieldKind | 2 | `4008` (global) | |
| `0x0020` | PageInfo | 34 | `4003` | C#: `BodyInfo` |
| `0x0021` | ❓ | 8 | `4003` | constant |
| `0x0022` | PrintSetup | ≥45 | `4003` | |
| `0x0023` | BoxTemplate | var | `400A`, `4003` | |
| `0x0024` | ❓ | 2 | `4003` | always 0 |
| `0x0025` | FontName | var | `400B` | |
| `0x0026` | TitlePartIndex | 2 | `4009` | |
| `0x0041` | Group | 17 | `4006` | |
| `0x0042` | GroupLayout | 18 | `4006` | |
| `0x0050` | BoxIndex | 2 | `4007` | |
| `0x0051` | Box | 36 | `4007` | |
| `0x0052` | ❓ | 2 | `4003` | always 0 |
| `0x0053` | Border | 16 | `4003`, `4007`, `400E` | |
| `0x0054` | BoxLayout | 20 | `4003`, `4007` | |
| `0x0055` | Connector | 26 | `4012` | |
| `0x0056` | Line | 14 | `4003`, `4012`, `400E` | |
| `0x0060` | Text | var | many | |
| `0x0061` | TextStyle | 8 | `4010` | |
| `0x0083` | Shape | 22 | `400E` | |
| `0x0084` | Fill | 10 | `400E` | C#: `ShapeColor` |
| `0x0088` | Selected | 2 | `4007`, `4012`, `400E` | |
| `0x0089` | DevMode | 96 | `4003` | 1 sample |
| `0x0092` | Shadow | 16 | `4003`, `4007`, `400E` | |
| `0x0093` | ❓ shadow-like | 16 | `4003` | |
| `0x0095` | ❓ shadow-like | 16 | `4003` | |
| `0x0096` | ❓ shadow-like | 16 | `4003` | |
| `0x0098` | ❓ | 4 | `4003` | |
| `0x009B` | ❓ | 10 | `4003` | constant |
| `0x009C` | FontInfo | 4 | `400B` | |
| `0x0D0A` | Banner | var | top | `.ops` only |
| `0x0FFD` | GlobalInfo | 34 | `4005` | |
| `0x0FFF` | Footer | 10 | end of file | §2.3 |
| `0x1000` | ❓ | 1864 | `4003` | `.ops` only |

| Section | Name | Section | Name |
|---|---|---|---|
| `0x4001` | Root | `0x4009` | Chart title |
| `0x4002` | Document (C#: "Container") | `0x400A` | Box templates |
| `0x4003` | Canvas | `0x400B` | Font table |
| `0x4005` | Global info | `0x400C` | Lines (always empty in the samples) |
| `0x4006` | Groups | `0x400E` | Drawing shapes |
| `0x4007` | Boxes | `0x4010` | Text style |
| `0x4008` | Text fields | `0x4012` | Connectors |

✅ Ids that do not appear in the samples: `0x4004`, `0x400D`, `0x400F`, `0x4011`. They are probably used by features the samples don't exercise.

### 4.2 Header records

#### `0x0D0A` Banner (`.ops` only)

✅ This is a copyright notice that doubles as a DOS `TYPE` trick. The id bytes are `0A 0D` (LF, CR). The length bytes are followed by two backspaces (`08 08`) that erase the length bytes on screen. Then comes the text, ending in `\r\n` and `0x1A` (Ctrl-Z, end of file in DOS) and a NUL. So `TYPE STYLE.OPS` prints "UOCF", a newline and a clean copyright message, then stops.

| Offset | Type | Value |
|---|---|---|
| 0 | u8[2] | `08 08` |
| 2 | string | "(c) Copyright 1992 Banner Blue Software Incorporated. … Banner Blue will aggressively pursue prosecution.\r\n\x1A" |

#### `0x0001` Reserved

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 144 | ✅ |
| 2 | u8[142] | reserved | ✅ zeros in every `.opx`. In the `.ops` sample it holds a copy of the banner text: uninitialised memory. |

#### `0x0003` VersionA / `0x0002` VersionB

| Record | Offset | Type | Field | Observed |
|---|---|---|---|---|
| `0x0003` | 0 | u16 | cb = 8 | |
| | 2 | u16 | ❓ version | 1001 (`0x03E9`) |
| | 4 | u32 | ❓ | `0x005C0C1A` |
| `0x0002` | 0 | u16 | cb = 10 | |
| | 2 | u16 | 🟡 document kind | **4** = chart (`.opx`), **2** = style (`.ops`) |
| | 4 | u16 | ❓ version | 1010 (`0x03F2`) |
| | 6 | u32 | ❓ | `0x005C0C1A` |

### 4.3 Global info

#### `0x0010` FieldCount

✅ `u16`: the number of text fields defined in the following `0x4008` section. This is 4 (Name, Title, Comment 1, Comment 2) in `.opx` samples and 2 (Name, Title) in the `.ops` sample.

#### `0x4008` Field definitions (global)

Each field is a `0x0011` FieldIndex (`u16`, zero-based), a `0x0012` FieldKind (`u16`, ❓ 1 or 2) and a `0x0060` Text holding the field's label ("Name", "Title", "Comment 1", "Comment 2").

❓ FieldKind is not understood. Several `.opx` files have kind 2 for "Name" and others kind 1.

#### `0x0FFD` GlobalInfo (34 bytes, no cb)

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | ❓ | 8 (`.opx`), 4 (`.ops`) |
| 2 | u16[3] | ❓ | 0 (`.ops`: 1, 1, 0) |
| 8 | u16 | ❓ | 1 |
| 10 | u16 | ❓ | 1 |
| 12 | i16 | ❓ | 0 or −564 (`cc fd`) |
| 14 | u16 | ✅ default group style | §5.6 |
| 16 | u16 | ❓ | 0 |
| 18 | u16 | ❓ | 25 (`.opx`), 29 (`.ops`) |
| 20 | i16 | ❓ | −1, 0, 1 |
| 22 | i16 | ❓ | −1, 0, 2 |
| 24 | u16 | 🟡 a box is selected | 1 exactly when a box section contains `0x0088` (18/18 samples) |
| 26 | u8[8] | reserved | 0. `.ops` has uninitialised memory here ("r Bl" from "Banner Blue"). |

### 4.4 Canvas records

#### `0x0020` PageInfo

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 34 | |
| 2 | u16 | ✅ box count | 1, 2, 5, 6 match the number of `0x0050` records |
| 4 | u16 | ❓ | 36 |
| 6 | u16 | ❓ | 0 (`.ops`: uninitialised) |
| 8 | u16 | ❓ | `0x2C2E` |
| 10 | u16 | ❓ | 1 |
| 12 | u16 | ❓ | 2 |
| 14 | u16 | ❓ | 1 |
| 16 | RBG4 | ✅ background colour | |
| 20 | u16 | ❓ | 22 |
| 22 | u16 | ❓ | 0 |
| 24 | u32 | ❓ | 0 (`.ops`: 144) |
| 28 | u32 | ❓ | 0 (`.ops`: 144) |
| 32 | u16 | 🟡 default-colour marker | `0x00FF` with the default white, otherwise 0 |

#### `0x0022` PrintSetup

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb of the fixed part = 44 | ✅ |
| 2 | u8[8] | ❓ | 0 |
| 10 | u16 | ❓ | 1 |
| 12 | u32[4] | 🟡 page margins | 0, or 72 each in `.ops` (72 = 1 inch in points?) |
| 28 | u32 | ❓ | 0 / 14 |
| 32 | u16 | ❓ | 1 |
| 34 | u16 | ❓ | 1 |
| 36 | u32 | ❓ | 637, 639, 637 |
| 40 | u32 | ❓ | 41, 139 |
| 44 | string | ✅ printer as `device,driver,port` | e.g. `HP LaserJet 4/4M Plus PS 600,adobeps,LPT2:`. Empty (a single NUL) in most samples. |

#### `0x0089` DevMode

✅ A Windows 3.1 `DEVMODE` structure (`dmSpecVersion = 0x030A`) for the printer named in `0x0022`. It is the same layout as Win16's `DEVMODE`: `dmDeviceName[32]`, `dmSpecVersion`, `dmDriverVersion`, `dmSize` (68), `dmDriverExtra`, `dmFields`, `dmOrientation`, `dmPaperSize`, `dmPaperLength` (2794 = 11 in), `dmPaperWidth` (2159 = 8.5 in), `dmScale` (100), `dmCopies`, `dmDefaultSource`, `dmPrintQuality` (600) and so on, followed by driver-private bytes. It only appears when a printer has been configured.

#### `0x0021`, `0x0098`, `0x009B`, `0x0024`, `0x0052`

❓ These are constant in every `.opx`:

| Id | Payload |
|---|---|
| `0x0021` | `08 00 00 00 90 00 64 00` (cb, 0, 144, 100) |
| `0x0098` | `04 00 01 00` (`.ops`: `04 00 00 00`) |
| `0x009B` | `0a 00 00 00 00 00 01 00 01 00` |
| `0x0024` | `00 00` |
| `0x0052` | `00 00` |

#### `0x0023` BoxTemplate

✅ A box template lists which text fields a box shows and in what order. Boxes refer to it by id (`0x0051` offset 32).

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | ✅ template id | A bitmask of the fields shown: `0x01` = Name only, `0x0F` = all four. Id 0 lists every field (the master template). |
| 2 | u16[4] | 🟡 inner margins | 8, 8, 8, 8 (`.opx`), 4, 4, 4, 4 (`.ops`) |
| 10 | u8 | ❓ | 4 |
| 11 | entry[] | ✅ field list | 3 bytes each: `01`, field index, `06` (❓) |
| … | u8 | terminator | `00` |

The `0x400A` section holds every template in use. The canvas-level `0x0023` is the template given to new boxes and is a copy of one in `0x400A`. Every `0x0051` template id appears in `0x400A` (all 109 samples).

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

### 4.5 Groups

#### `0x0041` Group

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 17 | ✅ |
| 2 | u16 | 🟡 group index | 0, 1, 2 … in file order |
| 4 | u16 | 🟡 depth | root = 0 |
| 6 | i16 | ✅ parent group index | −1 for the root group |
| 8 | u16 | ✅ parent box index | one-based; 0 for the root group |
| 10 | u16 | ✅ box count | the number of `0x0050` records in the following `0x4007` |
| 12 | u16 | 🟡 group style | 0 = chart default; 2 for an assistant group. See §5.6. |
| 14 | u16 | ❓ | 0 |
| 16 | u8 | ❓ | 0 |

#### `0x0042` GroupLayout

| Offset | Type | Field | Observed |
|---|---|---|---|
| 0 | u16 | cb = 18 | |
| 2 | u8[8] | ❓ | 0 |
| 10 | u16 | ❓ layout flags | 0, `0x08`, `0x0D`, `0x11` |
| 12 | u16 | ❓ | 0, 3, 25 |
| 14 | u16 | ❓ | equal to offset 12 |
| 16 | u16 | ❓ | 0 |

#### `0x0050` BoxIndex

✅ `u16`: the one-based number of the box within the whole chart.

#### `0x0051` Box

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 36 | ✅ |
| 2 | u16 | 🟡 box type | 0 normal, 2 assistant, 4 a box with no text or template (purpose unknown) |
| 4 | u8[6] | ❓ | 0 |
| 10 | u16 | ❓ | 1 (0 for type 4) |
| 12 | u16 | ❓ | 1 (0 for type 4) |
| 14 | u8[14] | ❓ | 0 |
| 28 | RBG4 | ✅ fill colour | |
| 32 | u16 | ✅ box template id | refers to `0x0023`; `0xFFFF` for type-4 boxes |
| 34 | u16 | ❓ | 0 |

#### `0x0054` BoxLayout

❓ Present in the canvas defaults and on boxes in assistant-style groups. All values look like distances:

| Offset | Type | Observed |
|---|---|---|
| 0 | u16 | cb = 20 |
| 2 | u16 | 0 |
| 4 | u16 | 0 |
| 6 | u16 | 180 (`.ops`: 90) |
| 8 | u16 | 36 or 180 (`.ops`: 18) |
| 10 | u16 | 22 or 0 |
| 12 | u16 | 22 |
| 14 | u16 | 22 |
| 16 | u16 | 14 |
| 18 | u16 | 22 |

The `.ops` values are exactly half the `.opx` ones. That points to a scaled unit.

### 4.6 Styling records

#### `0x0053` Border

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 16 | ✅ |
| 2 | u16 | ✅ border style | §5.1 |
| 4 | u16 | ❓ | 0 |
| 6 | u16 | ✅ shadow preset code | Changes only with the shadow. See §5.3. |
| 8 | u16 | ✅ line style | §5.2 |
| 10 | RBG4 | ✅ border colour | |
| 14 | u16 | ❓ | 0 or 9. Only seen on drawing shapes. Possibly uninitialised. |

#### `0x0092` Shadow

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 16 | ✅ |
| 2 | u16 | ✅ shadow type | §5.3 |
| 4 | i16 | ✅ vertical offset | + down, − up (Top = −4) |
| 6 | i16 | ✅ horizontal offset | + right, − left (LeftBottom = −4) |
| 8 | RBG4 | ✅ shadow colour | |
| 12 | RBG4 | 🟡 secondary colour | white; possibly a bevel highlight |

`0x0093`, `0x0095` and `0x0096` have the same 16-byte layout. ❓ Their purpose is unknown. `0x0093` in the 3D Gold style is type 7, offsets 1, 1, colour `40 40 40`.

#### `0x0056` Line

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 14 | ✅ |
| 2 | u16 | ✅ line style | §5.2 |
| 4 | u16 | ✅ thickness | §5.4 |
| 6 | u16 | ❓ | 0 |
| 8 | RBG4 | ✅ colour | |
| 12 | u16 | ❓ | 0 or 9 (same as Border offset 14) |

#### `0x4010` / `0x0061` TextStyle

A text-style section is a list of 8-byte property records:

| Offset | Type | Field |
|---|---|---|
| 0 | u16 | property id (§5.5) |
| 2 | u8[6] | value; the meaning depends on the property, and unused bytes are 0 |

The canvas-level `0x4010` lists all 13 properties (the defaults). Sections on boxes, title parts and shapes list only the properties that are overridden.

#### `0x0060` Text

✅ A string. Empty text is a single NUL. The samples never contain line breaks.

### 4.7 Connectors

#### `0x0055` Connector

✅ A user-drawn connector between two boxes, followed by its `0x0056` Line.

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 26 | ✅ |
| 2 | u16 | ✅ from: box index | |
| 4 | u16 | ✅ from: group index | |
| 6 | u16 | 🟡 from: anchor side | 3 or 4 |
| 8 | u16 | ✅ to: box index | |
| 10 | u16 | ✅ to: group index | |
| 12 | u16 | 🟡 to: anchor side | 3 or 4 |
| 14 | u32[3] | ❓ | 18, 18, 18. Possibly the lengths of the elbow segments. |

### 4.8 Drawing shapes

#### `0x0083` Shape

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 22 | ✅ |
| 2 | u16 | ✅ shape type | §5.7 |
| 4 | u32 | ✅ **top** | `ZeroY.opx` → 0 |
| 8 | u32 | ✅ **left** | `ZeroX.opx` → 0 |
| 12 | u32 | ✅ **right** | `Wider.opx` increases only this |
| 16 | u32 | ✅ **bottom** | `TextBox/Height1/2` change only this |
| 20 | u16 | 🟡 diagonal direction | 1 on free lines (which diagonal of the bounding box is drawn), 0 otherwise |

The fields are edges, not a position and size. For example, a horizontal line has `top = 486, bottom = 488`, and a vertical line has `left = 1011, right = 1013`.

#### `0x0084` Fill

| Offset | Type | Field | |
|---|---|---|---|
| 0 | u16 | cb = 10 | |
| 2 | u16 | ❓ | 0 |
| 4 | RBG4 | ✅ fill colour | Default is `ff ff ff 01`. `RedColor` = `ff 00 00 00`. |
| 8 | u16 | 🟡 default-colour marker | `0x00FF` / 0 |

#### `0x0088` Selected

✅ Marks the item it follows (in `0x400E` and `0x4012`) or is inside (in `0x4007`) as selected. In `0x400E` and `0x4012` the `u16` value is the zero-based index of the selected item. For example, `Rectangle.opx` has `02 00` after its third shape. In `0x4007` the value is always `0x24`. At most one item is selected in each sample.

### 4.9 Other

#### `0x1000` (`.ops` only)

❓ 1864 bytes: `de 50`, then 1860 bytes of `ff`, then `01 00`. Possibly a bitmap or table of unused slots.

---

## 5. Enumerations

### 5.1 Border style (`0x0053` offset 2)

✅ These match the order of the Org Chart "Border Style" menu:

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
| 1 | Solid |
| 3 | Dashed |
| 4 | Dashed (wide) |

✅ Value 2 has not been observed; it is probably dotted.

### 5.3 Shadow

| Type (`0x0092` offset 2) | Offsets (v, h) | Border offset 6 | Sample |
|---|---|---|---|
| 0 | 0, 0 | 0 | None |
| 1 | 4, 4 | `0x66` | Right + bottom |
| 1 | 4, −4 | `0x5C` | Left + bottom |
| 2 | 4, 4 | `0x65` | Right + bottom, bevelled |
| 2 | 4, −4 | `0x5B` | Left + bottom, bevelled |
| 3 | 4, 4 | `0x51` | Bottom only |
| 3 | −4, 4 | `0x29` | Top only |
| 4 | 4, 4 | `0x6F` | Left + bottom + right |
| 7 | 2, 2 | `0x98` | 3D (from the 3D Gold style) |

🟡 The border offset 6 value looks like a menu or preset code that identifies the shadow choice. Within a family, the bevelled variant is one less than the plain one.

### 5.4 Line thickness (`0x0056` offset 4)

✅ Line thickness values:

| UI choice | Stored |
|---|---|
| None / hairline | 1 |
| Thickness 1 | 2 (default) |
| Thickness 2 | 4 |
| Thickness 3 | 6 |
| Thickness 4 | 8 |
| Thickness 5 | 10 |
| Thickness 6 | 12 |

### 5.5 Text-style properties (`0x0061`)

| Id | Meaning | Value | Canvas default | |
|---|---|---|---|---|
| `0x01` | font style | u16 bit flags: bit 0 bold, bit 1 italic | 0 | ✅ |
| `0x02` | font | u16 index into the font table | 0 | ✅ The UI's "Narrow" and "Black" styles are separate fonts ("Arial Narrow", "Arial Black") added to the font table. |
| `0x03` | text colour | RGB3 | black | 🟡 |
| `0x05` | ❓ | u16 | 0 | |
| `0x06` | ❓ | u16 | 1 (0 on boxes and shapes) | |
| `0x07` | alignment | u16: 0 left, 1 centre, 2 right | 6 (❓) | ✅ |
| `0x08` | ❓ | u16 | 0 | |
| `0x09` | font size | u16 points | 14 | ✅ |
| `0x0A` | 🟡 text shadow colour | RBG4? | 0 (3D Gold: `40 40 40`) | |
| `0x0B` | 🟡 text background colour | RBG4? | white | |
| `0x0C` | ❓ flag | u16 | 0 (3D Gold: 1) | |
| `0x0D` | ❓ flag | u16 | 0 (3D Gold: 1) | |
| `0x0E` | ❓ effect | u16 | 0 (3D Gold: 7 or 8) | |

✅ Ids `0x04` and above `0x0E` have not been observed. The default section is always written in the order `06 08 07 05 02 09 0E 0A 0B 0C 0D 01 03`.

### 5.6 Group style (`0x0FFD` offset 14, `0x0041` offset 12)

| Value | Sample |
|---|---|
| 0 | Groups1 (default horizontal) |
| 2 | Groups2 |
| 3 | Groups3 |
| 4 | Groups4 |
| 5 | Groups5 |
| 6 | Groups6 |
| `0x22` | CoManager |

### 5.7 Shape type (`0x0083` offset 2)

| Value | Meaning |
|---|---|
| 1 | Rectangle |
| 2 | Text box |
| 3 | Horizontal line |
| 4 | Vertical line |
| 5 | Free (diagonal) line |

---

## 6. Quirks and hazards

- **Uninitialised memory.** Reserved and unknown fields sometimes hold leftover heap data. Examples are the banner text in `0x0001` and "r Bl" in `0x0FFD` in `.ops` files, and garbage font names in `PurpleBackground.opx`. Don't treat non-zero reserved bytes as errors.
- **Modified files.** A file edited after it was saved keeps its old checksums (§2.3).
- **Slack bytes** between the root section and the footer (§2.4).
- **Two colour byte orders** (§1.1).

## 7. Open questions

- The version fields in `0x0002` and `0x0003`, and the constant `0x005C0C1A`.
- Most of `0x0FFD`, `0x0020`, `0x0022`, `0x0042` and `0x0054`.
- `0x0093`, `0x0095` and `0x0096`. They have the same layout as Shadow; perhaps the defaults for titles, connectors and shapes.
- `0x0088` value `0x24` inside box sections.
- Box type 4, which has no text and template `0xFFFF`.
- The `0x400C` Lines section, which is always empty in the samples.
- The connector's anchor sides and its trailing `u32` values.
- `0x1000` in style files.
- The coordinate unit.
- The FieldKind (`0x0012`) values.

Samples that would answer these questions: multi-line text, more connector variants, charts with hidden fields, page setup or margin changes, zoom levels, and non-ASCII text.

## 8. Methodology

Findings came from:

1. Parsing every sample into a tree and tabulating each record id's length, parent section and distinct values.
2. Diffing samples that differ by one UI change. For example, `Shadow/*.opx` against `Layout/OneBox.opx` isolated the shadow type, offsets and the Border offset 6 code.
3. Treating the footer checksum as a black box. Pairs of files that differ by one byte showed the high word changing by `rol16(old ^ new, distance from the footer)`. That is the signature of the rotate-and-XOR checksum in §2.3.
