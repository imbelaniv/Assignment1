-- ============================================================
-- EventEase - Part 1 Database Script
-- Tables: Venues, Events, Bookings
-- Includes: Table creation, constraints, seed data
-- ============================================================

-- ============================================================
-- TABLE CREATION
-- ============================================================

CREATE TABLE Venues (
    VenueId   INT           PRIMARY KEY IDENTITY(1,1),
    VenueName NVARCHAR(255) NOT NULL,
    Location  NVARCHAR(255) NOT NULL,
    Capacity  INT           NOT NULL CHECK (Capacity > 0),
    ImageUrl  NVARCHAR(500) NULL
);

CREATE TABLE Events (
    EventId     INT           PRIMARY KEY IDENTITY(1,1),
    EventName   NVARCHAR(255) NOT NULL,
    EventDate   DATETIME2     NOT NULL,
    Description NVARCHAR(MAX) NULL,
    VenueId     INT           NULL,
    CONSTRAINT FK_Events_Venues FOREIGN KEY (VenueId)
        REFERENCES Venues(VenueId) ON DELETE SET NULL
);

CREATE TABLE Bookings (
    BookingId   INT       PRIMARY KEY IDENTITY(1,1),
    EventId     INT       NOT NULL,
    VenueId     INT       NOT NULL,
    BookingDate DATETIME2 NOT NULL,
    CONSTRAINT FK_Bookings_Events FOREIGN KEY (EventId)
        REFERENCES Events(EventId) ON DELETE NO ACTION,
    CONSTRAINT FK_Bookings_Venues FOREIGN KEY (VenueId)
        REFERENCES Venues(VenueId) ON DELETE NO ACTION
);

-- Indexes for foreign keys
CREATE INDEX IX_Events_VenueId   ON Events(VenueId);
CREATE INDEX IX_Bookings_EventId ON Bookings(EventId);
CREATE INDEX IX_Bookings_VenueId ON Bookings(VenueId);

-- EF Core migrations tracking (marks migration as applied)
CREATE TABLE __EFMigrationsHistory (
    MigrationId    NVARCHAR(150) NOT NULL PRIMARY KEY,
    ProductVersion NVARCHAR(32)  NOT NULL
);
INSERT INTO __EFMigrationsHistory VALUES ('20260315184501_InitialCreate', '9.0.0');


-- ============================================================
-- SEED DATA
-- ============================================================

INSERT INTO Venues (VenueName, Location, Capacity, ImageUrl) VALUES
('Grand Ballroom',      'Sandton, Johannesburg', 500,  'https://placehold.co/400x300?text=Grand+Ballroom'),
('Beachfront Pavilion', 'Umhlanga, Durban',      300,  'https://placehold.co/400x300?text=Beachfront+Pavilion'),
('The Garden Terrace',  'Stellenbosch, Cape Town', 150, 'https://placehold.co/400x300?text=Garden+Terrace'),
('City Conference Hall','Cape Town CBD',          800,  'https://placehold.co/400x300?text=Conference+Hall'),
('Rooftop Lounge',      'Rosebank, Johannesburg', 100,  'https://placehold.co/400x300?text=Rooftop+Lounge');

INSERT INTO Events (EventName, EventDate, Description, VenueId) VALUES
('Annual Tech Summit',      '2026-04-15 09:00', 'Annual technology conference for professionals.',   4),
('Spring Wedding Showcase', '2026-05-02 14:00', 'Wedding exhibition and showcase event.',            1),
('Jazz Night Live',         '2026-04-25 19:00', 'Live jazz performance featuring local artists.',    2),
('Corporate Strategy Day',  '2026-06-10 08:00', 'Full-day corporate planning and strategy session.', 3),
('Art & Culture Expo',      '2026-07-20 10:00', 'Exhibition showcasing local art and culture.',      NULL);

INSERT INTO Bookings (EventId, VenueId, BookingDate) VALUES
(1, 4, '2026-04-15'),
(2, 1, '2026-05-02'),
(3, 2, '2026-04-25'),
(4, 3, '2026-06-10');
