# WorldFlag Format

It controls the state of the world at each flag change of the story. Changes are only applied to the worlds that are specified in each entry.

This file is located inside `parag_xx.arc` where `xx` are the initial of the character name. The internal file is named `w_flag_xx.bin` where xx means the same as explained before.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32  | Version, always `1`.
| 0x4     | uint32  | Data Count

## World Flag Data Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8  | ID
| 0x1     | uint8  | Padding
| 0x2     | uint16 | Paragraph
| 0x4     | uint32 | World Flag Data Offset

## World Flag Data

Each world flag data consists of sections like this until it reaches the end of the flag definition which is a World Code and World Count of 0.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8  | [Code](#World-Code)
| 0x1     | uint8  | World Count
| 0x2     | uint16[WorldCount] | World ID

### World Code

| Value | Name | Description
|--------|------------|-----
| 0      | _CODE_END            | 
| 1      | _CODE_NEW_EPISODE    | 
| 2      | _CODE_OLD_EPISODE    | 
| 3      | _CODE_HIDDEN         | World is hidden from view
| 4      | _CODE_DARKNESS       | World is darkened
| 5      | _CODE_FREE           | World appears in the worldmap
| 6      | _CODE_LANDABLE       | World can be landed on
| 7      | _CODE_SHIELD         | World protected by a shield
| 8      | _CODE_CLEARED        | World cleared
| 9      | _CODE_NEXT           | World moves to the next flag
| 10     | _CODE_ROUTELESS      | World isn't connected by world lines
| 11     | _CODE_DISCONNECT     | 
| 12     | _CODE_CONNECT        | 
| 13     | _CODE_DICE_OPEN      | 
| 14     | _CODE_REFUSE         | 
| 15     | _CODE_UNKNOWN        | 
| 16     | _CODE_KNOWN          | 
| 17     | _CODE_MESSAGE        | 
| 18     | _CODE_UNCEAR         | 