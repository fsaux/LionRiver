-- Script Date: 7/14/2019 9:57 PM  - ErikEJ.SqlCeScripting version 3.5.2.81
CREATE TABLE [FleetTrack] (
  [Name] TEXT NOT NULL
, [timestamp] Datetime NOT NULL
, [Latitude] REAL NOT NULL
, [Longitude] REAL NOT NULL
, [COG] REAL NOT NULL
, [SOG] REAL NOT NULL
, CONSTRAINT [PK_FleetTrack] PRIMARY KEY ([Name],[timestamp])
);
