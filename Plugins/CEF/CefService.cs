#nullable enable
using SampSharp.Core.Natives.NativeObjects;
using System;
using System.Text.Json;

namespace ProjectSMP.Plugins.CEF
{
    public class CefBridgeNatives : NativeObjectSingleton<CefBridgeNatives>
    {
        [NativeMethod]
        public virtual int CallRemoteFunction(string function, string format, params object[] args)
            => throw new NativeNotImplementedException();
    }

    public static class CefService
    {
        private static CefBridgeNatives N => CefBridgeNatives.Instance;

        public static event Action<int, bool>? OnInitialized;
        public static event Action<int, int, int>? OnBrowserCreated;
        public static event Action<int, string, string>? OnClientEvent;

        private static void Call(string func, params object[] args)
        {
            var fmt = BuildFormat(args);
            Console.WriteLine($"[CEF] Call {func} fmt={fmt} args={string.Join(",", args)}");
            N.CallRemoteFunction(func, fmt, args);
        }

        private static string BuildFormat(object[] args)
        {
            var fmt = "";
            foreach (var a in args)
                fmt += a is string ? "s" : a is float or double ? "f" : "d";
            return fmt;
        }

        public static void HandleCefInitialize(int playerId, int success)
            => OnInitialized?.Invoke(playerId, success == 1);

        public static void HandleBrowserCreated(int playerId, int browserId, int statusCode)
            => OnBrowserCreated?.Invoke(playerId, browserId, statusCode);

        public static void HandleClientEvent(int playerId, string argsJson)
        {
            try
            {
                using var doc = JsonDocument.Parse(argsJson);
                var root = doc.RootElement;
                var eventName = root.GetProperty("e").GetString() ?? "";
                var payload = root.GetProperty("d").GetRawText();
                OnClientEvent?.Invoke(playerId, eventName, payload);
            }
            catch { }
        }

        public static void OnPlayerConnect(int playerId, string ip) { }
        public static void OnPlayerDisconnect(int playerId) { }

        public static void CreateBrowser(int playerId, int browserId, string url, bool hidden = false, bool focused = true)
            => Call("CefBridge_CreateBrowser", playerId, browserId, url, hidden ? 1 : 0, focused ? 1 : 0);

        public static void DestroyBrowser(int playerId, int browserId)
            => Call("CefBridge_DestroyBrowser", playerId, browserId);

        public static void HideBrowser(int playerId, int browserId, bool hide)
            => Call("CefBridge_HideBrowser", playerId, browserId, hide ? 1 : 0);

        public static void FocusBrowser(int playerId, int browserId, bool focused)
            => Call("CefBridge_FocusBrowser", playerId, browserId, focused ? 1 : 0);

        public static void LoadUrl(int playerId, int browserId, string url)
            => Call("CefBridge_LoadUrl", playerId, browserId, url);

        public static void EmitEvent(int playerId, string eventName, object? args = null)
        {
            var json = args != null ? JsonSerializer.Serialize(args) : "{}";
            Call("CefBridge_EmitEvent", playerId, eventName, json);
        }

        public static bool HasPlugin(int playerId)
        {
            return N.CallRemoteFunction("CefBridge_HasPlugin", "d", playerId) != 0;
        }

        public static void ToggleDevTools(int playerId, int browserId, bool enabled)
            => Call("CefBridge_ToggleDevTools", playerId, browserId, enabled ? 1 : 0);

        public static void AlwaysListenKeys(int playerId, int browserId, bool listen)
            => Call("CefBridge_AlwaysListenKeys", playerId, browserId, listen ? 1 : 0);

        public static void SetAudioSettings(int playerId, int browserId, float maxDist = 50f, float refDist = 15f)
            => Call("CefBridge_SetAudioSettings", playerId, browserId, maxDist, refDist);

        public static void CreateExtBrowser(int playerId, int browserId, string texture, string url, int scale)
            => Call("CefBridge_CreateExtBrowser", playerId, browserId, texture, url, scale);

        public static void AppendToObject(int playerId, int browserId, int objectId)
            => Call("CefBridge_AppendToObject", playerId, browserId, objectId);

        public static void RemoveFromObject(int playerId, int browserId, int objectId)
            => Call("CefBridge_RemoveFromObject", playerId, browserId, objectId);
    }
}