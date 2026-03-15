-- ============================================================
-- EventEase - Part 2 Database Script
-- Creates the consolidated booking details view
-- Run in Azure SQL Query Editor after Part 1 script
-- ============================================================

CREATE OR ALTER VIEW vw_BookingDetails AS
SELECT
    b.BookingId,
    b.BookingDate,
    e.EventId,
    e.EventName,
    e.EventDate,
    e.Description,
    v.VenueId,
    v.VenueName,
    v.Location,
    v.Capacity,
    v.ImageUrl
FROM Bookings b
JOIN Events e ON b.EventId = e.EventId
JOIN Venues v ON b.VenueId = v.VenueId;
