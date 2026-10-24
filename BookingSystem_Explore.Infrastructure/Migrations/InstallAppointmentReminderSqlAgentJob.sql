USE [msdb];
GO

IF EXISTS (SELECT 1 FROM dbo.sysjobs WHERE [name] = N'BookingSystem - Queue Appointment Reminders')
	EXEC dbo.sp_delete_job @job_name = N'BookingSystem - Queue Appointment Reminders';
GO

EXEC dbo.sp_add_job
	@job_name = N'BookingSystem - Queue Appointment Reminders',
	@enabled = 1,
	@description = N'Queues idempotent 24-hour appointment reminders using the procedure shared with the API worker.';
GO

EXEC dbo.sp_add_jobstep
	@job_name = N'BookingSystem - Queue Appointment Reminders',
	@step_name = N'Queue reminders',
	@subsystem = N'TSQL',
	@database_name = N'BookingSystem',
	@command = N'EXEC dbo.QueueAppointmentReminders;',
	@on_success_action = 1,
	@on_fail_action = 2;
GO

EXEC dbo.sp_add_schedule
	@schedule_name = N'BookingSystem Reminder Every 5 Minutes',
	@freq_type = 4,
	@freq_interval = 1,
	@freq_subday_type = 4,
	@freq_subday_interval = 5,
	@active_start_time = 0;
GO

EXEC dbo.sp_attach_schedule
	@job_name = N'BookingSystem - Queue Appointment Reminders',
	@schedule_name = N'BookingSystem Reminder Every 5 Minutes';
GO

EXEC dbo.sp_add_jobserver
	@job_name = N'BookingSystem - Queue Appointment Reminders';
GO
