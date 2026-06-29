public sealed record StoryRequest(string Input);

public sealed record StoryResponse(string Reply, string? SceneName = null, string? SceneId = null);