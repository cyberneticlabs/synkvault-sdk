namespace SynkVault.Sdk.Tests;

public sealed class SynkVaultErrorTests
{
    [Fact]
    public void Constructor_SetsStatusCodeAndMessage()
    {
        var ex = new SynkVaultException(404, "Not found");

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal("Not found", ex.Message);
        Assert.Null(ex.ResponseData);
    }

    [Fact]
    public void Constructor_SetsResponseData_WhenProvided()
    {
        var data = new { foo = "bar" };
        var ex = new SynkVaultException(500, "Error", data);

        Assert.Equal(500, ex.StatusCode);
        Assert.Equal("Error", ex.Message);
        Assert.Same(data, ex.ResponseData);
    }

    [Fact]
    public void IsInstanceOf_Exception()
    {
        var ex = new SynkVaultException(400, "Bad request");

        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void InstanceOf_SynkVaultException_Works()
    {
        Exception ex = new SynkVaultException(403, "Forbidden");

        Assert.IsType<SynkVaultException>(ex);
        var typed = (SynkVaultException)ex;
        Assert.Equal(403, typed.StatusCode);
    }
}
