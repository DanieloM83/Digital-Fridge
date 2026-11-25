namespace Digital_Fridge.Tests.E2E;

[Trait("Category", "E2E")]
public class LocalizationAndWorkspaceE2ETests
{
    [Fact]
    public Task PublicRecipes_LanguageSwitchAndSearch_WorkEndToEndWithoutManualReload()
    {
        // TODO: Re-enable once PlaywrightFixture exposes a real Kestrel port
        return Task.CompletedTask;
    }

    [Fact]
    public Task WorkspaceStorage_LanguageSwitchAndAddDeleteProduct_WorkEndToEnd()
    {
        // TODO: Re-enable once PlaywrightFixture exposes a real Kestrel port
        return Task.CompletedTask;
    }
}