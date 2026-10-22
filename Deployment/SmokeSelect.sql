SELECT TOP (5) BranchId, BranchName, IsActive FROM Branches ORDER BY CreatedDate DESC;
SELECT TOP (5) ServiceId, ServiceName, IsActive FROM Services ORDER BY CreatedDate DESC;
SELECT TOP (5) AppointmentId, AppointmentReference, Status, CreatedDate FROM Appointments ORDER BY CreatedDate DESC;
