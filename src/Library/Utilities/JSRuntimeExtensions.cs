
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ScrumStorySizer.Library.Utilities;

public static class JSRuntimeExtensions
{
    private static async ValueTask ExceptionWrapper(Func<ValueTask> function)
    {
        try { await function(); }
        catch (Exception) { }
    }

    private static async ValueTask<T> ExceptionWrapper<T>(Func<ValueTask<T>> function)
    {
        try { return await function(); }
        catch (Exception) { return default; }
    }

    public static async ValueTask BlurUtil(this IJSRuntime jSRuntime, ElementReference elementRef)
    {
        await ExceptionWrapper(async () => await jSRuntime.InvokeVoidAsync("utils.blur", elementRef));
    }

    public static async ValueTask RefocusUtil(this IJSRuntime jSRuntime)
    {
        await ExceptionWrapper(async () => await jSRuntime.InvokeVoidAsync("utils.refocus"));
    }

    public static async ValueTask StartConfetti(this IJSRuntime jSRuntime, int timeout)
    {
        await ExceptionWrapper(async () => await jSRuntime.InvokeVoidAsync("confetti.start", timeout));
    }

    public static async ValueTask<string> GetItemLocalStorage(this IJSRuntime jSRuntime, string key)
    {
        return await ExceptionWrapper(async () => await jSRuntime.InvokeAsync<string>("localStorage.getItem", key));
    }

    public static async ValueTask SetItemLocalStorage(this IJSRuntime jSRuntime, string key, string value)
    {
        await ExceptionWrapper(async () => await jSRuntime.InvokeVoidAsync("localStorage.setItem", key, value));
    }
}