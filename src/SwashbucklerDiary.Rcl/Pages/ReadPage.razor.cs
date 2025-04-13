using Microsoft.AspNetCore.Components;
using SwashbucklerDiary.Rcl.Components;
using SwashbucklerDiary.Rcl.Essentials;
using SwashbucklerDiary.Rcl.Extensions;
using SwashbucklerDiary.Rcl.Models;
using SwashbucklerDiary.Rcl.Services;
using SwashbucklerDiary.Shared;

namespace SwashbucklerDiary.Rcl.Pages
{
    public partial class ReadPage : ImportantComponentBase
    {
        private bool showDelete;

        private bool showMenu;

        private bool showShare;

        private bool showExport;

        private bool enableMarkdown;

        private bool showSetPrivacy;

        private bool showHighlightSearch;

        private bool? showMoblieOutline = false;

        private bool privacyMode;

        private bool firstLineIndent;

        private bool taskListLineThrough;

        private bool outline;

        private bool rightOutline;

        private bool afterFirstQuery;

        private bool highlightSearchAutofocus = true;

        private readonly string scrollContainerId = $"scroll-container-{Guid.NewGuid():N}";

        private string? search;

        private string scrollContainerSelector = string.Empty;

        private string? urlScheme;

        private DiaryModel diary = new();

        private MarkdownPreview markdownPreview = default!;

        private List<DynamicListItem> menuItems = [];

        private List<DynamicListItem> shareItems = [];

        private List<DiaryModel> exportDiaries = [];

        private readonly string highlightSearchContainerClass = $"search-{Guid.NewGuid():N}";

        [Inject]
        private IDiaryService DiaryService { get; set; } = default!;

        [Inject]
        private IGlobalConfiguration GlobalConfiguration { get; set; } = default!;

        [Inject]
        private IScreenshot ScreenshotService { get; set; } = default!;

        [Inject]
        private MasaBlazorHelper MasaBlazorHelper { get; set; } = default!;

        [Parameter]
        public Guid Id { get; set; }

        [SupplyParameterFromQuery]
        public string? Query { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            scrollContainerSelector = $"#{scrollContainerId}";
            LoadView();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                await UpdateDiary();
                StateHasChanged();
                if (!enableMarkdown)
                {
                    await HandleFirstQuery();
                }
            }
        }

        protected override void ReadSettings()
        {
            base.ReadSettings();

            enableMarkdown = SettingService.Get(s => s.Markdown);
            showSetPrivacy = SettingService.Get(s => s.SetPrivacyDiary);
            firstLineIndent = SettingService.Get(s => s.FirstLineIndent);
            taskListLineThrough = SettingService.Get(s => s.TaskListLineThrough);
            outline = SettingService.Get(s => s.Outline);
            rightOutline = SettingService.Get(s => s.RigthOutline);
            urlScheme = SettingService.Get(s => s.UrlScheme);
            privacyMode = SettingService.GetTemp(s => s.PrivacyMode);
        }

        private List<TagModel> Tags => diary.Tags ?? [];

        private bool IsTop => diary.Top;

        private bool IsPrivate => privacyMode;

        private bool ShowTitle => !string.IsNullOrEmpty(diary.Title);

        private bool ShowWeather => !string.IsNullOrEmpty(diary.Weather);

        private bool ShowMood => !string.IsNullOrEmpty(diary.Mood);

        private bool ShowLocation => !string.IsNullOrEmpty(diary.Location);

        private bool ShowAppBar => !showHighlightSearch;

        private string? WeatherIcon => GlobalConfiguration.GetWeatherIcon(diary.Weather!);

        private string? MoodIcon => GlobalConfiguration.GetMoodIcon(diary.Mood!);

        private string? WeatherText => I18n.T(diary.Weather);

        private string? MoodText => I18n.T(diary.Mood);

        private string? LocationText => diary.Location;

        private string TopText() => IsTop ? "Cancel top" : "Top";

        private string MarkdownText() => enableMarkdown ? "Text mode" : "Markdown mode";

        private string MarkdownIcon() => enableMarkdown ? "description" : "markdown";

        private string OutlineText() => outline ? "Hide outline" : "Display outline";

        private string PrivateText() => IsPrivate ? "Cancel privacy" : "Set to private";

        private string PrivateIcon() => IsPrivate ? "lock_open" : "lock";

        private async Task UpdateDiary()
        {
            var diary = await DiaryService.FindAsync(Id);
            if (diary is null)
            {
                await NavigateToBack();
                return;
            }

            this.diary = diary;
        }

        private void LoadView()
        {
            menuItems =
            [
                new(this, "Copy","content_copy", OnCopy),
                new(this, TopText,"vertical_align_top", OnTopping),
                new(this, "Export","mdi:mdi-export", OpenExportDialog),
                new(this, MarkdownText,MarkdownIcon, MarkdownChanged),
                new(this, "Copy reference", "format_quote", CopyReference),
                new(this, "Copy link", "mdi:mdi-link-variant", CopyLink),
                new(this, "Look up", "quick_reference_all", OpenSearch),
                new(this, "View referenced", "file_export", ViewReferenced),
                new(this, "Outline", "format_list_bulleted", ()=>showMoblieOutline=true, ()=>!MasaBlazorHelper.Breakpoint.MdAndUp),
                new(this, OutlineText, "format_list_bulleted", OutlineChanged, ()=>MasaBlazorHelper.Breakpoint.MdAndUp),
                new(this, PrivateText, PrivateIcon, DiaryPrivacyChanged,()=>privacyMode || showSetPrivacy)
            ];

            shareItems =
            [
                new(this, "Text sharing","description", ShareText),
                new(this, "Photo sharing","image", ShareImage),
            ];
        }

