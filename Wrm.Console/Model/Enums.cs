namespace Wrm.Model.Enums
{
    public enum WebResourceComponentState
    {
        Published = 0,
        Unpublished = 1,
        Deleted = 2,
        DeletedUnpublished = 3
    }

    public static class WebResourceIsManaged
    {
        public const bool Unmanaged = false;
        public const bool Managed = true;
    }

    public enum WebResourceType
    {
        Html = 1,
        Css = 2,
        Script = 3,
        Xml = 4,
        Png = 5,
        Jpg = 6,
        Gif = 7,
        Xap = 8,
        Xsl = 9,
        Ico = 10
    }
}
