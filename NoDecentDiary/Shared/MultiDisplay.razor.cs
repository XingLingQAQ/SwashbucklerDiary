﻿using Masa.Blazor;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoDecentDiary.Shared
{
    public partial class MultiDisplay
    {
        [Inject]
        public MasaBlazor? MasaBlazor { get; set; }

        [Parameter]
        public RenderFragment? MobileContent { get; set; }
        [Parameter]
        public RenderFragment? DesktopContent { get; set; }

        protected override Task OnInitializedAsync()
        {
            MasaBlazor!.Breakpoint.OnUpdate += () => { return InvokeAsync(this.StateHasChanged); };
            return base.OnInitializedAsync();
        }
    }
}