A game trainer for Metal Slug X (GOG Version, setup_metal_slug_x_gog-6_(12442)).

<img width="786" height="593" alt="image" src="https://github.com/user-attachments/assets/fcf5f503-577d-4587-ad8a-13ecd4988137" />

<img width="786" height="593" alt="image" src="https://github.com/user-attachments/assets/70372b3b-911f-4b96-8773-f920ccd55c78" />

<img width="1368" height="800" alt="image" src="https://github.com/user-attachments/assets/900cd1aa-e78f-49e3-b73c-206aecdfec14" />


## Misc notes dump on reversing and offsets

Metal Slug X
setup_metal_slug_x_gog-6_(12442)

Lives
Base Address             Offset
"mslugx.exe"+000F7BB8    1288 <--------- P1=1289 P2=
"mslugx.exe"+000F7BBC    1288

Timer
Base Address             Offset
"mslugx.exe"+000F7BB8    13DC <--------- + 2 = 13DF
"mslugx.exe"+000F7BBC    13DC

Continue Timer (2 Bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    13E0 <---------
"mslugx.exe"+000F7BBC    13E0

Invincibility Timer (2 Bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    1548 <--------- P1=1548 P2=
"mslugx.exe"+000F7BBC    1548

Skip this one, could be just animations, 
Status (Mummy = 3, 7, 11)
Base Address             Offset
"mslugx.exe"+000F7BB8    1554 <--------- P1=1556 P2=
"mslugx.exe"+000F7BBC    1554

POWs (2 bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    C5FC <--------- P1=C5FC P2=C600
"mslugx.exe"+000F7BBC    C5FC

Weapon Type (Byte)
(0 - 15):
0 = Hand Gun
1 = Cannon
2 = Shotgun
3 = Rocket Launcher
4 = Flame Shot
5 = Heavy Machine Gun
6 = Laser Gun
7 = Super Shotgun
8 = Super Rocket Launcher
9 = Super Flame Shot
10 = Super Heavy Machine Gun
11 = Super Laser Gun
12 = Enemy Chaser
13 = Iron Lizard
14 = Dropshot
15 = Super Cannon

Base Address             Offset
"mslugx.exe"+000F7BB8    C71C <--------- P1=C71F P2=C71E
"mslugx.exe"+000F7BBC    C71C

Bombs (Byte)
Base Address             Offset
"mslugx.exe"+000F7BB8    C720 <--------- P1=C721 P2=C720
"mslugx.exe"+000F7BBC    C720

Ammo (2 Bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    C724 <--------- P1=C724 P2=C726
"mslugx.exe"+000F7BBC    C724

Credits (Hex) P1 - 
Base Address             Offset
"mslugx.exe"+000F7BC0    34 <--------- OK


Bomb Type


Score


Vehicle Health
Base Address             Offset
"mslugx.exe"+000F7BB8    C620

Vehicle Ammo
Base Address             Offset
"mslugx.exe"+000F7BB8    C624 <--------- C626
"mslugx.exe"+000F7BBC    C624


Vehicle Type














OFFSET_P1_CURRENT_CHARACTER = 0x0;
OFFSET_P2_CURRENT_CHARACTER = 0x0;



BASE_ADDRESS_3 = 0x000F7BC0;
OFFSET_P1_CREDITS_COUNT = 0x34;
OFFSET_P2_CREDITS_COUNT = 0x35;

BASE_ADDRESS_1 = 0x000F7BB8;
BASE_ADDRESS_2 = 0x000F7BBC;
OFFSET_P1_LIVES_COUNT = 0x1289; //1288;
OFFSET_P1_LIVES_COUNT_COMPLETE = 0x128B; // We have to set this offset to 0 after setting lives.
OFFSET_P2_LIVES_COUNT = 0x1339;
OFFSET_P2_LIVES_COUNT_COMPLETE = 0x133B; // We have to set this offset to 0 after setting lives.
OFFSET_LEVEL_TIMER = 0x13DF; //13DC;
OFFSET_CONTINUE_TIMER = 0x13E0;
OFFSET_P1_INVINCIBILITY_TIMER = 0x1548; // Set to 60
OFFSET_P1_STATUS = 0x1554; // (3, 7, 11 = Mummy, character animations, 2 bytes? ignore...)
OFFSET_P2_INVINCIBILITY_TIMER = 0x15F8; // Set to 60
OFFSET_P2_STATUS = 0x1606; // (3, 7, 11 = Mummy, character animations, 2 bytes? ignore...)
OFFSET_P1_SCORE = 0xC46A; --MAX 2574621081 ??
OFFSET_P2_SCORE = 0xC472;
OFFSET_P1_POWS_RESCUED = 0xC5FC;
OFFSET_P2_POWS_RESCUED = 0xC600;
OFFSET_P1_VEHICLE_HEALTH_COUNT = 0xC620;
OFFSET_P1_VEHICLE_AMMO_CANON_COUNT = 0xC626;
OFFSET_P2_VEHICLE_HEALTH_COUNT = 0xC644;
OFFSET_P2_VEHICLE_AMMO_CANON_COUNT = 0xC64A;
OFFSET_P1_WEAPON_TYPE = 0xC71F; //C71C
OFFSET_P2_WEAPON_TYPE = 0xC71E; //C71C
OFFSET_P1_BOMB_COUNT = 0xC721; //C720
OFFSET_P2_BOMB_COUNT = 0xC720; //C720
OFFSET_P2_BOMB_TYPE = 0xC722; // (0 = None, Grenade, Fire Bomb, Stone)
OFFSET_P1_BOMB_TYPE = 0xC723; // (0 = None, Grenade, Fire Bomb, Stone)
OFFSET_P1_AMMO_COUNT = 0xC724;
OFFSET_P2_AMMO_COUNT = 0xC726;


0D8499BA character P1   1532
0 = normal gun
1 = normal gun
2 = normal gun
3 = normal gun
4 = big gun
5 = big gun
6 = big gun
7 = big gun
8 = fat
9 = fat
10 = fat
11 = fat
12 = zombie
13 = zombie
14 = zombie
15 = zombie
16 = knife
17 = knife
18 = knife
19 = knife
20-23 = knife/radio
24-27 = fat/fork
28-31 = zombie up
32-35 = aim down
36-39 = jump/spin/stand
40-43 = fat/aim down






0DE62560 P2 inv 15F8
0DE624B0 P1 inv 1548
B0








0D8499BA P1 current char
0D8499D0 P1 Invi


0D849A6A P1 current char  160E
0D849A80 P2 Invi          15F8


0D7E1556 1606
0D7EC248 C2F8

0D7EC252 0D7EC248
"mslugx.exe"+000F7BB8
C302  C2F8    Level

0     X       Level Complete

1     0       1:2
2     0       1:3
3     0       1:4
4     0       1:5
5     0       1:1

1     1       2:2
2     1       2:3
3     1       2:4
4     1       2:5
5     1       2:6
6     1       2:7
7     1       2:8
8     1       2:9
9     1       2:A
10     1       2:B
11     1       2:C
12     1       2:D
13     1       2:1

1     2       3:2
2     2       3:3
3     2       3:1

1     3       4:2
2     3       4:3
3     3       4:4
4     3       4:5
5     3       4:1

1     4       5:2
2     4       5:3
3     4       5:4
4     4       5:1

1     5       6:2
2     5       6:3
3     5       6:4
4     5       6:5
5     5       6:1

1     6       7:2
2     6       7:3
3     6       7:4
4     6       7:5
5     6       7:6
6     6       7:7
7     6       7:8
8     6       7:9

1     7       8:2
2     7       8:3
3     7       8:4
4     7       8:3
5+    7       8:1

1     8       9:2
2     8       9:3
3     8       9:4
4     8       9:5
5     8       9:6
6     8       9:7
7     8       9:8
8     8       9:9
9     8       9:1

1     9       A:2
2     9       A:3
3     9       A:4
4     9       A:5
5     9       A:6
6     9       A:7
7     9       A:8
8     9       A:9
9     9       A:1

1    10       B:1

1    11       C:2
2    11       C:3
3    11       C:1

1    12       D:1


1    14       TEST1 for ANDY THE WIZARD
1    15       TEST2 for MEEHER
1    16       TEST3 for YOKO-SHU 
1    17       TEST4 for ナミひら (Namihira)
1    18       TEST5 for アリタ (Arita)
1    19       TEST6 for NISHINO
1    20       TEST7 for NAKATSUKA
1    21       TEST8 for TAGUCHI






Debug
"mslugx.exe"+000F7BB8
F000
Binary
Bit 0 = 1  ???
Bit 1 = 2  VIEW PA Point (sprite showing respawn point)
Bit 2 = 4  Museigen Grenade (Infinite Grenades)
Bit 3 = 8  Museigen POW Gun (Infinite Ammo)
Bit 4 = 16 ???
Bit 5 = 32 ???
Bit 6 = 64 ???
Bit 7 = 128 ???

Debug
F001
Binary
Bit 0 = 1  VIEW INFORMATION (View debug information)
Bit 1 = 2  ???
Bit 2 = 4  ???
Bit 3 = 8  VIEW GEO SCROLL A (Walkable ground)
Bit 4 = 16 VIEW GEO SCROLL B (Background)
Bit 5 = 32 VIEW ATTACK RECT (View attack hitbox)
Bit 6 = 64 VIEW BODY RECT (View body hitbox)
Bit 7 = 128 Muteki (Invincibility)


D-button, textile Zenmetsu
Itsudemo PAUSE

Metal Slug X Offsets:

Lives
Base Address             Offset
"mslugx.exe"+000F7BB8    1288 <--------- + 1 = 1289
"mslugx.exe"+000F7BBC    1288

Timer
Base Address             Offset
"mslugx.exe"+000F7BB8    13DC <--------- + 2 = 13DF
"mslugx.exe"+000F7BBC    13DC

Continue Timer (2 Bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    13E0 <---------
"mslugx.exe"+000F7BBC    13E0

Invincibility Timer (2 Bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    1548 <--------- OK
"mslugx.exe"+000F7BBC    1548

Status (Mummy = 3, 7, 11)
Base Address             Offset
"mslugx.exe"+000F7BB8    1554 <---------
"mslugx.exe"+000F7BBC    1554

POWs (2 bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    C5FC <--------- OK
"mslugx.exe"+000F7BBC    C5FC

Weapon Type (Byte)
(0 - 15):
0 = 
1 = 
2 = 
3 = 
4 = 
5 = 
6 = Laser
7 = 
8 = 
9 = 
10 = 
11 = 
12 = 
13 = 
14 = 
15 = 

Base Address             Offset
"mslugx.exe"+000F7BB8    C71C <--------- C71F
"mslugx.exe"+000F7BBC    C71C

Bombs (Byte)
Base Address             Offset
"mslugx.exe"+000F7BB8    C720 <--------- C721
"mslugx.exe"+000F7BBC    C720

Ammo (2 Bytes)
Base Address             Offset
"mslugx.exe"+000F7BB8    C724 <--------- OK
"mslugx.exe"+000F7BBC    C724

Credits (Hex) P1 - 
Base Address             Offset
"mslugx.exe"+000F7BC0    34 <--------- OK


Bomb Type


Score


Vehicle Health


Vehicle Ammo
Base Address             Offset
"mslugx.exe"+000F7BB8    C624 <--------- C626
"mslugx.exe"+000F7BBC    C624


Vehicle Type


0F1DE3EB

2B
