-- PLMIntegration Database Schema

-- Parts table for storing part information
CREATE TABLE IF NOT EXISTS "Parts" (
    "ID" TEXT PRIMARY KEY,      -- Unique identifier for the part
    "Name" TEXT NOT NULL,       -- Name of the part
    "Type" TEXT NOT NULL,       -- Type/category of the part
    "Source" TEXT NOT NULL,     -- Source system (NX, Teamcenter, etc.)
    "FilePath" TEXT,            -- Full path to the file
    "FileName" TEXT,            -- Name of the file with extension
    "Checksum" TEXT,            -- SHA-256 checksum of the file
    "IsDuplicate" INTEGER DEFAULT 0, -- Flag for duplicate files (0=no, 1=yes)
    "DuplicateOf" TEXT,         -- ID of the original part this is a duplicate of
    "Metadata" TEXT             -- JSON serialized metadata/attributes
);

-- Index for fast lookup by part name
CREATE INDEX IF NOT EXISTS "idx_parts_name" ON "Parts"("Name");

-- Index for filtering by source system
CREATE INDEX IF NOT EXISTS "idx_parts_source" ON "Parts"("Source");

-- Index for checksum lookups (for duplicate detection)
CREATE INDEX IF NOT EXISTS "idx_parts_checksum" ON "Parts"("Checksum");

-- Index for duplicate flag
CREATE INDEX IF NOT EXISTS "idx_parts_duplicate" ON "Parts"("IsDuplicate");