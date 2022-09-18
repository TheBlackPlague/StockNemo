namespace Backend.Data.Template;

public interface MoveUpdateType {}

public struct Normal : MoveUpdateType {}
public struct ClassicalUpdate : MoveUpdateType {}
public struct NNUpdate : MoveUpdateType {}
