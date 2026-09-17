namespace HackF5.UnitySpy.Gui.ViewModels
{
    public sealed class BreadcrumbItem
    {
        public BreadcrumbItem(string label, string path, bool isLast)
        {
            this.Label = label;
            this.Path = path;
            this.IsLast = isLast;
        }

        public string Label { get; }

        public string Path { get; }

        public bool IsLast { get; }
    }
}