        private void OpenDeleteDialog()
        {
            showDelete = true;
            StateHasChanged();
        }

        private async Task HandleDelete()
        {
            showDelete = false;
            bool flag = await DiaryService.DeleteAsync(diary);
            if (flag)
            {
                await PopupServiceHelper.Success(I18n.T("Delete successfully"));
                await NavigateToBack();
            }
            else
            {
                await PopupServiceHelper.Error(I18n.T("Delete failed"));
            }
        }

        private void ToWrite()
        {
            To($"write?DiaryId={Id}", false);
        }

        private async Task OnTopping()
        {
            diary.Top = !diary.Top;
            diary.UpdateTime = DateTime.Now;
            await DiaryService.UpdateAsync(diary, it => new { it.Top, it.UpdateTime });
            StateHasChanged();
        }

        private async Task OnCopy()
        {
            var content = diary.CreateCopyContent();
            await PlatformIntegration.SetClipboardAsync(content);
            await PopupServiceHelper.Success(I18n.T("Copy successfully"));
        }

        private async Task ShareText()
        {
            var content = diary.CreateCopyContent();
            await PlatformIntegration.ShareTextAsync(I18n.T("Share"), content);
            await HandleAchievements(Achievement.Share);
        }

        private async void ShareImage()
        {
            await PopupServiceHelper.StartLoading();

            if (enableMarkdown && markdownPreview is not null)
            {
                await markdownPreview.RenderLazyLoadingImage();
            }

            var filePath = await ScreenshotService.CaptureAsync("#screenshot");
            await PlatformIntegration.ShareFileAsync(I18n.T("Share"), filePath);

            await PopupServiceHelper.StopLoading();
            await InvokeAsync(StateHasChanged);

            await HandleAchievements(Achievement.Share);
        }

        private async Task MarkdownChanged()
        {
            enableMarkdown = !enableMarkdown;
            await SettingService.SetAsync(s => s.Markdown, enableMarkdown);
            StateHasChanged();
        }

        private async Task OutlineChanged()
        {
            outline = !outline;
            await SettingService.SetAsync(s => s.Outline, outline);
            StateHasChanged();
        }

        private async Task DiaryPrivacyChanged()
        {
            await DiaryService.MovePrivacyDiaryAsync(diary, !privacyMode);
            if (privacyMode)
            {
                await PopupServiceHelper.Success(I18n.T("Removed from privacy mode"));
            }
            else
            {
                await PopupServiceHelper.Success(I18n.T("Moved to privacy mode"));
            }
        }

        private string CounterValue()
        {
            return $"{diary.GetWordCount()} {I18n.T("Word count unit")}";
        }

        private async Task OpenExportDialog()
        {
            var diary = await DiaryService.FindAsync(this.diary.Id); ;
            exportDiaries = [diary];
            showExport = true;
            StateHasChanged();
        }

        private async Task CopyReference()
        {
            var text = diary.GetReferenceText(I18n);
            await PlatformIntegration.SetClipboardAsync(text);
            await PopupServiceHelper.Success(I18n.T("Copy successfully"));
        }

        private async Task CopyLink()
        {
            string text;
            if (PlatformIntegration.CurrentPlatform == AppDevicePlatform.Browser)
            {
                text = NavigationManager.ToAbsoluteUri($"read/{Id}").ToString();
            }
            else
            {
                text = $"{urlScheme}://read/{Id}";
            }

            await PlatformIntegration.SetClipboardAsync(text);
            await PopupServiceHelper.Success(I18n.T("Copy successfully"));
        }

        private async Task HandleFirstQuery()
        {
            if (afterFirstQuery) return;
            afterFirstQuery = true;
            if (string.IsNullOrWhiteSpace(Query)) return;

            await Task.Delay(500);
            search = Query;
            highlightSearchAutofocus = false;
            showHighlightSearch = true;
            StateHasChanged();
        }

        private void OpenSearch()
        {
            highlightSearchAutofocus = true;
            showHighlightSearch = true;
        }

        private async Task ViewReferenced()
        {
            string referencedText = $"read/{diary.Id}";
            var exist = await DiaryService.AnyAsync(it => (it.Content ?? string.Empty).Contains(referencedText ?? string.Empty, StringComparison.CurrentCultureIgnoreCase));
            if (!exist)
            {
                if (diary.Template)
                {
                    await PopupServiceHelper.Info(I18n.T("This template is not referenced"));
                }
                else
                {
                    await PopupServiceHelper.Info(I18n.T("This diary is not referenced"));
                }
            }
            else
            {
                To($"referencedDetails/{diary.Id}");
            }
        }
    }
}
