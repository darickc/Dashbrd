using Microsoft.AspNetCore.Components;

namespace Dashbrd.Shared
{
    public partial class Layout
    {
        [Parameter] public RenderFragment FullscreenBelow { get; set; }
        [Parameter] public RenderFragment TopBar { get; set; }
        [Parameter] public RenderFragment TopLeft { get; set; }
        [Parameter] public RenderFragment TopCenter { get; set; }
        [Parameter] public RenderFragment TopRight { get; set; }
        [Parameter] public RenderFragment UpperThird { get; set; }
        [Parameter] public RenderFragment MiddleCenter { get; set; }
        [Parameter] public RenderFragment LowerThird { get; set; }
        [Parameter] public RenderFragment BottomBar { get; set; }
        [Parameter] public RenderFragment BottomLeft { get; set; }
        [Parameter] public RenderFragment BottomCenter { get; set; }
        [Parameter] public RenderFragment BottomRight { get; set; }
        [Parameter] public RenderFragment FullscreenAbove { get; set; }
    }
}
