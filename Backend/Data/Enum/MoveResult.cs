namespace Backend.Data.Enum;

public enum MoveResult
{
        
    // Whether the move was:
    // 1. success or fail
    // 2. whether it resulted in a check or checkmate.

    Success,
    Fail,
    SuccessAndCheck,
    Checkmate

}