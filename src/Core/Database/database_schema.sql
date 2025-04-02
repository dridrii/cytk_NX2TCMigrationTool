-- PLMIntegration Database Schema

-- Parts table for storing part information
CREATE TABLE IF NOT EXISTS Parts (
    ID TEXT PRIMARY KEY,      -- Unique identifier for the part
    Name TEXT NOT NULL,       -- Name of the part
    Type TEXT NOT NULL,       -- Type/category of the part
    Source TEXT NOT NULL,     -- Source system (NX, Teamcenter, etc.)
    Metadata TEXT             -- JSON serialized metadata/attributes
);

-- Index for fast lookup by part name
CREATE INDEX IF NOT EXISTS idx_parts_name ON Parts(Name);

-- Index for filtering by source system
CREATE INDEX IF NOT EXISTS idx_parts_source ON Parts(Source);

-- The following CREATE statements use proper SQL syntax for index creation
-- These are commented out as they relate to tables that don't exist yet

-- Possible future tables (not currently implemented):

/*
-- PartRelationships table for tracking part assembly structure
CREATE TABLE IF NOT EXISTS PartRelationships (
    ParentID TEXT NOT NULL,
    ChildID TEXT NOT NULL, 
    Quantity INTEGER NOT NULL DEFAULT 1,
    PRIMARY KEY (ParentID, ChildID),
    FOREIGN KEY (ParentID) REFERENCES Parts(ID),
    FOREIGN KEY (ChildID) REFERENCES Parts(ID)
);

-- Create indexes for PartRelationships table
CREATE INDEX IF NOT EXISTS idx_part_relationships_parent ON PartRelationships(ParentID);
CREATE INDEX IF NOT EXISTS idx_part_relationships_child ON PartRelationships(ChildID);

-- MigrationJobs table for tracking migration status
CREATE TABLE IF NOT EXISTS MigrationJobs (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    PartID TEXT NOT NULL,
    Status TEXT NOT NULL,
    StartTime DATETIME,
    EndTime DATETIME,
    Log TEXT,
    FOREIGN KEY (PartID) REFERENCES Parts(ID)
);

-- Create indexes for MigrationJobs
CREATE INDEX IF NOT EXISTS idx_migration_jobs_part ON MigrationJobs(PartID);
CREATE INDEX IF NOT EXISTS idx_migration_jobs_status ON MigrationJobs(Status);

-- UserSettings table for storing application settings
CREATE TABLE IF NOT EXISTS UserSettings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL
);
*/