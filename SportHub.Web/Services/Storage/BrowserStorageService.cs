using Microsoft.JSInterop;

namespace SportHub.Web.Services.Storage;

public class BrowserStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask<string?> GetItemAsync(string key)
    {
        return _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public ValueTask SetItemAsync(string key, string value)
    {
        return _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public ValueTask RemoveItemAsync(string key)
    {
        return _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}

