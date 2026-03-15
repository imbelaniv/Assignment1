-- ============================================================
-- EventEase - Part 3 Database Script
-- Adds: EventType lookup table, IsAvailable field on Venues
-- Run in Azure SQL Query Editor after Part 2 script
-- ============================================================

-- EventType lookup table
CREATE TABLE EventType (
    EventTypeId INT           PRIMARY KEY IDENTITY(1,1),
    TypeName    NVARCHAR(100) NOT NULL
);

-- Seed event types
INSERT INTO EventType (TypeName) VALUES
('Conference'),
('Wedding'),
('Concert'),
('Corporate'),
('Exhibition'),
('Other');

-- Add EventTypeId FK to Events
ALTER TABLE Events
    ADD EventTypeId INT NULL
        CONSTRAINT FK_Events_EventType FOREIGN KEY REFERENCES EventType(EventTypeId);

-- Add IsAvailable flag to Venues
ALTER TABLE Venues
    ADD IsAvailable BIT NOT NULL DEFAULT 1;
