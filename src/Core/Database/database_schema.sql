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

-- BOM_Relationships table to store parent-child relationships between parts
CREATE TABLE IF NOT EXISTS "BOM_Relationships" (
    "ID" TEXT PRIMARY KEY,               -- Unique identifier for the relationship
    "ParentID" TEXT NOT NULL,            -- ID of the parent part (assembly)
    "ChildID" TEXT NOT NULL,             -- ID of the child part (component)
    "RelationType" TEXT NOT NULL,        -- Type of relationship (ASSEMBLY, MASTER_MODEL)
    "InstanceName" TEXT,                 -- Instance name of the component
    "Position" INTEGER,                  -- Position/order in the assembly
    "Quantity" INTEGER DEFAULT 1,        -- Quantity of this component in the assembly
    "Verified" INTEGER DEFAULT 0,        -- Flag indicating if the relationship has been verified (0=no, 1=yes)
    "LastUpdated" TEXT,                  -- Timestamp of when this relationship was last updated
    FOREIGN KEY ("ParentID") REFERENCES "Parts"("ID"),
    FOREIGN KEY ("ChildID") REFERENCES "Parts"("ID")
);

-- Indexes for the BOM_Relationships table
CREATE INDEX IF NOT EXISTS "idx_bom_parent" ON "BOM_Relationships"("ParentID");
CREATE INDEX IF NOT EXISTS "idx_bom_child" ON "BOM_Relationships"("ChildID");
CREATE INDEX IF NOT EXISTS "idx_bom_reltype" ON "BOM_Relationships"("RelationType");

-- AssemblyStats table to store statistics about assemblies
CREATE TABLE IF NOT EXISTS "AssemblyStats" (
    "PartID" TEXT PRIMARY KEY,           -- ID of the part (references Parts table)
    "IsAssembly" INTEGER DEFAULT 0,      -- Flag indicating if this is an assembly (0=no, 1=yes)
    "IsDrafting" INTEGER DEFAULT 0,      -- Flag indicating if this is a drafting (0=no, 1=yes)
    "ComponentCount" INTEGER DEFAULT 0,  -- Number of direct components in this assembly
    "TotalComponentCount" INTEGER DEFAULT 0, -- Total number of components in the assembly hierarchy
    "AssemblyDepth" INTEGER DEFAULT 0,   -- Depth of this assembly in the overall structure
    "ParentCount" INTEGER DEFAULT 0,     -- Number of assemblies this part appears in
    "LastAnalyzed" TEXT,                 -- Timestamp of when this assembly was last analyzed
    FOREIGN KEY ("PartID") REFERENCES "Parts"("ID")
);

-- Index for the AssemblyStats table
CREATE INDEX IF NOT EXISTS "idx_assembly_stats" ON "AssemblyStats"("IsAssembly", "ComponentCount");