using ClinicManagementSystem.Models.ViewModels;

namespace ClinicManagementSystem.Interfaces.Services;

/// <summary>
/// Service for retrieving patient visit history and audit information
/// All queries are read-only and optimized for performance
/// </summary>
public interface IPatientHistoryService
{
    /// <summary>
    /// Gets summary statistics for a patient's visit history
    /// Excludes voided visits from calculations
    /// </summary>
    Task<PatientHistorySummaryDto> GetPatientSummaryAsync(int patientId);

    /// <summary>
    /// Gets all visits for a patient ordered by date (newest first)
    /// Includes both active and voided visits
    /// </summary>
    Task<PatientVisitTimelineDto> GetPatientVisitsTimelineAsync(int patientId);

    /// <summary>
    /// Gets previous visits (newest first) excluding the specified visit
    /// Used to display related visits in Visit Details view
    /// </summary>
    Task<List<PreviousVisitDto>> GetPreviousVisitsAsync(int patientId, int excludeVisitId, int take = 5);
}
