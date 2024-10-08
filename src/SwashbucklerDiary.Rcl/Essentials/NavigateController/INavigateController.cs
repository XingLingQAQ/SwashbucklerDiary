﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SwashbucklerDiary.Rcl.Essentials
{
    public interface INavigateController
    {
        bool IsInitialized { get; }

        bool CanPageUpdate { get; }

        List<string> PageCachePaths { get; }

        RouteMatcher RouteMatcher { get; }

        void Init(NavigationManager navigationManager, IJSRuntime jSRuntime, IEnumerable<string> uris);

        void RemovePageCache(string url);

        void AddHistoryAction(Action action, bool preventNavigation = true);

        void AddHistoryAction(Func<Task> func, bool preventNavigation = true);

        void RemoveHistoryAction(Action action);

        void RemoveHistoryAction(Func<Task> func);
    }
}
