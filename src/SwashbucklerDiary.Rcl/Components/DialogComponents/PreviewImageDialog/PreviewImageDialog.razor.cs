﻿using Microsoft.AspNetCore.Components;
using SwashbucklerDiary.Rcl.Services;
using SwashbucklerDiary.Shared;

namespace SwashbucklerDiary.Rcl.Components
{
    public partial class PreviewImageDialog : DialogComponentBase
    {
        private readonly string id = $"zoom-{Guid.NewGuid()}";

        private bool isInitialized;

        [Inject]
        private IMediaResourceManager MediaResourceManager { get; set; } = default!;

        [Inject]
        private ZoomJSModule Module { get; set; } = default!;

        [Parameter]
        public override bool Visible
        {
            get => base.Visible;
            set => SetVisible(value);
        }

        [Parameter]
        public string? Src { get; set; }

        protected async Task BeforeShowContent()
        {
            if (!isInitialized)
            {
                await Module.Init($"#{id}");
                isInitialized = true;
            }
        }

        private async void SetVisible(bool value)
        {
            if (base.Visible == value)
            {
                return;
            }

            base.Visible = value;
            if (!value && Module is not null && isInitialized)
            {
                // Cannot be placed in BeforeShowContent, Because you will see the Reset animation
                await Module.Reset($"#{id}");
            }
        }

        private async Task SaveToLocal()
        {
            if (string.IsNullOrEmpty(Src))
            {
                await PopupServiceHelper.Error(I18n.T("Image.Not exist"));
                return;
            }

            await MediaResourceManager.SaveImageAsync(Src);
        }

        private async Task Share()
        {
            if (string.IsNullOrEmpty(Src))
            {
                await PopupServiceHelper.Error(I18n.T("Image.Not exist"));
                return;
            }

            var isSuccess = await MediaResourceManager.ShareImageAsync(I18n.T("Share.Share"), Src);
            if (isSuccess)
            {
                await HandleAchievements(Achievement.Share);
            }
        }
    }
}
