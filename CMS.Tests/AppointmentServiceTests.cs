using CMS.BLL.Services;
using CMS.DAL;
using CMS.Models.Entities;
using CMS.Models.Enums;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace CMS.Tests;

public class AppointmentServiceTests
{
    private ClinicDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ClinicDbContext(options);
    }

    private async Task<(int patientId, int doctorId, int recepId, DateTime availDate, TimeSpan startTime, TimeSpan endTime)>
        SeedBasicDataAsync(ClinicDbContext context)
    {
        var patient = new Patient { FullName = "Test Patient", Phone = "111" };
        var doctor = new Doctor { FullName = "Test Doctor", Phone = "222", Email = "doc@test.com", LicenseNumber = "LIC1" };

        // تم إزالة Department لأنها غير موجودة في نموذج Receptionist
        var receptionist = new Receptionist { FullName = "Test Recp", Phone = "333" }; // <-- تعديل

        context.AddRange(patient, doctor, receptionist);
        await context.SaveChangesAsync();

        DateTime tomorrow = DateTime.UtcNow.Date.AddDays(1);
        byte dayOfWeek = (byte)tomorrow.DayOfWeek;

        var availability = new DoctorAvailability
        {
            DoctorId = doctor.Id,
            DayOfWeek = dayOfWeek,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        };

        context.DoctorAvailabilities.Add(availability);
        await context.SaveChangesAsync();

        return (patient.Id, doctor.Id, receptionist.Id, tomorrow, availability.StartTime, availability.EndTime);
    }

    private async Task<int> SeedAppointmentAsync(ClinicDbContext context, AppointmentStatus status)
    {
        var (patientId, doctorId, recepId, date, start, end) = await SeedBasicDataAsync(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30)),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        return appointment.Id;
    }

    // ════════════════════════════════════════
    // UC-09: BookAsync Tests (TC-01 to TC-10)
    // ════════════════════════════════════════

    [Fact]
    public async Task BookAsync_ValidData_ReturnsAppointmentWithScheduledStatus()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, date, start, end) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        };

        var result = await service.BookAsync(appointment);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task BookAsync_PatientNotFound_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (_, doctorId, recepId, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = 99,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Patient not found");
    }

    [Fact]
    public async Task BookAsync_DoctorNotFound_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, _, recepId, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = 99,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor not found");
    }

    [Fact]
    public async Task BookAsync_ReceptionistNotFound_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, _, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = 99,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Receptionist not found");
    }

    [Fact]
    public async Task BookAsync_DateInPast_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, _, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = DateTime.UtcNow.Date.AddDays(-1),
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Cannot book an appointment in the past");
    }

    [Fact]
    public async Task BookAsync_DoctorNotAvailableOnDay_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, _, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        DateTime nextWeek = DateTime.UtcNow.Date.AddDays(7);

        // تحويل DayOfWeek إلى int قبل العملية الحسابية
        byte wrongDay = (byte)(((int)nextWeek.DayOfWeek + 1) % 7); // <-- تعديل

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = nextWeek,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor is not available on this day");
    }

    [Fact]
    public async Task BookAsync_OutsideWorkingHours_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, date, _, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = new TimeSpan(17, 0, 0),
            EndTime = new TimeSpan(17, 30, 0)
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment is outside doctor's working hours");
    }

    [Fact]
    public async Task BookAsync_DurationNot30Minutes_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appointment = new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(60))
        };

        var act = () => service.BookAsync(appointment);
        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment duration must be exactly 30 minutes");
    }

    [Fact]
    public async Task BookAsync_TimeSlotTaken_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        await service.BookAsync(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        });

        var act = () => service.BookAsync(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        });

        await act.Should().ThrowAsync<Exception>().WithMessage("This time slot is not available");
    }

    [Fact]
    public async Task BookAsync_CancelledSlotCanBeRebooked_ReturnsAppointment()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var booked = await service.BookAsync(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        });
        await service.CancelAsync(booked.Id);

        var act = await service.BookAsync(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        });

        act.Should().NotBeNull();
        act.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    // ════════════════════════════════════════
    // UC-10: ConfirmAsync Tests (TC-11 to TC-14)
    // ════════════════════════════════════════

    [Fact]
    public async Task ConfirmAsync_ScheduledAppointment_ChangesToConfirmed()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var result = await service.ConfirmAsync(id);
        result.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmAsync_RescheduledAppointment_ChangesToConfirmed()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Rescheduled);
        var service = new AppointmentService(context);

        var result = await service.ConfirmAsync(id);
        result.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public async Task ConfirmAsync_AppointmentNotFound_ThrowsException()
    {
        var context = GetInMemoryContext();
        var service = new AppointmentService(context);

        var act = () => service.ConfirmAsync(99);
        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment not found");
    }

    [Fact]
    public async Task ConfirmAsync_CompletedAppointment_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Completed);
        var service = new AppointmentService(context);

        var act = () => service.ConfirmAsync(id);
        await act.Should().ThrowAsync<Exception>().WithMessage("Only Scheduled or Rescheduled appointments can be confirmed");
    }

    // ════════════════════════════════════════
    // UC-11: CancelAsync Tests (TC-15 to TC-18)
    // ════════════════════════════════════════

    [Fact]
    public async Task CancelAsync_ScheduledAppointment_ChangesToCancelled()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var result = await service.CancelAsync(id);
        result.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_ConfirmedAppointment_ChangesToCancelled()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Confirmed);
        var service = new AppointmentService(context);

        var result = await service.CancelAsync(id);
        result.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public async Task CancelAsync_CompletedAppointment_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Completed);
        var service = new AppointmentService(context);

        var act = () => service.CancelAsync(id);
        await act.Should().ThrowAsync<Exception>().WithMessage("Cannot cancel a completed appointment");
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Cancelled);
        var service = new AppointmentService(context);

        var act = () => service.CancelAsync(id);
        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment is already cancelled");
    }

    // ════════════════════════════════════════
    // UC-12: CompleteAsync Tests (TC-26 to TC-27)
    // ════════════════════════════════════════

    [Fact]
    public async Task CompleteAsync_ConfirmedAppointment_ChangesToCompleted()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Confirmed);
        var service = new AppointmentService(context);

        var result = await service.CompleteAsync(id);
        result.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public async Task CompleteAsync_NonConfirmedAppointment_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var act = () => service.CompleteAsync(id);
        await act.Should().ThrowAsync<Exception>().WithMessage("Only confirmed appointments can be completed");
    }

    // ════════════════════════════════════════
    // UC-13: RescheduleAsync Tests (TC-19 to TC-25)
    // ════════════════════════════════════════

    [Fact]
    public async Task RescheduleAsync_ValidData_ChangesToRescheduled()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var doctor = await context.Doctors.FirstAsync();
        DateTime nextWeek = DateTime.UtcNow.Date.AddDays(7);
        byte newDay = (byte)nextWeek.DayOfWeek;

        context.DoctorAvailabilities.Add(new DoctorAvailability
        {
            DoctorId = doctor.Id,
            DayOfWeek = newDay,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(15, 0, 0)
        });
        await context.SaveChangesAsync();

        var result = await service.RescheduleAsync(id, nextWeek, new TimeSpan(10, 0, 0), new TimeSpan(10, 30, 0));

        result.Status.Should().Be(AppointmentStatus.Rescheduled);
        result.AppointmentDate.Should().Be(nextWeek);
    }

    [Fact]
    public async Task RescheduleAsync_DateInPast_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var act = () => service.RescheduleAsync(id, DateTime.UtcNow.Date.AddDays(-1), new TimeSpan(10, 0, 0), new TimeSpan(10, 30, 0));
        await act.Should().ThrowAsync<Exception>().WithMessage("Cannot reschedule to a past date");
    }

    [Fact]
    public async Task RescheduleAsync_NewSlotTaken_ThrowsException()
    {
        var context = GetInMemoryContext();
        var (patientId, doctorId, recepId, date, start, _) = await SeedBasicDataAsync(context);
        var service = new AppointmentService(context);

        var appt1 = await service.BookAsync(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start,
            EndTime = start.Add(TimeSpan.FromMinutes(30))
        });
        var appt2 = await service.BookAsync(new Appointment
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ReceptionistId = recepId,
            AppointmentDate = date,
            StartTime = start.Add(TimeSpan.FromMinutes(30)),
            EndTime = start.Add(TimeSpan.FromMinutes(60))
        });

        var act = () => service.RescheduleAsync(appt2.Id, date, start, start.Add(TimeSpan.FromMinutes(30)));
        await act.Should().ThrowAsync<Exception>().WithMessage("This time slot is not available");
    }

    [Fact]
    public async Task RescheduleAsync_CompletedAppointment_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Completed);
        var service = new AppointmentService(context);

        var act = () => service.RescheduleAsync(id, DateTime.UtcNow.Date.AddDays(2), new TimeSpan(10, 0, 0), new TimeSpan(10, 30, 0));
        await act.Should().ThrowAsync<Exception>().WithMessage("Only Scheduled or Confirmed appointments can be rescheduled");
    }

    [Fact]
    public async Task RescheduleAsync_DoctorNotAvailableOnNewDay_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        DateTime nextWeek = DateTime.UtcNow.Date.AddDays(7);

        // تحويل DayOfWeek إلى int قبل العملية الحسابية
        byte wrongDay = (byte)(((int)nextWeek.DayOfWeek + 3) % 7); // <-- تعديل

        var act = () => service.RescheduleAsync(id, nextWeek, new TimeSpan(10, 0, 0), new TimeSpan(10, 30, 0));
        await act.Should().ThrowAsync<Exception>().WithMessage("Doctor is not available on this day");
    }

    [Fact]
    public async Task RescheduleAsync_OutsideWorkingHours_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var doctor = await context.Doctors.FirstAsync();
        DateTime nextWeek = DateTime.UtcNow.Date.AddDays(7);
        byte newDay = (byte)nextWeek.DayOfWeek;

        context.DoctorAvailabilities.Add(new DoctorAvailability
        {
            DoctorId = doctor.Id,
            DayOfWeek = newDay,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        });
        await context.SaveChangesAsync();

        var act = () => service.RescheduleAsync(id, nextWeek, new TimeSpan(15, 0, 0), new TimeSpan(15, 30, 0));
        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment is outside doctor's working hours");
    }

    [Fact]
    public async Task RescheduleAsync_DurationNot30Minutes_ThrowsException()
    {
        var context = GetInMemoryContext();
        var id = await SeedAppointmentAsync(context, AppointmentStatus.Scheduled);
        var service = new AppointmentService(context);

        var doctor = await context.Doctors.FirstAsync();
        DateTime nextWeek = DateTime.UtcNow.Date.AddDays(7);
        byte newDay = (byte)nextWeek.DayOfWeek;

        context.DoctorAvailabilities.Add(new DoctorAvailability
        {
            DoctorId = doctor.Id,
            DayOfWeek = newDay,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(17, 0, 0)
        });
        await context.SaveChangesAsync();

        var act = () => service.RescheduleAsync(id, nextWeek, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
        await act.Should().ThrowAsync<Exception>().WithMessage("Appointment duration must be exactly 30 minutes");
    }
}