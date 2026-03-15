-- Run this script in Azure SQL Query Editor after deploying the app
-- Creates the consolidated booking details view used by the Bookings Index page

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
JOIN Events   e ON b.EventId  = e.EventId
JOIN Venues   v ON b.VenueId  = v.VenueId;
