## Global

All properties are little-endian.

## Records and Sections

The file contains records and sections, identified by a 2-byte id field.

If `id < 0x4000`, then this is a record. The length of the record is given by a 2-byte field length field following the id.

If `id >= 0x4000`, then this is a section marker. Sections have no length. A section contains records (and sub-sections) and continues until it encounters a record with `id == startId + 0x2000`.

## File Structure

The file begins with the 4-byte signature "UOCF".

### Overall Structure

The file contains the unknown record 0x0001 which contains 0x90 zero bytes.

The file contains the unknown record 0x0003.

The file contains the unknown record 0x0002.

The file contains the parent section (0x4001).

The file contains the footer record 0x0FFF which contains the file size and CRC.

### Footer

The footer is the last 14 bytes of the file: id `0x0FFF`, length `10`, then a 2-byte size (also `10`), a 4-byte CRC and a 4-byte file size. The file size is the offset of the footer, i.e. the length of the file minus 14; every sample satisfies this, and the C# reader rejects files where it does not hold.

The low 16 bits of the CRC equal the sum of all bytes before the footer modulo 65536 in every sample except `Custom.opx`. The algorithm for the high 16 bits is not known, so the CRC is not validated.

### Stale bytes

The parent section is normally followed immediately by the footer. A few samples (`Simple.opx`, `Box/Name1.opx`) have extra bytes between the parent section's end marker (0x6001) and the final footer. These look like leftovers from an earlier, longer save of the same file; they include a stale footer, and in `Simple.opx` part of an older document. The C# reader exposes them as `OrganizationChart.SlackBytes`.

### Parent Section

The parent section (0x4001) contains the container section (0x4002).

### Container Section

The container section (0x4002) contains:
- The global info section (0x4005)
- The canvas section (0x4003)

### Global Info Section

The global info section (0x4005) describes the global state of the file. It contains:
- The text part count record (0x0010) describes the number of text parts in the following section
- The box text parts section (0x4008) describes the text parts
- The global info record (0x0FFD) describes information about the global state of the file

### Canvas Section

The canvas section (0x4003) describes the canvas of the file. It contains:
- The unknown record 0x0021
- The body info record (0x0020) describes the background color
- The unknown record 0x0098
- The unknown record 0x0022
- The unknown record 0x009B
- The unknown section 0x400A
- The font section (0x400B) describes information about the fonts used in the file
- The text style section (0x4010) describes information about the text style of the title
- The title section (0x4009) describes information about the title of the file
- The unknown record 0x0024
- The unknown record 0x0023
- The unknown record 0x0052
- The border record (0x0053) describes information about the border of the title (style, line style, color)
- The shadow info record (0x0092) describes information about the shadow of the title
- The unknown record 0x0054
- The unknown record 0x0093
- The unknown record 0x0095
- The unknown record 0x0096
- The levels section (0x4006) describes information about the levels in the file
- The connectors section (0x4012) describes information about the user-defined connectors between boxes in the file 
- The shapes section (0x4012) describes information about the user-defined shapes defined in the file 

### Levels Section
The levels section (0x4006) describes all the levels with different logical hierachy in the file. It is a repeating series of records:
- The unknown record 0x0041
- The unknown record 0x0042
- The level section (0x4007) describes information about each level of the file
- The lines section (0x400C) describes information about the lines between boxes

### Level Section
The level section (0x4007) describes multiple boxes with the same logical hierachy in the file. It is a repeating series of records:
- The box index record (0x0050) describes the one-based index of the box (1, 2, 3...)
- The unknown record 0x0052
- The border record (0x0053) describes information about the border of the box (style, line style, color)
- The shadow record (0x0092) describes information about the shadow of the box
- The box text parts section (0x4008) describes informations about the text parts of the box
- [Optional] the unknown record 0x0054

### Box Text Parts Section
The box text parts section (0x4008) describes the text content of a box in the file. It is a repeating series of records:
- The box part index record (0x0011) describes the zero-based index/ID of the text part (Name, Title, Comment1, Comment2)
- The box part type record (0x0012) describes the type of the text part (Name/Title, Comment1/Comment2)
- The text value record (0x0060) describes the value of the text part
- [Optional] the text style info section (0x4010) describes the style of the text part

### Title Section
The title section (0x4009) describes the title of the file. It is a repeating series of records:
- The title part index record (0x0026) describes the zero-based index/ID of the title part (?, Title, ?, ?, ?, ?)
- The text record (0x0060) describes the value of the title part
- [Optional] the text style info section (0x4010) describes the style of the title part

### Font Section
The font section (0x400B) describes the fonts used in the file. It is a repeating series of records:
- The font index record (0x009C) describes the zero-based index of the font.
- The font name record (0x0025) describes the name of the font.

### Lines Section
The lines section (0x400C) describes the lines associated with a level. It is a repeating series of records:
- The line record (0x0056) describes the thickness, stye, color etc. of the line.

### Shapes Section
The shapes section (0x400E) describes the user-defined shapes in the file. It is a repeating series of records:
- The shape record (0x0053) describes the shape of the record (location, width, height).
- The border record (0x0053) describes information about the border of the shape (style, line style, color).
- The shadow info record (0x0092) describes information about the shadow of the shape.
- The shape color record (0x0084) describes information about the background color of the shape.

#### Text Style Section
The text style section (0x4010) describes the text styles of a text value. It is a repeating series of records:
- The style record (0x0061) describes the text align, size etc. of the text.

### Connectors Section
The connectors section (0x4012) describes the user-defined connectors between boxes in the file. It is a repeating series of records:
- The line record (0x0056) describes the thickness, style, color etc. of the line. 
- [Optional] the unknown record 0x0088 - probably some sort of terminator?
