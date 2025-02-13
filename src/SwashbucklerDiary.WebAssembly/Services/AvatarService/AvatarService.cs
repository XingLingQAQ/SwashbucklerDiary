﻿using SwashbucklerDiary.Rcl.Essentials;
using SwashbucklerDiary.Rcl.Services;

namespace SwashbucklerDiary.WebAssembly.Services
{
    public class AvatarService : Rcl.Services.AvatarService
    {
        public AvatarService(ISettingService settingService,
            IMediaResourceManager mediaResourceManager,
            IPlatformIntegration platformIntegration,
            II18nService i18n,
            IPopupServiceHelper popupServiceHelper,
            IAppFileSystem appFileSystem)
            : base(settingService, mediaResourceManager, platformIntegration, i18n, popupServiceHelper, appFileSystem)
        {
        }

        public override async Task<string> SetAvatarByCapture()
        {
            await _popupServiceHelper.Error(_i18n.T("Share.NotSupported"));
            return string.Empty;

            //bool isCaptureSupported = await _platformIntegration.IsCaptureSupported();
            //if (!isCaptureSupported)
            //{
            //    await _popupServiceHelper.Error(_i18n.T("User.NoCapture"));
            //    return string.Empty;
            //}

            //string? photoPath = await _platformIntegration.CapturePhotoAsync();
            //if (string.IsNullOrEmpty(photoPath))
            //{
            //    return string.Empty;
            //}

            //return await SetAvatar(photoPath);
        }
    }
}
