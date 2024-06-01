﻿using Microsoft.JSInterop;
using SwashbucklerDiary.Rcl.Essentials;
using SwashbucklerDiary.WebAssembly.Extensions;

namespace SwashbucklerDiary.WebAssembly.Essentials
{
    public class AppLifecycle : IAppLifecycle
    {
        public event Action<ActivationArguments>? Activated;

        public event Action? Resumed;

        public event Action? Stopped;

        private readonly Lazy<ValueTask<IJSInProcessObjectReference>> _module;

        public ActivationArguments? ActivationArguments { get; set; }

        public AppLifecycle(IJSRuntime jSRuntime)
        {
            _module = new(() => ((IJSInProcessRuntime)jSRuntime).ImportJsModule("js/appLifecycle.js"));
        }

        [JSInvokable]
        public void OnResume() => Resumed?.Invoke();

        [JSInvokable]
        public void OnStop() => Stopped?.Invoke();

        public async void QuitApp()
        {
            var module = await _module.Value;
            module.InvokeVoid("quit");
        }

        public async Task InitializedAsync()
        {
            var module = await _module.Value;
            var dotNetObject = DotNetObjectReference.Create(this);
            module.InvokeVoid("init", dotNetObject, nameof(OnStop), nameof(OnResume));
        }
    }
}
