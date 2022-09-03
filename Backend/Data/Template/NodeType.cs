namespace Backend.Data.Template;

internal interface NodeType {}

internal struct PvNode : NodeType {}
internal struct NonPvNode : NodeType {}
internal struct RootNode : NodeType {}