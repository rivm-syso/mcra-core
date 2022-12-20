using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum UserRoles {
        Administrator,
        Developer,
    }

    public enum SpecialUserGroups {
        AllUsers = -1,
    }

    public enum UserGroupMembershipRole {
        None = -1,
        Visible = 0,
        Member = 1,
        Maintainer = 2,
    }

    public enum AccessLevel {
        None = -1,
        Visible = 0,
        Use = 1,
        Read = 2,
        Write = 3,
        Full = 4,
    }

    public enum DeleteMethod {
        DeleteImmediately,
        ScheduleForDeletion,
    }

    public enum MCRATaskType {
        [Display(Name = "Unknown")]
        Unknown = -1,
        [Display(Name = "Simulation")]
        Simulation = 0,
        [Display(Name = "Data source calculation")]
        DataSourceCompilation = 1,
        [Display(Name = "Food conversion")]
        FoodConversion = 2,
        [Display(Name = "Exposure calculation")]
        IntakeCalculation = 3,
        [Display(Name = "Concentration modelling")]
        ConcentrationModelling = 4,
        [Display(Name = "Exposure screening")]
        ExposureScreening = 5,
        [Display(Name = "Loop calculation")]
        LoopCalculation = 6,
        [Display(Name = "Loop reporting")]
        LoopReporting = 7,
    }

    public enum MCRATaskStatus {
        [Display(Name = "faulted")]
        Faulted = -1,
        [Display(Name = "waiting for activation")]
        WaitingForActivation = 0,
        [Display(Name = "running")]
        Running = 1,
        [Display(Name = "ran to completion")]
        RanToCompletion = 2,
        [Display(Name = "canceled")]
        Canceled = 3,
        [Display(Name = "aborting")]
        Aborting = 4,
        [Display(Name = "unknown")]
        Unknown = 5,
        [Display(Name = "ready")]
        Ready = 6,
    }

    public enum ServiceState {
        Idle,
        Running,
        Unavailable,
    }

    public enum RawDataSourceFileType {
        [Display(Name = "MS Access (.mdb)")]
        MicrosoftAccessFileType,
        [Display(Name = "MS Excel (.xls, .xlsx)")]
        MicrosoftExcelFileType,
        [Display(Name = "Unknown file type")]
        UnknownFileType,
        [Display(Name = "SQLite Database")]
        SQLiteDatabaseType,
        [Display(Name = "Compressed Zip Archive")]
        ZipArchiveType,
        [Display(Name = "Comma separated values (.csv)")]
        CsvFileType
    }

    public enum ActionZipDataExportFormat {
        NoData,
        CsvData,
        OriginalData
    }

    public enum DataDownloadFormat {
        McraFormat,
        OriginalData,
        Metadata
    }
}
