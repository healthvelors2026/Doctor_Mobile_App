namespace DoctorMobileApp.CommonClass
{
    public enum EnumIsCheckList
    {
        EnumAdmission = 0,
        EnumBedTransfer = 1,
        EnumDischarge = 2,
        EnumMRD = 3,
        EnumPreSurgery = 4,
        EnumOrientation = 5,
        EnumOrnaments = 6
    }

    public enum EnumOPDIPDFlag
    {
        EnmOPD = 0,
        EnmIPD = 1,
        EnmDirect = 2,
        EnumBlood = 3,
        EnumPatientBloodTest = 4,
        EnumIPDDischarge = 5
    }

    public enum EnumBedStatus
    {
        Vacant = 0,
        VacantButNotReleased = 1,
        VacantAndReleasedButNotCleaned = 2,
        AllotedToPatientButNotOccupied = 3,
        Occupied = 4,
        OutOfOrder = 5,
        Dependent = 6,
        NotReleasedInCaseOfMove = 7,
        TransferRequested = 8
    }

    public enum EnumBedTransferTypeInTracking
    {
        Transfer = 1,
        Move = 2,
        Swap = 3
    }

    public enum EnumBedDependencyStatus
    {
        Current = 0,
        Closed = 1,
        Cancelled = 2
    }
}