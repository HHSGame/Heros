namespace HHSGame.Core
{
    /// <summary>
    /// 平台无关的应用程序宿主接口，替代 Terminal.Gui.Application 的直接依赖。
    /// </summary>
    public interface IApplicationHost
    {
        bool IsInitialized { get; }
        void LayoutAndDraw(bool forceDraw);
    }
}
